using Csp.Web.HelperClasses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.Services.Protocols;
using Csp.Api.HelperClasses;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Web
{
    /// <summary>
    /// Summary description for ScheduledJobsService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ScheduledJobsService : System.Web.Services.WebService
    {
        public AuthHeader Authentication { get; set; }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BulkRegistrationProcessResult ProcessBulkRegistrations()
        {
            var processResult = new BulkRegistrationProcessResult
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if(!CronJobHelper.IsJobPending(CronJobEnum.BulkRegistration))
                {
                    CronJobHelper.QueueJob(CronJobEnum.BulkRegistration);
                    var bulkRegistrationService = DependencyResolver.Current.GetService<IBulkRegistrationService>();
                    bulkRegistrationService.Process();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.BulkRegistration, CronJobStatus.Success);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessBulkRegistrations();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.BulkRegistration, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }


        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessAscScorecardReminder()
        {
            var processResult = new BaseJobResult
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                var ascScoreCardLogic = DependencyResolver.Current.GetService<IAscScoreCardLogic>();

                ascScoreCardLogic.AscReminderEmail();
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessAscScorecardReminder();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessAutofocusTrialExpiration()
        {
            var processResult = new BaseJobResult
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.AutofocusTrialExp))
                {
                    CronJobHelper.QueueJob(CronJobEnum.AutofocusTrialExp);
                    var securityLogic = DependencyResolver.Current.GetService<ISecurityLogic>();
                    securityLogic.ProcessAutofocusTrialsExpiringToday();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.AutofocusTrialExp, CronJobStatus.Success);
                }

            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessAutofocusTrialExpiration();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.AutofocusTrialExp, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }


        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessEvalAccounts()
        {
            var processResult = new BaseJobResult
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.EvalAccountCreation))
                {
                    CronJobHelper.QueueJob(CronJobEnum.EvalAccountCreation);
                    var evalAccountCreationService = DependencyResolver.Current.GetService<IEvalAccountCreationService>();
                    evalAccountCreationService.CreateEvalAccountsForVmEvals();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.EvalAccountCreation, CronJobStatus.Success);
                }
               
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessEvalAccounts();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.EvalAccountCreation, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessVmRegistrations()
        {
            var processResult = new BaseJobResult
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.VmRegistration))
                {
                    CronJobHelper.QueueJob(CronJobEnum.VmRegistration);
                    var vmRegistrationService = DependencyResolver.Current.GetService<IVmRegistrationService>();
                    vmRegistrationService.CorrectVmRegistrationData(EVmType.Vm);
                    CronJobHelper.UpdateJobStatus(CronJobEnum.VmRegistration, CronJobStatus.Success);
                }

               
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessVmRegistrations();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.VmRegistration, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessMfaRegistrations()
        {
            var processResult = new BaseJobResult()
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.MfaRegistration))
                {
                    CronJobHelper.QueueJob(CronJobEnum.MfaRegistration);
                    var mfaLogic = DependencyResolver.Current.GetService<IMfaLogic>();
                    mfaLogic.ProcessPendingMfaUserRegistrations();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.MfaRegistration, CronJobStatus.Success);
                }
              
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessPendingMfaUserRegistrations();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.MfaRegistration, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessEmailNotificationStatusUpdate()

        {
            var processResult = new BaseJobResult()
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.EmailNotifications))
                {
                    CronJobHelper.QueueJob(CronJobEnum.EmailNotifications);
                    var emailnotificationLogic = DependencyResolver.Current.GetService<IEmailNotificationLogic>();
                    emailnotificationLogic.UpdatePendingEmailNotificationStatus();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.EmailNotifications, CronJobStatus.Success);
                }
              
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to UpdatePendingEmailNotificationStatus();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.EmailNotifications, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;

        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult NotifyLicensingApiExpiration()
        {
            var processResult = new BaseJobResult()
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.LicensingApiNotification))
                {
                    CronJobHelper.QueueJob(CronJobEnum.LicensingApiNotification);
                    EmailHelper.SendLicensingKeyExpiryNotification();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.LicensingApiNotification, CronJobStatus.Success);
                }
               
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to UpdatePendingEmailNotificationStatus();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.LicensingApiNotification, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessBulkDeviceTransfer()
        {
            var processResult = new BaseJobResult()
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.BulkDeviceTransfer))
                {
                    CronJobHelper.QueueJob(CronJobEnum.BulkDeviceTransfer);
                    var deviceTransferLogic = DependencyResolver.Current.GetService<IDeviceTransferLogic>();

                    deviceTransferLogic.ProcessBulkDeviceTransfer();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.BulkDeviceTransfer, CronJobStatus.Success);
                }
               
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessBulkDeviceTransfer();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.BulkDeviceTransfer, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessVmElaTokenNotification()
        {
            var processResult = new BaseJobResult()
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.VmElaTokenNotification))
                {
                    CronJobHelper.QueueJob(CronJobEnum.VmElaTokenNotification);
                    var siteLicenseLogic = DependencyResolver.Current.GetService<ISiteLicenseLogic>();

                    siteLicenseLogic.SendVmElaTokenUsageNotification();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.VmElaTokenNotification, CronJobStatus.Success);
                }
               
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessVmElaTokenNotification();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.VmElaTokenNotification, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessCloudServicesUsersTransfer()
        {
            var processResult = new BaseJobResult()
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.CloudServiceUserTransfer))
                {
                    CronJobHelper.QueueJob(CronJobEnum.CloudServiceUserTransfer);
                    var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
                    userAccountLogic.ProcessCloudServiceUsersTransfer();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.CloudServiceUserTransfer, CronJobStatus.Success);
                }
               
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessCloudServicesUsersTransfer();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.CloudServiceUserTransfer, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessBulkOtpRequest()
        {
            var processResult = new BaseJobResult()
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.BulkOtpRequest))
                {
                    CronJobHelper.QueueJob(CronJobEnum.BulkOtpRequest);
                    var otpLogic = DependencyResolver.Current.GetService<IOtpLogic>();
                    otpLogic.ProcessPendingBulkOtpRequests();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.BulkOtpRequest, CronJobStatus.Success);
                }
               
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessBulkOtpRequest();";

                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.BulkOtpRequest, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessOtpExpirationRequest()
        {
            var processResult = new BaseJobResult()
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.OtpExpirationRequest))
                {
                    CronJobHelper.QueueJob(CronJobEnum.OtpExpirationRequest);
                    var otpLogic = DependencyResolver.Current.GetService<IOtpLogic>();
                    otpLogic.ProcessOtpExpirationRequest(systemUserAccountId);
                    CronJobHelper.UpdateJobStatus(CronJobEnum.OtpExpirationRequest, CronJobStatus.Success);
                }
                
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessOtpExpirationRequest();";
                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.OtpExpirationRequest, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }

        [SoapHeader("Authentication")]
        [WebMethod]
        public BaseJobResult ProcessPinExpirationRequest()
        {
            var processResult = new BaseJobResult()
            {
                IsSuccess = true
            };

            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);

            if (!UserHelper.IsValidServiceAccount(Authentication.Username, Authentication.Password))
            {
                processResult.IsSuccess = false;
                processResult.ErrorMessage = "Access Denied";

                return processResult;
            }

            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.PinExpirationRequest))
                {
                    CronJobHelper.QueueJob(CronJobEnum.PinExpirationRequest);
                    var otpLogic = DependencyResolver.Current.GetService<IOtpLogic>();
                    otpLogic.ProcessPinExpirationRequest(systemUserAccountId);
                    CronJobHelper.UpdateJobStatus(CronJobEnum.PinExpirationRequest, CronJobStatus.Success);
                }
              
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ProcessPinExpirationRequest();";
                processResult.IsSuccess = false;
                processResult.ErrorMessage = string.Format("{0} {1}", errorMsg, ex.Message);
                CronJobHelper.UpdateJobStatus(CronJobEnum.PinExpirationRequest, CronJobStatus.Failed);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

            }

            return processResult;
        }
      
    }

    [Serializable]
    public class BulkRegistrationProcessResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    [Serializable]
    public class BaseJobResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}
