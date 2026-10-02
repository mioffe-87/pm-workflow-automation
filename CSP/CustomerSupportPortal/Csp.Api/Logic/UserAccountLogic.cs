using System;
using System.Configuration;
using System.Text.RegularExpressions;
using Csp.Api.EntityClasses;
using Csp.Api.HelperClasses;
using Csp.Api.LoginService;
using Csp.Api.Model;
using Csp.Api.DataAccess;
using System.Transactions;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using PAN.Common.Core.Persistence;
using Membership = Csp.Api.Model.Membership;
using Newtonsoft.Json;
using static Csp.Api.HelperClasses.WebCommon;
using System.Net.Http;

namespace Csp.Api.Logic
{

    public class UserAccountLogic : BaseLogic, IUserAccountLogic
    {

        private readonly IRepository _repository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ILookupLogic _lookupLogic;
        private readonly ISupportAccountRepository _supportAccountRepository;
        private readonly IMembershipLogic _membershipLogic;
        private readonly ISalesforceService _salesforceService;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserRegistrationRepository _userRegistrationRepository;
        private readonly IEvalTrackerRepository _evalTrackerRepository;
        private readonly ISupportAccountDomainsService _supportAccountDomainsService;
        private readonly IWildfireWipeoutService _wildfireWipeoutService;
        private readonly IEncryptionProvider _encryptionProvider;
        private readonly ILookupRepository _lookupRepository;
        private readonly ILoginServiceLogic _loginServiceLogic;
        private readonly IDeviceRepository _ideviceRepository;
        private readonly ICsspRepository _csspRepository;
        private readonly IMarketplaceAccountLogic _marketplaceAccountLogic;
	    private readonly IPartialShipmentDataAccess _partialShipmentDataAccess;
        private readonly IApplicationSettings _applicationSettings;

        private readonly string user_profile_topic = ConfigurationManager.AppSettings.Get("User_Profile_TopicName");
        private readonly string user_profile_project = ConfigurationManager.AppSettings.Get("User_Profile_ProjectName");

        public UserAccountLogic(IRepository repository, IUserAccountRepository userAccountRepository, ILookupLogic lookupLogic, 
                IMembershipLogic membershipLogic, ISalesforceService salesforceService, IMembershipRepository membershipRepository, 
                IUserRegistrationRepository userRegistrationRepository, IEvalTrackerRepository evalTrackerRepository, 
                ISupportAccountRepository supportAccountRepository, ISupportAccountDomainsService supportAccountDomainsService, 
                IWildfireWipeoutService wildfireWipeoutService, IEncryptionProvider encryptionProvider, ILookupRepository lookupRepository, 
                ILoginServiceLogic loginServiceLogic, IDeviceRepository deviceRepository, ICsspRepository csspRepository,
                IMarketplaceAccountLogic marketplaceAccountLogic, IPartialShipmentDataAccess partialShipmentDataAccess, IApplicationSettings applicationSettings)
        {
            _repository = repository;
            _userAccountRepository = userAccountRepository;
            _lookupLogic = lookupLogic;
            _membershipLogic = membershipLogic;
            _salesforceService = salesforceService;
            _membershipRepository = membershipRepository;
            _userRegistrationRepository = userRegistrationRepository;
            _evalTrackerRepository = evalTrackerRepository;
            _supportAccountRepository = supportAccountRepository;
            _supportAccountDomainsService = supportAccountDomainsService;
            _wildfireWipeoutService = wildfireWipeoutService;
            _encryptionProvider = encryptionProvider;
            _lookupRepository = lookupRepository;
            _loginServiceLogic = loginServiceLogic;
            _ideviceRepository = deviceRepository;
            _csspRepository = csspRepository;
            _marketplaceAccountLogic = marketplaceAccountLogic;
            _partialShipmentDataAccess = partialShipmentDataAccess;
            _applicationSettings = applicationSettings;
        }

        /// <summary>
        /// This Method is used to get User Account based on userAccountId
        /// </summary>
        /// <param name="userAccountId">userAccount Id</param>
        /// <returns>UserAccount</returns>
        public UserAccount GetUserAccount(long userAccountId, long restrictedByUserAccountId)
        {

            UserAccount userAccount = new UserAccount();
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();

            userAccount = userAccountDataAccess.Read(userAccountId, restrictedByUserAccountId);

            return userAccount;
        }

        public void UpdateTemporaryAnnouncement(UserAccount userAccount, long restrictedByUserAccountId)
        {

            var dataAccess = new UserAccountDataAccess();
            var updateUserAccount = dataAccess.Read(userAccount.Id, restrictedByUserAccountId);
            updateUserAccount.IsTempMessageViewed = userAccount.IsTempMessageViewed;
            updateUserAccount.LastUpdateByUserAccountId = userAccount.Id;
            dataAccess.Update(updateUserAccount, restrictedByUserAccountId);
        }

        /// <summary>
        /// This Method is used to get User Account based on email
        /// </summary>
        /// <param name="userAccountId">email</param>
        /// <returns>UserAccount</returns>
        public UserAccount GetUserAccountByEmail(string email)
        {

            UserAccount userAccount = new UserAccount();
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();

            userAccount = userAccountDataAccess.ReadUserByEmailId(email);
            return userAccount;
        }

        /// <summary>
        /// This Method is used to check if the email address already exists
        /// </summary>
        /// <param name="displayName">Display Name</param>
        /// <returns>UserAccount</returns>
        public bool IsEmailExists(string email)
        {
            var userAccountDataAccess = new UserAccountDataAccess();

            return userAccountDataAccess.IsEmailExists(email);
        }

        /// <summary>
        /// This Method is used to check if the display name exists
        /// </summary>
        /// <param name="displayName">Display Name</param>
        /// <returns>UserAccount</returns>
        public bool IsDisplayNameExists(string displayName, long id)
        {
            var userAccountDataAccess = new UserAccountDataAccess();

            return userAccountDataAccess.IsDisplayNameExists(displayName, id);
        }

        /// <summary>
        /// This Method is used to Create UserAccount
        /// </summary>
        /// <param name="userAccount">user Account</param>
        /// <returns>UserAccount</returns>
        public UserAccount CreateUserAccount(UserAccount userAccount)
        {

            UserAccountDataAccess userAccountDataAccess = null;

            using (TransactionScope transactionScope = new TransactionScope())
            {

                userAccountDataAccess = new UserAccountDataAccess();

                userAccount = userAccountDataAccess.Create(userAccount);
                transactionScope.Complete();
            }

            return userAccount;
        }

        /// <summary>
        /// This Method is used to Update UserAccount
        /// </summary>
        /// <param name="userAccount">userAccount</param>
        /// <returns>UserAccount</returns>
        public UserAccount UpdateUserAccount(UserAccount userAccount, long restrictedByUserAccountId, string clientIpAddress, string supportAccountName)
        {

            var userAccountDataAccess = new UserAccountDataAccess();

            userAccount = userAccountDataAccess.Update(userAccount, restrictedByUserAccountId);

            var addressCodes = GetRegionAndCountryCode(userAccount.Address);

            //Password isn't used here for updates (only new accounts) so it's safe to enter anything
            //Nevertheless, we'll generate a strong random password on the off chance that this is a new account (impossible) so that we don't accidentilly create an easily hackable new account
            var meaninglessPassword = GenerateMeaninglessPassword();

            var loginServiceLogic = new LoginServiceLogic();
            loginServiceLogic.Register(userAccount.Email, meaninglessPassword,
                                                                 userAccount.FirstName,
                                                                 userAccount.LastName, supportAccountName,
                                                                 userAccount.Address.Address1,
                                                                 userAccount.Address.Address2,
                                                                 userAccount.Address.City,
                                                                 addressCodes.RegionCode,
                                                                 userAccount.Address.PostalCode,
                                                                 addressCodes.CountryCode, userAccount.PhoneNumber,
                                                                 clientIpAddress);

            UpdateAllSfContactsForUser(userAccount);

            return userAccount;
        }

        private void UpdateAllSfContactsForUser(UserAccount userAccount)
        {
            //Search membership table for user to get ALL unique SF contacts
            var uniqueContacts = _membershipRepository.GetUniqueContactsForSfUpdate(userAccount.Id);

            foreach (var contact in uniqueContacts)
            {
                _salesforceService.AddUpdateSalesforceContact(userAccount, contact.SupportAccountId, contact.SalesforceId, contact.MembershipId, userAccount.Id);
            }
        }

        /// <summary>
        /// Password isn't used here for updates (only new accounts) so it's safe to enter anything
        /// Nevertheless, we'll generate a strong random password on the off chance that this is a new account (impossible) so that we don't accidentilly create an easily hackable new account
        /// </summary>
        /// <returns></returns>
        private string GenerateMeaninglessPassword()
        {
            //var rand = new Random();
            //var meaninglessPassword = rand.Next(999999999).ToString();
            //meaninglessPassword = _encryptionProvider.GetMd5Hash(meaninglessPassword);
            //return meaninglessPassword;
            return System.Web.Security.Membership.GeneratePassword(20, 5);
        }

        public SelfRegistrationCodeModel IsValidSelfRegistrationCode(string code)
        {
            var model = _userRegistrationRepository.GetSelfRegistrationByCode(code);

            if (model == null)
                return null;

            return model;
        }

        /// <summary>
        /// If ErrorMessage is not null or empty then there is an error - DONT READ ANY OTHER VARIABLES
        /// If HasMultipleSupportAccountIds is true, then SapId is provided,
        /// otherwise SupportAccountId is provided
        /// </summary>
        public class ValidateUserRegistrationModel
        {
            public string ErrorMessage { get; set; }
            public bool HasMultipleSupportAccountIds { get; set; }
            public long? SapId { get; set; }
            public long? SupportAccountId { get; set; }
            public bool OfferToEmailSuperUsers { get; set; }
            public bool IsSelfRegistrationCode { get; set; }
	        public bool IsPartialShipmentUser { get; set; }
        }
        public ValidateUserRegistrationModel ValidateUserRegistration(string email, string displayName, string deviceCode,
                                                                                                                             string customerCode, bool showInternalErrors = false)
        {

            var emailInUse = _userRegistrationRepository.IsEmailInUse(email);
            if (emailInUse)
            {
                return new ValidateUserRegistrationModel
                {
                    ErrorMessage = "Email Address already exists.",
                };
            }

            var displayNameInUse = _userRegistrationRepository.IsDisplayNameInUse(displayName);
            if (displayNameInUse)
            {
                return new ValidateUserRegistrationModel
                {
                    ErrorMessage = "Display Name already exists.",
                };
            }

            var guidRegex = new Regex(@"[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.IgnoreCase);
            var mayBeSelfRegistrationCode = deviceCode == customerCode && guidRegex.IsMatch(deviceCode);
            if (mayBeSelfRegistrationCode)
            {
                var model = _userRegistrationRepository.GetSelfRegistrationByCode(deviceCode);

                if (model != null)
                {
                    if (model.IsActiveAndUnexpired)
                    {
                        return new ValidateUserRegistrationModel
                        {
                            HasMultipleSupportAccountIds = false,
                            SupportAccountId = model.SupportAccountId,
                            IsSelfRegistrationCode = true
                        };
                    }
                    else
                    {
                        return new ValidateUserRegistrationModel
                        {
                            ErrorMessage = "This self registration code is no longer active",
                        };
                    }
                }
            }

            var isEvalDevice = IsEvalDeviceCode(deviceCode);

            if (isEvalDevice)
            {
                //Check that the SN and Eval ID match in Eval Tracker or VM Evals

                var evalId = customerCode;

                var evalTrackerData = _evalTrackerRepository.GetSfAccountDataByEvalId(evalId);

                if (evalTrackerData == null)
                {
                    return new ValidateUserRegistrationModel
                    {
                        ErrorMessage = showInternalErrors ? "Can't find Eval ID in CSP Log table 'csp.vmevals'" : "Error: Eval-ID does not exist. Please contact Support",
                    };
                }

                //Also call web service and compare SF Account ID to supportAccountIds
                var sfEvalData = _salesforceService.GetSalesforceEvalDataByEvalId(evalId);

                if (sfEvalData == null || string.IsNullOrEmpty(sfEvalData.SfAccountId))
                {
                    return new ValidateUserRegistrationModel
                    {
                        ErrorMessage = showInternalErrors ? "Salesforce web service can't match Eval ID to a real account/request" : "Error: Eval-ID does not exist. Please contact Support",
                    };
                }

                var sfEvalMatchcesEvalData = DoSalesforceAccountIdsMatch(sfEvalData.SfAccountId, evalTrackerData.SfAccountId);

                //Verify SF Account ID maps to one and only one support account
                var matchingSupportAcccountIds = _userRegistrationRepository.GetSupportAccountIdsBySfAccountId(evalTrackerData.SfAccountId, evalId);
                var isOneSupportAccountIdMatch = matchingSupportAcccountIds.Count() == 1;

                if (!sfEvalMatchcesEvalData || !isOneSupportAccountIdMatch)
                {
                    if (showInternalErrors)
                    {
                        if (!sfEvalMatchcesEvalData)
                        {
                            return new ValidateUserRegistrationModel
                            {
                                ErrorMessage = "logged SFDCID in csp.vmevals does not match value returned from Salesforce service"
                            };
                        }
                        if (!isOneSupportAccountIdMatch)
                        {
                            return new ValidateUserRegistrationModel
                            {
                                ErrorMessage = "Matching SF AccountID corresponds to multiple Support Accounts: " + evalTrackerData.SfAccountId
                            };
                        }
                    }
                    else
                    {
                        return new ValidateUserRegistrationModel
                        {
                            ErrorMessage = "Error: Unable to create user account.  Please contact Support",
                        };
                    }
                }

                //Validate DeviceCode against eval data
                var validDeviceCode = evalTrackerData.DeviceCodes.Contains(deviceCode);
                if (!validDeviceCode)
                {
                    return new ValidateUserRegistrationModel
                    {
                        ErrorMessage = showInternalErrors ? "The Device code was expected to be an eval device, but was not" : "Error: Eval-ID does not match serial number or auth code.  Please contact Support.",
                    };
                }

                return new ValidateUserRegistrationModel
                {
                    HasMultipleSupportAccountIds = false,
                    SupportAccountId = matchingSupportAcccountIds.Single(),
                };

            }

            var isLightCyberMigration = false;
            var devicePartNumber = _ideviceRepository.GetDevicePartNumber(deviceCode);
            if (!string.IsNullOrEmpty(devicePartNumber) && devicePartNumber.StartsWith("PAN-LCP-"))
            {
                isLightCyberMigration = true;
            }

            //Find SupportAccountIds by Device
            var deviceSerialNumberSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsByDeviceSerialNumber(deviceCode);
            var licenseAuthCodeSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsByLicenseAuthCode(deviceCode);
 	       //Find support account by Parent Order Number
	       var parentOrder = _partialShipmentDataAccess.GetMasterOrderDetail(deviceCode.TrimStart('0'));
	       var partialShipmentSupportAccount = parentOrder !=null ? new List<long> {parentOrder.SupportAccountId} : new List<long>();

            var deviceSupportAccountIds = deviceSerialNumberSupportAccountIds.Union(licenseAuthCodeSupportAccountIds).Union(partialShipmentSupportAccount).ToList();
           
            //Find SupportAccountIds by Customer Code
            var sapIdSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsBySapId(customerCode).ToList();
            var salesOrderNumberSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsBySalesOrderNumber(customerCode).ToList();
            long customerId;
            long.TryParse(customerCode, out customerId);
            if (partialShipmentSupportAccount.Any() && (parentOrder.EndCustomerSapId == customerId || parentOrder.SoldToSapId == customerId))
            {
                sapIdSupportAccountIds.Add(parentOrder.SupportAccountId);
            }

            var customerSupportAccountIds = sapIdSupportAccountIds.Union(salesOrderNumberSupportAccountIds).ToList();

            //Find intersecting SupportAccountIds
            if (deviceSupportAccountIds.Any())
            { 
            var intersectingSupportAccountIds = deviceSupportAccountIds.Intersect(customerSupportAccountIds);

            //Light Cyber Migration doesn't have sales order, we need to skip deviceSupportAccountIds
            if (intersectingSupportAccountIds.Count() == 0 && isLightCyberMigration)
            {
                intersectingSupportAccountIds = customerSupportAccountIds;
            }

                switch (intersectingSupportAccountIds.Count())
                {
                    case 0:

                        var anyDevices = deviceSupportAccountIds.Any();
                        var anyCustomerOrders = customerSupportAccountIds.Any();                      
                        //No devices found, but SAP ID looks right
                        if (!anyDevices && anyCustomerOrders)
                        {
                            if (ShowEmailToSuperUserOption(deviceCode, customerCode))
                            {
                                return new ValidateUserRegistrationModel
                                {
                                    OfferToEmailSuperUsers = true,
                                };
                            }
                        }
                        return new ValidateUserRegistrationModel
                        {
                            ErrorMessage = showInternalErrors ? "No support account matches both the device code and customer code" : "Error: unable to create user account.  Please contact Support",
                        };

                    case 1:
                        return new ValidateUserRegistrationModel
                        {
                            HasMultipleSupportAccountIds = false,
                            SupportAccountId = intersectingSupportAccountIds.Single(),
                            IsPartialShipmentUser = parentOrder != null
                        };

                    //Multiple matches
                    default:

                        long sapId;

                        if (sapIdSupportAccountIds.Any())
                        {
                            sapId = Convert.ToInt64(customerCode);
                        }
                        else if (salesOrderNumberSupportAccountIds.Any())
                        {
                            var sapIds = _userRegistrationRepository.GetSalesOrderSapIdsBySalesOrderNumber(customerCode);
                            if (sapIds.Count() == 1)
                            {
                                sapId = sapIds.Single();
                            }
                            else
                            {
                                throw new Exception(string.Format("Too Many SAPIds Email: {0} Device Code: {1} Customer Code: {2}", email, deviceCode, customerCode));
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("User registration impossible condition! Email: {0} Device Code: {1} Customer Code: {2}", email, deviceCode, customerCode));
                        }

                        return new ValidateUserRegistrationModel
                        {
                            HasMultipleSupportAccountIds = true,
                            SapId = sapId,
                            ErrorMessage = showInternalErrors ? "Multiple Support Account IDs match the device code and customer code for this SAP ID: " + sapId : null,
                        };
                }
             
            }
            return new ValidateUserRegistrationModel
            {
                ErrorMessage = showInternalErrors ? "No support account matches both the device code and customer code" : "Error: unable to create user account.  Please contact Support",
            };
        }

        private bool DoSalesforceAccountIdsMatch(string sfAccountId, string otherSfAccountId)
        {

            //if either are null or empty return false;
            if (string.IsNullOrEmpty(sfAccountId) || string.IsNullOrEmpty(otherSfAccountId))
            {
                return false;
            }

            //If they already match, return true
            if (sfAccountId == otherSfAccountId)
            {
                return true;
            }

            //Otherwise, try shortening to 15 chars then comparing

            var shortSfId = sfAccountId.Length > 15 ? sfAccountId.Substring(0, 15) : sfAccountId;

            var shortOtherSfId = otherSfAccountId.Length > 15 ? otherSfAccountId.Substring(0, 15) : otherSfAccountId;

            return shortSfId == shortOtherSfId;
        }

        public bool SupportAccountRequiresSuperUserApproval(long supportAccountId)
        {
            var supportAccountReqiresApproval = _supportAccountRepository.RegistrationRequiresSuApproval(supportAccountId);

            return supportAccountReqiresApproval;
        }

        public UserProfile RegisterUserAccount(UserAccount userAccount, long supportAccountId, string password, string clientIpAddress, out long membershipId, out string errorMessage, out long userAccountId,
                                               bool isMembershipPending = false, bool isAwsRegistration = false, string supportAccountName = "", string cloudAccountId = "",
                                               long deviceId = 0, string deviceCode = "", string csspSerialNumber = "", string customerAccountId = "", bool isUsingCloudDeviceForRegistration = false,
                                               bool isAzureRegistration = false, bool isCsspRegistration = false, bool createMembership = true, bool isGoogleCloudRegistration = false, bool isPartialShipmentUser = false,
                                               bool allowedOrg = true)
        {
            bool isCsspDefaultAccount = false;
            UserProfile userProfile = null;
            membershipId = 0;
            userAccountId = 0;
            errorMessage = string.Empty;
            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["UserToCreateAwsSupportAccount"], out systemUserAccountId);
            string defaultAwsSalesforceAccount = ConfigurationManager.AppSettings["SalesforceDefaultAWSAccountId"].ToString();
            string defaultAzureSalesforceAccount = ConfigurationManager.AppSettings["SalesforceDefaultAZUREAccountId"].ToString();
            string defaultGoogleCloudSFDCAccount = ConfigurationManager.AppSettings["SalesforceDefaultGoogleCloudAccountId"].ToString();
            string defaultCsspSalesforceAccount = "";
            // Get CountryCode/RegionCode incase of US/Canada
            var addressCodes = GetRegionAndCountryCode(userAccount.Address);
            SalesforceUserModel salesforceUserModel = null;
            string domainType = string.Empty;

            bool isPanEmployee = userAccount.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION);

            if (!isUsingCloudDeviceForRegistration)
            {
                _supportAccountDomainsService.CreateFirstSupportAccountDomain(supportAccountId, userAccount.Email);
            }
            else
            {
                if (isCsspRegistration)
                {
                    deviceId = _ideviceRepository.ReadDeviceIdBySerialNumber(csspSerialNumber);
                    //1) Backup in csp.logdevice
                    var logDeviceDataAccess = new LogDeviceDataAccess();
                    logDeviceDataAccess.Log(deviceId, CsspConstants.CsspDeviceTransferLog, 1);
                }
            }
            Supportaccount supportAccount = _supportAccountRepository.GetSupportAccountById(supportAccountId);

            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                try
                {
                    if (isUsingCloudDeviceForRegistration)
                    {
                        string endUserAccount = string.Empty;
                        long csspSupportAccountId = 0;
                        long? csspEndUserSupportAccountId = null;
                        string endUserSFDCAccountId = null;
                        if (isCsspRegistration)
                        {
                            endUserAccount = string.Join("-", customerAccountId.Split('-').Skip(1));
                            csspSupportAccountId = Convert.ToInt64(customerAccountId.Split('-')[0]);
                            //  Check if End User has a Support Account already.
                            _csspRepository.GetEndUserSupportAccount(endUserAccount, csspSupportAccountId, out csspEndUserSupportAccountId, out endUserSFDCAccountId);
                            if (csspEndUserSupportAccountId.HasValue)
                            {
                                supportAccountId = csspEndUserSupportAccountId.Value;
                            }
                            else
                            {
                                supportAccountName = _supportAccountRepository.GetSupportAccountNameForEndUser(endUserAccount);
                            }
                            if (!string.IsNullOrEmpty(endUserSFDCAccountId))
                            {
                                isCsspDefaultAccount = false;
                            }
                            else
                            {
                                defaultCsspSalesforceAccount = _supportAccountRepository.GetDefaultSfdcAccountForCssp(csspSupportAccountId);
                                isCsspDefaultAccount = true;
                            }
                        }

                        if (supportAccountId <= 0)
                        {
                            if (_supportAccountRepository.CreateSupportAccount(supportAccountName, 1,
                                systemUserAccountId,
                                out supportAccountId))
                            {
                                supportAccount = _supportAccountRepository.GetSupportAccountById(supportAccountId);
                                // Set Salesforce accountid here
                                if (isAwsRegistration)
                                {
                                    supportAccount.SalesforceAccountId = defaultAwsSalesforceAccount;
                                }
                                else if (isGoogleCloudRegistration)
                                {
                                    supportAccount.SalesforceAccountId = defaultGoogleCloudSFDCAccount;
                                }
                                else if (isAzureRegistration)
                                {
                                    supportAccount.SalesforceAccountId = defaultAzureSalesforceAccount;
                                }
                                else
                                {
                                    supportAccount.SalesforceAccountId = isCsspDefaultAccount ? defaultCsspSalesforceAccount : endUserSFDCAccountId;
                                    // Update Support Account Id of the end user so that next time for another registration it can be used again.
                                    if (!csspEndUserSupportAccountId.HasValue)
                                        _csspRepository.UpdateSupportAccountIdForCsspEndUser(endUserAccount, csspSupportAccountId,
                                            supportAccountId, supportAccount.SalesforceAccountId);
                                }
                            }
                        }
                        else
                        {
                            supportAccount = _supportAccountRepository.GetSupportAccountById(supportAccountId);
                            isMembershipPending = true; // If Support Account already exists, then membership approval needs to be routed to SuperUser.
                            if (isCsspRegistration)
                            {
                                defaultCsspSalesforceAccount = _supportAccountRepository.GetDefaultSfdcAccountForCssp(csspSupportAccountId);
                                if (supportAccount.SalesforceAccountId == defaultCsspSalesforceAccount)
                                {
                                    isCsspDefaultAccount = true;
                                }
                            }

                        }
                    }
                    else
                    {
                        // if its a aws support account.
                        if (string.IsNullOrEmpty(supportAccount.SalesforceAccountId) && _supportAccountRepository.IsAwsAccount(supportAccountId))
                        {
                            supportAccount.SalesforceAccountId = defaultAwsSalesforceAccount;
                        }
                    }
                    userAccount.DefaultSupportAccountId = supportAccount.SupportAccountId;
                    userAccount = CreateUserAccount(userAccount);

                    if (userAccount != null && userAccount.Id > 0)
                    {
                        if (supportAccountId > 0)
                        {
                            var isMemberExits = CheckMemberExists(supportAccountId);
                            if (!isMemberExits)
                            {
                                var domainDetail = GetDomainData(userAccount.Email.Substring(userAccount.Email.IndexOf('@') + 1));
                                if (domainDetail != null)
                                {
                                    domainType = domainDetail.IsPublic ? "public" : "private";
                                }
                            }
                        }
                        userAccountId = userAccount.Id;

                        // SSO Registration

                        // Don't register/insert pan employee in panlogin
                        if (isPanEmployee)
                        {
                            string key = ConfigurationManager.AppSettings["LoginServicePrivateKey"];
                            userProfile = _loginServiceLogic.GetUserProfile(userAccount.Email, clientIpAddress, key);
                        }
                        else
                        {

                            userProfile = _loginServiceLogic.Register(userAccount.Email, password,
                                                                     userAccount.FirstName ?? string.Empty,
                                                                     userAccount.LastName ?? string.Empty, supportAccount.Name ?? string.Empty,
                                                                     userAccount.Address.Address1 ?? string.Empty,
                                                                     userAccount.Address.Address2 ?? string.Empty,
                                                                     userAccount.Address.City ?? string.Empty,
                                                                     addressCodes.RegionCode ?? string.Empty,
                                                                     userAccount.Address.PostalCode ?? string.Empty,
                                                                     addressCodes.CountryCode ?? string.Empty, userAccount.PhoneNumber ?? string.Empty,
                                                                     clientIpAddress);
                        }

                        // Create New Membership
                        if (createMembership)
                        {
                            var membership = new Membership()
                            {
                                UserAccount = new UserAccount() { Id = userAccount.Id },
                                SupportAccountId = supportAccountId,
                                ActivationDate = DateTime.Now,
                                Description = "New user created",
                                CreatedOn = DateTime.Now,
                                CreatedByEmail = userAccount.CreatedByEmail,
                                CreatedByUserAccountId = userAccount.CreatedByUserAccountId
                            };

                            // Set Membership expiration to earlier date if the user is associated with more than one support account
                            if (isMembershipPending)
                            {
                                membership.MembershipStatus = (int)MembershipStatus.Pending;
                                membership.MembershipResponseGuid = Guid.NewGuid().ToString();
                                membership.ExpirationDate = DateTime.Now.AddDays(-1);
                            }

                            // CSP-1989 Create RBAC roles on the hub from AdminSite
                            // Logic is the same as with membership table: if zero reords for 
                            // the Support Account in RBAC source- create first RBAC super user.
                            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();
                            IEnumerable<RbackUserRole> destinationRoles = userAccountDataAccess.GetRbacRolesForUser(null, supportAccountId);
                            if (!destinationRoles.Any())
                            {
                                userAccountDataAccess.RegisterAdminRbacRoleForUser(userAccount.Email, supportAccountId);
                            }

                            // Comments By LK On 09/26/2016
                            // To check if the user is a panw employee, so we can ignore the threat researcher use count validation
                            membership.IsValidateThreatResearcherUseCount =
                                !userAccount.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION) ? true : false;

                            membership = _membershipLogic.CreateMembership(membership);
                            membershipId = membership.Id;

                            // Assign Membership Roles

                            _membershipLogic.AssignMembershipRoles(membershipId, userAccountId, supportAccountId, userAccount.CreatedByUserAccountId, isPartialShipmentUser, allowedOrg);
                            //Publish event to User Profile   
                            #region Publish event to User Profile                        
                            var localmembership = _membershipLogic.GetMembership(membershipId, userAccountId);
                            if (localmembership != null)
                            {                               
                                if (localmembership.Roles.Any(r => r.Id == (long)RoleType.DomainAdministrator))
                                {                                   
                                   var customdata = new UserProfileEvent()
                                    {
                                        eventType = "ROLE_CHANGE",
                                        email = userAccount.Email,
                                        domainAdmin = true
                                    };
                                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(JsonConvert.SerializeObject(customdata)));
                                    EventPublisher eventPublisher = new EventPublisher();
                                    eventPublisher.PublishCustomEvent(customdata, user_profile_project, user_profile_topic);
                                }
                            }
                            #endregion
                            //After the membership is created, set the default support account id (must be done AFTER membership exists)
                            UpdateSupportAccountForUser(userAccount.Id, supportAccountId);
                            CreateJiveId(userAccount.Id, userAccount.Email);
                            UpdateSelectedMembershipAndSalesForce(userAccount.Id, membershipId, supportAccountId);

                            if (userProfile != null)
                            {
                                // Salesforce Contact Creation

                                var salesforceId = supportAccount.SalesforceAccountId;
                                salesforceUserModel = _salesforceService.AddUpdateSalesforceContact(userAccount, supportAccountId, salesforceId, membershipId,
                                                                                                                            userAccount.CreatedByUserAccountId, isCsspDefaultAccount);
                            }
                        }

                        // Register aws device to support account.
                        if (isUsingCloudDeviceForRegistration)
                        {

                            if (isAwsRegistration || isGoogleCloudRegistration)
                            {
                                bool result = false;
                                if (isAwsRegistration)
                                {
                                    //cloudAccountId is NULL for AWS GOV cloud before License API validation is done. 
                                    if (!string.IsNullOrEmpty(cloudAccountId) && !_ideviceRepository.IsAwsAccountSupportAccountMappingExists(supportAccountId, cloudAccountId))
                                    {
                                        _ideviceRepository.CreateAwsAccountSupportAccountMapping(supportAccountId, cloudAccountId);
                                    }
                                    result = _ideviceRepository.RegisterCloudDevices(deviceId, supportAccountId, userAccountId, cloudAccountId);
                                }
                                else
                                {
                                    //cloudAccountId is NULL for Google Cloud at present, so use the same API as that for Azure.
                                    result = _ideviceRepository.RegisterAzureDevice(deviceId, supportAccountId, userAccountId);
                                }

                                if (!result)
                                {
                                    Elmah.ErrorSignal.FromCurrentContext()
                                        .Raise(
                                            new Exception(string.Format(
                                                "Cannot register {0} device id {1} to new support account id {2}",
                                                isAwsRegistration ? "AWS" : "Google Cloud", deviceId, supportAccountId)));
                                }
                            }
                            else if (isAzureRegistration)
                            {
                                if (!_ideviceRepository.RegisterAzureDevice(deviceId, supportAccountId, userAccountId))
                                {
                                    Elmah.ErrorSignal.FromCurrentContext()
                                        .Raise(
                                            new Exception(string.Format(
                                                "Cannot register azure device id {0} to new support account id {1}", deviceId,
                                                supportAccountId)));
                                }
                            }
                            else
                            {
                                //Set new supportaccount id
                                _csspRepository.UpdateSupportAccountForProvisionedDevice(deviceId, userAccountId,
                                    supportAccountId);
                            }
                        }
                        else
                        {

                            RegisterCirroSecureDeviceToSupportAccount(deviceCode, supportAccountId, userAccountId);

                        }                   
                        transactionScope.Complete();
                    }
                }
                catch (Exception oex)
                {
                    errorMessage = "An error occurred while processing your request. Please contact customer support.";
                    transactionScope.Dispose();
                    Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
                }
            }

            try
            {
                // Create Marketplace account
                if (salesforceUserModel != null)
                {
                    if (!_marketplaceAccountLogic.IsAccountExists(userAccount.Email))
                    {
                        UserAccount createdByUser = null;

                        if (userAccountId > 0)
                        {
                            createdByUser = GetUserAccount(userAccountId, userAccount.CreatedByUserAccountId);
                        }

                        var marketplaceAccount = new MarketplaceAccount()
                        {
                            Email = userAccount.Email,
                            SalesforceUsername = salesforceUserModel.SalesForceLoginId,
                            SalesforceAccountId = salesforceUserModel.SalesForceAccountId,
                            CreatedBy = (userAccountId > 0 && createdByUser != null)
                                ? createdByUser.Email
                                : ApiConstants.PANW_SYSTEM_USER_EMAIL
                        };

                        _marketplaceAccountLogic.CreateAccount(marketplaceAccount);
                    }
                }

                if (isUsingCloudDeviceForRegistration)
                {
                    if (isAwsRegistration)
                        UpdateDeviceHyperVisorInfo(deviceId, null, ApiConstants.HYPERVISOR_VERSION_AWS);
                    else if (isGoogleCloudRegistration)
                        UpdateDeviceHyperVisorInfo(deviceId, null, ApiConstants.HYPERVISOR_VERSION_GOOGLE);
                    else if (isAzureRegistration)
                        UpdateDeviceHyperVisorInfo(deviceId, null, ApiConstants.HYPERVISOR_VEERSION_AZURE);
                }

                //Partner assertion is CSOD: If email is associated with a Partner account in Salesforce, update cornerstone.accounts user type to PART
                var isPartnerInSF = _salesforceService.getSfAccountTypeByEmail(userAccount.Email);

                if (isPartnerInSF)
                {
                    if (userProfile != null && !userProfile.Email.ToLower().EndsWith("@paloaltonetworks.com"))
                    {
                        UpdateCsodUserType(userProfile.Email, "PART", "CSP Registration");
                    }
                }
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
            }

            if (supportAccountId > 0 && userAccount != null && userAccount.Id > 0 && string.IsNullOrEmpty(errorMessage))
            {
                if (!string.IsNullOrEmpty(domainType))
                {
                    new SupportAccountDataAccess().UpdateSupportAccountEmailDomainType(supportAccountId, domainType);
                }
            }

            return userProfile;
        }

        private bool RegisterCirroSecureDeviceToSupportAccount(string deviceCode, long supportAccountId, long userAccountId)
        {

            DeviceDataAccess deviceDataAccess = null;
            bool isSuccess = false;

            using (TransactionScope transactionScope = new TransactionScope())
            {

                deviceDataAccess = new DeviceDataAccess();

                long deviceId;

                if (deviceDataAccess.IsDeviceCodeCirroSecure(deviceCode, out deviceId))
                {
                    isSuccess = deviceDataAccess.RegisterCirroSecureDevice(deviceId, supportAccountId, userAccountId);
                }

                transactionScope.Complete();
            }

            return isSuccess;
        }

        /// <summary>
        /// This Method is used to Create UserAccount
        /// </summary>
        /// <param name="userAccount">user Account</param>
        /// <returns>UserAccount</returns>
        private bool UpdateSupportAccountForUser(long userAccountID, long supportAccountId)
        {

            UserAccountDataAccess userAccountDataAccess = null;
            bool result = false;
            using (TransactionScope transactionScope = new TransactionScope())
            {

                userAccountDataAccess = new UserAccountDataAccess();

                result = userAccountDataAccess.UpdateSupportAccountForUser(userAccountID, supportAccountId);
                transactionScope.Complete();
            }
            return result;
        }

        private class AddressCodes
        {
            public string RegionCode { get; set; }
            public string CountryCode { get; set; }
        }

        private AddressCodes GetRegionAndCountryCode(Address address)
        {
            var countryId = address.CountryId ?? 0;
            var countryCode = _lookupRepository.GetCountryCodeById(countryId);
            var regionCode = string.Empty;

            if (countryCode == CountryCode.US.ToDescription() ||
                    countryCode == CountryCode.CA.ToDescription())
            {
                long regionId;
                long.TryParse(address.Region, out regionId);

                if (regionId > 0)
                {
                    regionCode = _lookupLogic.GetRegionCodeById(regionId, countryId);
                }
                else if (!string.IsNullOrEmpty(address.Region))
                {
                    regionCode = address.Region;
                }
            }
            else
            {
                regionCode = address.Region;
            }

            return new AddressCodes
            {
                RegionCode = regionCode,
                CountryCode = countryCode,
            };
        }

        public void SetSelectedMembershipAndSalesForce(long userAccountId, long membershipId, long supportAccountId)
        {

            var selectedSalesforceData = _membershipRepository.GetSelectedSalesforceDataByMembership(membershipId);

            _userAccountRepository.SetSelectedSalesfoceData(userAccountId, selectedSalesforceData);
        }

        public void UpdateSelectedMembershipAndSalesForce(long userAccountId, long membershipId, long supportAccountId)
        {
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();
            var selectedSalesforceData = userAccountDataAccess.GetSalesforceInfoByMembershipId(membershipId);
            var userData = userAccountDataAccess.GetSalesforceInfo(userAccountId);

            if (userData.SelectedMembershipId == selectedSalesforceData.MembershipId
                    && userData.SelectedSalesForceAccountId == selectedSalesforceData.SalesForceAccountId
                    && userData.SelectedSalesForceContactId == selectedSalesforceData.SalesForceContactId
                    && userData.SelectedSalesForceLoginId == selectedSalesforceData.SalesForceLoginId) return;

            userAccountDataAccess.SetSelectedSalesforceData(userAccountId, selectedSalesforceData);
        }

        public void CreateJiveId(long userAccountId, string email)
        {
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();
            var configValues = new ConfigValues();

            var jiveId = CalculateMd5Hash(string.Format("{0}{1}{2}", DateTime.Now.ToString(), email,
                                                                                     configValues.GetAppSetting(AppSettings.PaloAltoHomeAddress))); // Unique id for Jive

            userAccountDataAccess.SetJiveId(userAccountId, jiveId);
        }


        public bool IsValidNonPublicEmail(string email)
        {
            var publicDomains = _lookupLogic.GetPublicDomains();
            publicDomains = publicDomains.Replace(".", "\\.");
            // Comments By LK On 09/26/2016
            // Changes to allow special characters in an email address
            //var pattern = @"^[\w\.'-]+@(?!" + publicDomains + @")[\w\.-]+\.\w{2,12}$";
            var pattern = @"^(([\w-'\+!#&amp;$%'*/=?^_`{|}~-]+\.)+[\w-'\+!#&amp;$%'*/=?^_`{|}~-]+|([a-zA-Z]{1}|[\w-'\+!#&amp;$%'*/=?^_`{|}~-]{2,}))@(?!" + publicDomains + @")[\w\.-]+\.\w{2,12}$";

            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        public List<UserAccount> GetAdminUserAccountsBySupportAccountId(long supportAccountId)
        {
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();
            return userAccountDataAccess.GetUsersByRole(supportAccountId, (int)RoleType.SuperUser);
        }
        public bool HasCPSPAdminRole(long userAccountId)
        {
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();
            List<UserAccount> users = userAccountDataAccess.GetUsersByRole(ApiConstants.PANW_SUPPORT_ACCOUNT_ID, (int)RoleType.CPSPAdmin);
            return users.Any(x => x.Id == userAccountId);
        }

        public IEnumerable<WildfireUserAccountItem> GetWildfireUsersBySupportAccountId(long supportAccountId, DataParameters dataParameters)
        {
            return _userAccountRepository.GetWildfireUsersBySupportAccountId(supportAccountId, dataParameters);
        }

        public UserProfile AddUpdateWildfireUserAccount(WildfireUserAccount userAccount, long supportAccountId, List<long> deviceIds, string clientIpAddress, long createdByUserAccountId, out long wildfireUserAccountId, List<long> changedDeviceIds = null)
        {
            UserProfile userProfile = null;

            if (!userAccount.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION)
                && !IsUserRegistered(userAccount.Email))
            {

                var supportAccountName = _supportAccountRepository.GetSupportAccountNameBySupportAccountId(supportAccountId);

                // Get CountryCode/RegionCode incase of US/Canada
                var addressCodes = GetRegionAndCountryCode(userAccount.Address);

                //Password isn't used here for updates (only new accounts) so it's safe to enter anything
                //Nevertheless, we'll generate a strong random password on the off chance that this is a new account (impossible) so that we don't accidentilly create an easily hackable new account
                //var meaninglessPassword = GenerateMeaninglessPassword();

                // SSO Registration
                var loginServiceLogic = new LoginServiceLogic();
                userProfile = loginServiceLogic.Register(userAccount.Email, string.Empty,
                                                                                                 userAccount.FirstName,
                                                                                                 userAccount.LastName, supportAccountName,
                                                                                                 userAccount.Address.Address1,
                                                                                                 userAccount.Address.Address2,
                                                                                                 userAccount.Address.City,
                                                                                                 addressCodes.RegionCode,
                                                                                                 userAccount.Address.PostalCode,
                                                                                                 addressCodes.CountryCode, userAccount.PhoneNumber,
                                                                                                 clientIpAddress, true);
            }

            var createdByUserAccount = _repository.Get<Useraccount>(createdByUserAccountId);

            var addUpdateWildfireUserAccountModel = new AddUpdateWildfireUserAccountModel
            {
                Email = userAccount.Email,
                FirstName = userAccount.FirstName,
                LastName = userAccount.LastName,
                CreatedByUserEmail = createdByUserAccount.Email,
                LastUpdatedByUserAccountId = createdByUserAccount.UserAccountId
            };

            if (userAccount.Id != 0)
            {
                addUpdateWildfireUserAccountModel.WildfireUserAccountId = userAccount.Id;

                _wildfireWipeoutService.DeleteEmailNotificationForDevices(userAccount.Id, supportAccountId,
                                                                                                                                    deviceIds.Select(x => (long)x));
            }

            wildfireUserAccountId = _userAccountRepository.AddUpdateWildfireUserAccount(addUpdateWildfireUserAccountModel, supportAccountId, deviceIds, createdByUserAccount.Email, changedDeviceIds, clientIpAddress);

            return userProfile;
        }

        public void BulkUpdateWildfireUserAccountDevices(long wildfireUserAccountId, long[] deviceIds, long supportAccountId, string clientIp)
        {

            var wildfireUserAccount = GetWildfireUserAccountById(wildfireUserAccountId);
            var userAccount = new WildfireUserAccount()
            {
                Id = wildfireUserAccount.WildfireUserAccountId,
                Email = wildfireUserAccount.Email,
                FirstName = wildfireUserAccount.FirstName,
                LastName = wildfireUserAccount.LastName
            };

            var userProfile = _loginServiceLogic.GetUserProfile(userAccount.Email, clientIp, _applicationSettings.LoginServicePrivateKey);
            if (userProfile == null) throw new Exception("NULL user profile for email: " + userAccount.Email);

            var countryId = _lookupLogic.GetCountryIdByCode(userProfile.Country);
            string region;

            if (userProfile.Country == CountryCode.US.ToDescription() || userProfile.Country == CountryCode.CA.ToDescription())
            {
                region = _lookupLogic.GetRegionIdByCode(userProfile.State, countryId).ToString();
            }
            else
            {
                region = userProfile.State;
            }

            userAccount.Address = new Address()
            {
                Address1 = userProfile.Address1,
                Address2 = userProfile.Address2,
                City = userProfile.City,
                CountryId = countryId,
                PostalCode = userProfile.Zipcode,
                Region = region,
            };

            userAccount.PhoneNumber = userProfile.Phone;


            const long createdBySystemUserAccountId = 1;
            long tempOut;
            AddUpdateWildfireUserAccount(userAccount, supportAccountId, deviceIds.ToList(),
                                                                                                                                                     clientIp, createdBySystemUserAccountId,
                                                                                                                                                     out tempOut);
        }

        public void DeleteWildfireUserAccount(long wildfireUserAccountId, long supportAccountId, string clientIpAddress,
                string deletedByUserAccountId)
        {

            //MUST delete email notifications FIRST because the DB must be populated with the wildfire user account before calling the service
            _wildfireWipeoutService.DeleteEmailNotification(wildfireUserAccountId, supportAccountId, deviceId: null);


            //Then it is safe to remove the DB entries
            _userAccountRepository.DeleteWildfireUserAccount(wildfireUserAccountId, supportAccountId, clientIpAddress, deletedByUserAccountId);
        }

        public Wildfireuseraccount GetWildfireUserAccountById(long id)
        {
            return _repository.Get<Wildfireuseraccount>(id);
        }

        public bool IsWildfireUserAccountExists(string email, long supportAccountId)
        {
            return _userAccountRepository.IsWildfireUserAccountExists(email, supportAccountId);
        }

        public long GetWildfireUserAccountId(string email)
        {
            return _userAccountRepository.GetWildfireUserAccountId(email);
        }

        public bool ShowEmailToSuperUserOption(string deviceCode, string customerCode)
        {
            //Verify device code is not in sales order history, but that customer code is.

            //Find SupportAccountIds by Device
            var deviceSerialNumberSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsByDeviceSerialNumber(deviceCode);
            var licenseAuthCodeSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsByLicenseAuthCode(deviceCode);

            var deviceSupportAccountIds = deviceSerialNumberSupportAccountIds.Union(licenseAuthCodeSupportAccountIds).ToArray();

            if (deviceSupportAccountIds.Any()) return false;


            //Find SupportAccountIds by Customer Code
            var sapIdSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsBySapId(customerCode);
            var salesOrderNumberSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsBySalesOrderNumber(customerCode);

            var customerSupportAccountIds = sapIdSupportAccountIds.Union(salesOrderNumberSupportAccountIds).ToArray();

            //Verify that customer code resovles to 1 and only 1 support account id
            if (customerSupportAccountIds.Count() != 1) return false;

            //Verify that device code is in device/license table
            var isValidDevice = _userRegistrationRepository.IsValidDeviceSerialNumber(deviceCode);
            var isValidLicense = _userRegistrationRepository.IsValidLicenseAuthCode(deviceCode);

            return isValidDevice || isValidLicense;
        }

        public void EmailSuperUsers(string email, string customerCode, string firstName, string lastName)
        {
            //Find SupportAccountIds by Customer Code
            var sapIdSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsBySapId(customerCode);
            var salesOrderNumberSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsBySalesOrderNumber(customerCode);

            var customerSupportAccountIds = sapIdSupportAccountIds.Union(salesOrderNumberSupportAccountIds).ToArray();

            if (customerSupportAccountIds.Count() != 1)
            {
                throw new Exception("Cannot send email. Failed to identify support account");
            }

            var supporAccountId = customerSupportAccountIds.Single();

            EmailHelper.SendSuperUserRequestForAccess(supporAccountId, email, firstName, lastName);
        }

        public bool IsEvalDeviceCode(string deviceCode)
        {
            return _userRegistrationRepository.IsEvalDeviceCode(deviceCode);
        }

        public bool IsEvalSupportAccount(long supportAccountId)
        {
            return _supportAccountRepository.IsEvalSupportAccount(supportAccountId);
        }

        #region IUserAccountLogic Members


        public bool IsProfileExists(string email, string ipAddress)
        {
            return _loginServiceLogic.IsProfileExists(email, ipAddress);
        }

        #endregion

        public void UpdateSalesforceContactsAccount(UserAccount userAccount, long? supportAccountId, string currentSfdcId, string newSfdcId)
        {
            _salesforceService.UpdateSalesforceContactsAccount(userAccount, supportAccountId, currentSfdcId, newSfdcId);
        }

        public IEnumerable<UserAccount> GetAwsSupportAccountUsers(long supportAccountId)
        {
            return _userAccountRepository.GetAwsSupportAccountUsers(supportAccountId);
        }


        public IEnumerable<UserAccount> GetSupportAccountUsers(long supportAccountId)
        {
            return _userAccountRepository.GetSupportAccountUsers(supportAccountId);
        }

        public IEnumerable<NameValueDescription> GetSupportAccountUsersWithSfdcContactId(long supportAccountId, string searchTerm)
        {
            return _userAccountRepository.GetSupportAccountUsersWithSfdcContactId(supportAccountId, searchTerm);
        }

        public void UpdateDeviceHyperVisorInfo(long deviceId, string panOsVersion, string hyperVisorVersion)
        {
            var deviceDataAccess = new DeviceDataAccess();

            deviceDataAccess.UpdateDeviceHyperVisorInfo(deviceId, panOsVersion, hyperVisorVersion);
        }

        public long GetDefaultSupportAccountByUser(long userAccountId)
        {
            var userAccountDataAccess = new UserAccountDataAccess();
            return userAccountDataAccess.GetDefaultSupportAccountByUser(userAccountId);
        }

        /// <summary>
        /// Get the details about the new user created
        /// </summary>
        /// <param name="userAccountId"></param>
        /// <param name="supportAccountId"></param>
        /// <param name="membershipId"></param>
        /// <returns></returns>
        public NewUserDetails GetNewUserCreationDetails(long userAccountId, long supportAccountId, long membershipId)
        {
            var userAccountDataAccess = new UserAccountDataAccess();

            return userAccountDataAccess.GetNewUserCreationDetails(userAccountId, supportAccountId, membershipId);
        }

        public bool GrantPermissionToUser(long supportAccountId, long userAccountId, Permission permission)
        {
            try
            {
                return _userAccountRepository.GrantPermissionToUser(supportAccountId, userAccountId, permission);
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("AssignRoleToUser exception: {0}, {1}", e.Message, e.StackTrace)));
                return false;
            }
        }

        public List<LicensingApi> GetSupportAccountsByLicensingApiKeyExpiration()
        {
            try
            {
                var userAccountDataAccess = new UserAccountDataAccess();
                return userAccountDataAccess.GetSupportAccountsByLicensingApiKeyExpiration();
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("AssignRoleToUser exception: {0}, {1}", e.Message, e.StackTrace)));
                return new List<LicensingApi>();
            }
        }

        #region Transfer of Tenants - Move users

        public void ProcessCloudServiceUsersTransfer()
        {
            try
            {
                var userAccountDataAccess = new UserAccountDataAccess();

                var usersPendingForTransfer = userAccountDataAccess.GetUsersPendingForTransfer();

                foreach (var user in usersPendingForTransfer)
                {
                    using (var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                        new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
                    {
                        //Create Member if user does not exist in destination account
                        if (user.IsNewMember)
                        {
                            var noofSuperUsers = _membershipLogic.GetCountOfUsersByRole(user.DestinationSupportAccountId, (int)RoleType.SuperUser);

                            var isMembershipPending = noofSuperUsers > 0 &&
                                SupportAccountRequiresSuperUserApproval(user.DestinationSupportAccountId);

                            var membership = new Membership()
                            {
                                UserAccount = new UserAccount { Id = user.UserAccountId },
                                SupportAccountId = user.DestinationSupportAccountId,
                                ActivationDate = DateTime.Now,
                                Description = "New user created",
                                CreatedOn = DateTime.Now,
                                CreatedByUserAccountId = user.CreatedByUserAccountId
                            };

                            if (isMembershipPending)
                            {
                                membership.MembershipStatus = (int)MembershipStatus.Pending;
                                membership.MembershipResponseGuid = Guid.NewGuid().ToString();
                                membership.ExpirationDate = DateTime.Now.AddDays(-1);
                            }
                            membership.IsValidateThreatResearcherUseCount = !user.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION);

                            membership = _membershipLogic.CreateMembership(membership);
                            var membershipId = membership.Id;

                            // Assign Membership Roles
                            _membershipLogic.AssignMembershipRoles(membershipId, user.UserAccountId, user.DestinationSupportAccountId,
                                                                    user.CreatedByUserAccountId);

                            if (isMembershipPending)
                            {
                                var emailModel = new UserAccountRegistrationEmailModel
                                {
                                    UserAccount = new UserAccount
                                    {
                                        Id = user.UserAccountId,
                                        FirstName = user.FirstName,
                                        LastName = user.LastName,
                                        Email = user.Email
                                    }
                                };
                                var baseUrl = ConfigurationManager.AppSettings["ApplicationUrl"];
                                var updateMembershipStatusUrl =
                                    string.Format(@"{0}/Membership/UpdateMembershipStatus", baseUrl);

                                EmailHelper.SendMembershipApprovalEmail(emailModel, user.DestinationSupportAccountId,
                                    membershipId,
                                    updateMembershipStatusUrl);
                            }
                        }

                        // TECH DEBT: cleanup after testing complete of CSP-1871 Create RBAC roles on the hub from CSP - add mapping
                        /*
                        //Get the current rbac role for user and move the same to destination
                        IEnumerable<RbackUserRole> destinationRoles = userAccountDataAccess.GetRbacRolesForUser(user.Email, user.DestinationSupportAccountId);
                        IEnumerable<RbackUserRole> sourceAccountRoles = userAccountDataAccess.GetRbacRolesForUser(user.Email, user.SourceSupportAccountId);
                        // CSP-1871 Create RBAC roles on the hub from CSP 
                        var roles = sourceAccountRoles.Where(i => !destinationRoles.Any(d => d == i)).ToList();
                        var rolesNew = roles.Select(i => i.Clone()).ToList();
                        rolesNew.ForEach(i => i.SupportAccountId = user.DestinationSupportAccountId.ToString());
                        // add the role to new Support Account
                        WebCommon.GetWebRequest(ConfigurationManager.AppSettings["RbacRoleProxy"]
                            + "PostHub?supportAccountId=" + user.DestinationSupportAccountId,
                            "POST", jsonPayload: JsonConvert.SerializeObject(rolesNew)).GetResponse();
                        // remove the role from old Support Account
                        WebCommon.GetWebRequest(ConfigurationManager.AppSettings["RbacRoleProxy"]
                            + "DeleteHub?supportAccountId=" + user.SourceSupportAccountId,
                            "DELETE", jsonPayload: JsonConvert.SerializeObject(roles)).GetResponse();
                        // ORIGINAL CODE
                        if (!(destinationAccountRoles.Any(r => r.Equals("Account Super User")) || destinationAccountRoles.Any(r => r.Equals(user.CloudService)))
                        {
                            var destinationAccountRoles = sourceAccountRoles.Contains(user.TenantId.ToString()) ? 
                                                        string.Join(",", sourceAccountRoles.Where(r => r.Contains(user.TenantId.ToString()))) :
                                                        string.Format(@"{0}:Instance Admin:{1}", user.CloudService, user.TenantId);
                            userAccountDataAccess.AddRbacRole(user.Email, user.DestinationSupportAccountId,
                                destinationAccountRoles, user.CreatedByUserEmail);
                        }

                        //Remove tenant specific role mappings from source account
                        var sourceAccountTenantRoles = sourceAccountRoles.Where(r => r.Contains(user.TenantId.ToString())).ToList();
                        if (sourceAccountTenantRoles.Any())
                        {
                            userAccountDataAccess.DeleteRbacRoleMapping(user.Email, user.SourceSupportAccountId,
                                string.Join(",", sourceAccountTenantRoles), user.CreatedByUserEmail);
                        }
                        */

                        // Update the status of transfer for the user
                        userAccountDataAccess.UpdateUserTransferStatus(user.TransferId, ApiConstants.COMPLETE);

                        transactionScope.Complete();
                    }

                    //Send Notification
                    EmailHelper.SendUserTransferNotification(user);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        #endregion

        public bool IsUserExistsByUniqueIdAndEmail(string uniqueId, string email)
        {
            var userAccountDataAccess = new UserAccountDataAccess();

            return userAccountDataAccess.IsUserExistsByUniqueIdAndEmail(uniqueId, email);
        }

        #region private method
        private static void UpdateCsodUserType(string email, string userType, string updatedBy)
        {
            var baseUrl = ConfigurationManager.AppSettings["CornerstoneApiUrl"];
            var apiToken = ConfigurationManager.AppSettings["CornerstoneApiToken"];
            var apiclientName = ConfigurationManager.AppSettings["CornerstoneApiClientName"];
            var certThumbprint = ConfigurationManager.AppSettings["CornerstoneApiCertThumbprint"];

            try
            {
                var url = string.Format("{0}account?email={1}", baseUrl, email);
                var input = new
                {
                    userType = userType,
                    updatedBy = updatedBy,
                    source = apiclientName
                };
                var json = JsonConvert.SerializeObject(input);

                var httpWebRequest = WebCommon.GetWebRequestWithClientCertificate(url, "PATCH", certThumbprint, json);

                httpWebRequest.Headers.Add("Authorization", string.Format("Basic {0}", apiToken));

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result =
                        JsonConvert.DeserializeObject<CornerstoneUserEmailUpdateResponse>(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        private bool IsUserRegistered(string email)
        {
            return _loginServiceLogic.IsUserRegistered(email);
        }
        #endregion

        public UserProfile ActivateUser(string email)
        {
            return _loginServiceLogic.ActivateUser(email);
        }

        #region public domain
        public bool CanAddEmailToSupportAccount(long supportAccountId, string email)
        {
            try
            {
                string method = HttpMethod.Get.ToString();
                var baseUrl = string.Format(ApiConstants.AddEmailToSupportAccountEndpoint, _applicationSettings.PortalBffHost,
                    supportAccountId, supportAccountId, email);
                var result = CommonHelper.ProcessWebRequest(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, 
                    string.Empty, true);
                return result.ToLower() == "true";
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("CanAddEmailToSupportAccount exception: {0}, {1}",
                    ex.Message, ex.StackTrace)));
                return false;
            }
        }

        public string GetDomainDetails(string domain)
        {
            try
            {
                return GetDomainResponseMessage(GetDomainData(domain));
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetDomainDetails exception: {0}, {1}",
                    ex.Message, ex.StackTrace)));
                return string.Empty;
            }
        }

        public long GetCsspSupportAccountId(string customerAccountId)
        {
            string endUserAccount = string.Empty;
            long csspSupportAccountId = 0;
            long? csspEndUserSupportAccountId = null;
            string endUserSFDCAccountId = null;
            endUserAccount = string.Join("-", customerAccountId.Split('-').Skip(1));
            csspSupportAccountId = Convert.ToInt64(customerAccountId.Split('-')[0]);
            _csspRepository.GetEndUserSupportAccount(endUserAccount, csspSupportAccountId, out csspEndUserSupportAccountId, out endUserSFDCAccountId);
            return csspEndUserSupportAccountId.HasValue ? csspEndUserSupportAccountId.Value : 0;
        }

        private DomainResponse GetDomainData(string domain)
        {
            try
            {
                string method = HttpMethod.Get.ToString();
                var baseUrl = string.Format(ApiConstants.DomainDetailEndpoint, _applicationSettings.PortalBffHost,
                    domain);
                return CommonHelper.ProcessWebRequest<DomainResponse>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint,
                    string.Empty, true).ResponseObject;
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetDomainDetails exception: {0}, {1}",
                    ex.Message, ex.StackTrace)));
                return new DomainResponse();
            }
        }

        private string GetDomainResponseMessage(DomainResponse response)
        {
            return (response == null) ? "Error: unable to create user account. Please contact Support" :
                  response.IsBlacklisted ? ApiConstants.BlockListedDomainMessage :
                  response.IsPublic ? ApiConstants.PrivateDomainMessage :
                  ApiConstants.PublicDomainMessage;
        }

        private bool CheckMemberExists(long supportAccountId)
        {
            try
            {
                string method = HttpMethod.Get.ToString();
                var baseUrl = string.Format(ApiConstants.HasMemberEndpoint, _applicationSettings.PortalBffHost,
                    supportAccountId);
                var response = CommonHelper.ProcessWebRequest<SupportMember>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint,
                    string.Empty, true).ResponseObject;
                return (response?.MemberExists == true) ? true : false;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("CheckMemberExists exception: {0}, {1}",
                    ex.Message, ex.StackTrace)));
                return false;
            }
        }

        #endregion
    }
}
