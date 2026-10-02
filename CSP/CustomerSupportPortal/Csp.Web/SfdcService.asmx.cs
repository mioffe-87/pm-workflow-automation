using Csp.Api.Logic;
using Csp.Api.Logic.Salesforce;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.Persistence;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.Services.Protocols;
using Csp.Api.HelperClasses;
using Csp.Web.HelperClasses;
using NHibernate.Hql.Ast.ANTLR.Tree;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Web
{
    /// <summary>
    /// Summary description for SfdcService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SfdcService : System.Web.Services.WebService
    {
        public AuthHeader Authentication;

        [SoapHeader("Authentication")]
        [WebMethod]
        public SyncMembershipsResult SyncMembershipsWithSfdc()
        {
            var syncMembershipsResult = new SyncMembershipsResult
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                syncMembershipsResult.IsSuccess = false;
                syncMembershipsResult.ErrorMessage = "Access Denied";

                return syncMembershipsResult;
            }

            try
            {
                var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
                var membershipLogic = DependencyResolver.Current.GetService<IMembershipLogic>();
                var salesforceService = DependencyResolver.Current.GetService<ISalesforceService>();

                var memberships = membershipLogic.GetAllMembershipsWithoutSfdcContact();

                if (!CronJobHelper.IsJobPending(CronJobEnum.SfdcAddUpdateContact))
                {
                    CronJobHelper.QueueJob(CronJobEnum.SfdcAddUpdateContact);

                    foreach (var membership in memberships)
                    {
                        var userAccount = new UserAccount();

                        try
                        {
                            userAccount = userAccountLogic.GetUserAccount(membership.UserAccountId, systemUserAccountId);
                        }
                        catch
                        {
                            continue;
                        }
                        salesforceService.AddUpdateSalesforceContact(userAccount, membership.SupportAccountId,
                            membership.SalesforceAccountId, membership.MembershipId, systemUserAccountId);
                    }
                    CronJobHelper.UpdateJobStatus(CronJobEnum.SfdcAddUpdateContact, CronJobStatus.Success);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to SyncMembershipsWithSfdc();";

                syncMembershipsResult.IsSuccess = false;
                syncMembershipsResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.SfdcAddUpdateContact, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return syncMembershipsResult;
        }
    }

    [Serializable]
    public class SyncMembershipsResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}
