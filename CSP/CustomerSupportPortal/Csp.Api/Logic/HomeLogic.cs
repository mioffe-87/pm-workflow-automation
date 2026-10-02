using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using LINQtoCSV;

namespace Csp.Api.Logic
{

    public class HomeLogic : IHomeLogic
    {

        private readonly IResourceLinkRepository _resourceLinkRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ICurrentStateService _currentStateService;
        private readonly ISecurityRepository _securityRepository;
        private readonly ISupportAccountLogic _supportAccountLogic;
        private readonly ILookupRepository _lookupRepository;
        private readonly IUserProfileDataAccess _userpoProfileDataAccess;
        private readonly ISecurityLogic _securityLogic;

        public HomeLogic(IResourceLinkRepository resourceLinkRepository, IMembershipRepository membershipRepository, IUserAccountRepository userAccountRepository, ISecurityRepository securityRepository, 
            ICurrentStateService currentStateService, ISupportAccountLogic supportAccountLogic, ILookupRepository lookupRepository,IUserProfileDataAccess userProfileDataAccess,ISecurityLogic securityLogic)
        {
            _resourceLinkRepository = resourceLinkRepository;
            _membershipRepository = membershipRepository;
            _userAccountRepository = userAccountRepository;
            _securityRepository = securityRepository;
            _currentStateService = currentStateService;
            _supportAccountLogic = supportAccountLogic;
            _lookupRepository = lookupRepository;
            _userpoProfileDataAccess = userProfileDataAccess;
            _securityLogic = securityLogic;
        }

        public IEnumerable<ResourceLink> GetResourceLinks(bool hasPermViewSoftwareUpdates, bool hasPermViewDynamicUpdates, bool hasSupportAccountSelected, bool hasPermCaseManagement, 
                                                          bool canSeeAutoFocusLink, bool hasViewASCDashboardReport, bool hasReturnsPortalAccess, bool hasPartnerReportAccess, long UserAccountId,long supportAccountId)
        {
            List<ResourceLink> links = _resourceLinkRepository.GetResourceLinks().ToList();

            //HACK - Move these hardcoded perm fields into the table and pass perm requirements back in the result set so the view can filter
            if (!hasPermViewSoftwareUpdates)
            {
                links.Remove(links.Single(x => x.ResourceId == ResourceLinkType.SoftwareUpdates.ToString()));
            }
            if (!hasPermViewDynamicUpdates)
            {
                links.Remove(links.Single(x => x.ResourceId == ResourceLinkType.DynamicUpdates.ToString()));
            }
            if (!hasSupportAccountSelected)
            {
                links.Remove(links.Single(x => x.ResourceId == ResourceLinkType.CaseManagement.ToString()));
            }
            else if (!hasPermCaseManagement)
            {
                var caseMgmtLink = links.Single(x => x.ResourceId == ResourceLinkType.CaseManagement.ToString());
                caseMgmtLink.Url = "javascript: ShowCaseManagementWarningWindow();";
            }

            //Add support account url to 2 key links
            var link = links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.SoftwareUpdates.ToString());
            if (link != null)
            {
							link.Url += "/" + (supportAccountId).ToString();
            }

            link = links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.DynamicUpdates.ToString());
            if (link != null)
            {
							link.Url += "/" + (supportAccountId).ToString();
            }

            if (!canSeeAutoFocusLink)
            {
                var trLink = links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.AutoFocus.ToString());
                if (trLink != null)
                {
                    links.Remove(trLink);
                }
            }
						if (_currentStateService != null && supportAccountId >0)
            {
							if (!hasViewASCDashboardReport && !DependencyResolver.Current.GetService<IAscDeviceRegistrationService>().IsMemberOfAsc(UserAccountId, Convert.ToInt64(supportAccountId)))
                {
									var trLink = links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.Reports.ToString());
	                if (trLink != null) {
		                links.Remove(trLink);
	                }
                }
            }
            else
            {
	            var trLink = links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.Reports.ToString());
	            if (trLink != null) {
		            links.Remove(trLink);
	            }
            }

            if (!hasReturnsPortalAccess)
            {
                var ssdPortal = links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.SSDPortal.ToString());
                if (ssdPortal != null)
                {
                    links.Remove(ssdPortal);
                }
            }

            #region Partner Services

            if (hasPartnerReportAccess)
            {
							var roles = _securityRepository.GetRoles(UserAccountId, supportAccountId).Split(',').ToList();

                //Check if ATC only user
                if(roles.All(r => r.Contains("ATC")))
                {
                    links.Remove(links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.WildfirePortal.ToString()));
                    links.Remove(links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.AutoFocus.ToString()));
                    links.Remove(links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.SecurityLifecycleReview.ToString()));
                    links.Remove(links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.SSDPortal.ToString()));
                }
            }
            else
            {
                links.Remove(links.SingleOrDefault(x => x.ResourceId == ResourceLinkType.PartnerServices.ToString()));
            }


            #endregion

            return links;
        }

        public CurrentLoggedInData GetCurrentLoggedInData(long? supportAccountId, long restrictedByUserAccountId)
        {

            MembershipRolesDataAccess _membershipRolesDataAccess= new MembershipRolesDataAccess();

            if (supportAccountId.HasValue)
            {

                var supportAccount = _supportAccountLogic.GetSupportAccount(supportAccountId.Value, restrictedByUserAccountId);

                return new CurrentLoggedInData
                {
                    CurrentAccount = supportAccount.Name,
                   // CurrentRoles = _membershipRepository.GetUserRoles(supportAccountId.Value, restrictedByUserAccountId),
                    DefaultSupportAccountId = _userAccountRepository.GetDefaultAccount(restrictedByUserAccountId),
                    CurrentSupportAccountId = supportAccountId,
                    CurrentRoles = _membershipRolesDataAccess.GetRolesByUserAccountId(restrictedByUserAccountId,supportAccountId ?? 0)
                };
            }
            else
            {

                return new CurrentLoggedInData
                {
                    CurrentAccount = string.Empty,
                    CurrentRoles = new string[0],
                };
            }
        }

        public IEnumerable<SelectableAccount> GetAvailableAccounts(long userAccountId, DataParameters dataParameters, bool setRoles)
        {

            var accounts = _securityRepository.GetAvailableAccounts(userAccountId, dataParameters).Select(x => new SelectableAccount
            {
                SupportAccountId = x.SupportAccountId,
                CompanyName = x.CompanyName,
                Type = x.Type == "Eval" ? "Eval" : (x.Type == "Marketplace" ? "Marketplace": "")                
            }).ToArray();

            if (setRoles)
            {
                foreach (var account in accounts)
                {
                    account.Roles = _securityRepository.GetRoles(userAccountId, account.SupportAccountId);
                }
            }

            return accounts;
        }

        public IEnumerable<SelectableAccount> GetAvailableAccountsForDevice(long userAccountId, DataParameters dataParameters, bool setRoles)
        {


            var supportAccountDataAccess = new SupportAccountDataAccess();
            var ascBklnAccounts = supportAccountDataAccess.GetAscBklnAccounts(_currentStateService.GetSupportAccountId() ?? 0);

            var accounts = _securityRepository.GetAvailableAccountsForDevice(userAccountId, dataParameters, ascBklnAccounts).Select(x => new SelectableAccount
            {
                SupportAccountId = x.SupportAccountId,
                CompanyName = x.CompanyName,
                Type = x.Type == "Eval" ? "Eval" : "",
            }).ToArray();

            if (setRoles)
            {
                foreach (var account in accounts)
                {
                    var roles = _securityRepository.GetRoles(userAccountId, account.SupportAccountId);
                    account.Roles = !ascBklnAccounts.Contains(account)
                        ? (roles.Equals("")
                            ? "ASC"
                            : roles)
                        : "ASC Pending";
                }
            }

            return accounts;
        }

        public byte[] GetAvailableAccountsCsv(long userAccountId, string searchField, string searchValue)
        {

            var dataParameters = new DataParameters { SearchField = searchField, SearchValue = searchValue };

            var data = GetAvailableAccounts(userAccountId, dataParameters, setRoles: true);

            var cc = new CsvContext();
            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                QuoteAllFields = true,
            };

            using (var ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {

                var accountsCsv = Mapper.Map<SupportAccountCsv[]>(data);
                cc.Write(accountsCsv, tw, outputFileDescription);

                tw.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        public byte[] GetAvailableDeviceAccountsCsv(long userAccountId, string searchField, string searchValue)
        {

            var dataParameters = new DataParameters { SearchField = searchField, SearchValue = searchValue };

            var data = GetAvailableAccountsForDevice(userAccountId, dataParameters, setRoles: true);

            var cc = new CsvContext();
            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                QuoteAllFields = true,
            };

            using (var ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {

                var accountsCsv = Mapper.Map<SupportAccountCsv[]>(data);
                cc.Write(accountsCsv, tw, outputFileDescription);

                tw.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        public void SetDefault(long supportAccountId, long userAccountId)
        {

            // 1) Get all support account access
            var matchingAccounts = _securityRepository.GetListOfAccountsUserCanAccess(userAccountId).Where(x => x.SupportAccountId == supportAccountId);
            var hasMembership = matchingAccounts.Any();
            if (!hasMembership) throw new Exception(string.Format("User Account ID: {0} can't set default membership to Support Account ID: {1} - No Membership", userAccountId, supportAccountId));

            _userAccountRepository.SetDefaultAccount(supportAccountId, userAccountId);
        }

        /// <summary>
        /// Tries to get the DefaultSupportAccountId, failing that
        /// it will try to create one if none is set and there is only one membership
        /// for a SupportAccountId
        /// </summary>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public long? GetOrCreateDefaultSupportAccountId(long userAccountId)
        {

            var defaultSupportAccountId = _userAccountRepository.GetDefaultAccount(userAccountId);
            if (defaultSupportAccountId.HasValue)
            {

                // Verify they still have access
							var matchingAccounts = _securityRepository.GetListOfAccountsUserCanAccess(userAccountId).Where(x => x.SupportAccountId == defaultSupportAccountId.Value);
                if (matchingAccounts.Any())
                {
                    return defaultSupportAccountId;
                }
                else
                {
                    defaultSupportAccountId = null;
                }
            }

            //Will return null if there aren't exactly 1 membership and it is of type support account
            var potentialDefaultSupportAccountId = _membershipRepository.GetSingleSupportAccountMembership(userAccountId);
            if (potentialDefaultSupportAccountId.HasValue)
            {
                SetDefault(potentialDefaultSupportAccountId.Value, userAccountId);
            }
            return potentialDefaultSupportAccountId;
        }

        public bool UserHasAccessToAccount(long supportAccountId, long userAccountId, List<RoleSecurity> roleSecurities)
        {

            if (roleSecurities.Any(x => x.SupportAccountId == supportAccountId)) return true;

            //bool canAccess = _securityRepository.CanUserSignInToSupportAccount(userAccountId, supportAccountId);

            ////Is Asc & doesnt have access to account, check in SF if current account is nova customer
            //if (roleSecurities.Any(x => x.SecurityClassification == RoleSecurity.SecurityClassifications.Asc) && !canAccess)
            //{
            //   List<long> customerCspIds=  GetAscCustomerAccountsFromSalesForce(userAccountId);
            //   if (customerCspIds.Any())
            //   {
            //       foreach (var item in customerCspIds)
            //       {
            //           roleSecurities.Add(new RoleSecurity()
            //           {
            //               SupportAccountId = item,
            //               CategoryNum = (int)RoleCategory.SupportAccount,
            //               SecurityClassificationNum = (int)SecurityClassification.Support
            //           });
            //        }
            //       return customerCspIds.Any(x => x == supportAccountId);

            //   }
            //}
            //return canAccess;
            return _securityRepository.CanUserSignInToSupportAccount(userAccountId, supportAccountId);
        }

        public int GetNumberOfAccountsUserCanAccess(long userAccountId)
        {
            return _securityRepository.GetNumberOfAccountsUserCanAccess(userAccountId);
        }

        public bool HasPendingMemberships(long userAccountId)
        {
            return _membershipRepository.HasPendingMemberships(userAccountId);
        }

        public BannerMessage GetBannerMessage()
        {
            return _lookupRepository.GetBannerMessage();
        }

        public string GetSalesforceLoginId(long supportAccountId, long userAccountId)
        {
            return _userpoProfileDataAccess.GetSalesforceLoginId(supportAccountId, userAccountId);
        }

        public bool EnableDayOneConfig(long supportAccountId)
        {
            return _userpoProfileDataAccess.EnableDayOneConfig(supportAccountId);
        }

        private List<long> GetAscCustomerAccountsFromSalesForce(long userAccountId)
        {
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();
            var ascSalesForceAccountIds = userAccountDataAccess.GetAscSalesForceAccountIdsByUserAccountId(userAccountId);

            if (ascSalesForceAccountIds.Any())
            {
                return _securityLogic.GetAscCustomerAccountsFromSalesForce(ascSalesForceAccountIds,userAccountId);
            }

            return new List<long>();
        }
    }
}
