using System.IO;
using Csp.Api.DataAccess;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.EntityClasses;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Csp.Api.Services.Contracts;
using PAN.Common.Core.Persistence;
using Role = Csp.Api.Model.Role;
using Csp.Api.Model.Enums;
using AccountType = Csp.Api.Model.AccountType;

namespace Csp.Api.Logic {

	public class SupportAccountLogic : BaseLogic, ISupportAccountLogic {
		private readonly IRepository _repository;
		private readonly IUserAccountLogic _userAccountLogic;
		private readonly IDeviceLogic _deviceLogic;
		private readonly IUserRegistrationRepository _userRegistrationRepository;
		private readonly ISupportAccountRepository _supportAccountRepository;
		private readonly IMembershipLogic _membershipLogic;
		private readonly ILicenseLogic _licenseLogic;

		public SupportAccountLogic(IRepository repository, IUserAccountLogic userAccountLogic, IDeviceLogic deviceLogic, IUserRegistrationRepository userRegistrationRepository, ISupportAccountRepository supportAccountRepository, IMembershipLogic membershipLogic, ILicenseLogic licenseLogic) {
			_repository = repository;
			_userAccountLogic = userAccountLogic;
			_deviceLogic = deviceLogic;
			_userRegistrationRepository = userRegistrationRepository;
			_supportAccountRepository = supportAccountRepository;
			_membershipLogic = membershipLogic;
			_licenseLogic = licenseLogic;
		}

		private static volatile Dictionary<long, AccountType> _accountTypeDictionary = new Dictionary<long, AccountType>();


		///<summary>
		/// This method is used to update defaultroles list for an Group.
		///</summary>
		///<param name="supportAccount">Group</param>
		/// <returns>void</returns>
		private void UpdateDefaultRoles(SupportAccount supportAccount) {

			Dictionary<long, long> databaseDefaultRolesValues = new Dictionary<long, long>();
			Dictionary<long, long> objectDefaultRolesValues = new Dictionary<long, long>();
			ListUpdateHelper updateHelper;
			List<Role> databaseDefaultRoleList = null;
			SupportAccountDefaultRolesDataAccess supportAccountDefaultRolesDataAccess = new SupportAccountDefaultRolesDataAccess();

			databaseDefaultRoleList = supportAccountDefaultRolesDataAccess.Read(supportAccount.Id, new DataParameters());

			foreach (Role role in databaseDefaultRoleList) {
				databaseDefaultRolesValues.Add(role.Id, supportAccount.Id);
			}

			foreach (Role role in supportAccount.DefaultRoles) {
                #region TAC Super User/TAC User/Impersonate User
                // Comments By LK On 03/21
                // Exclude TAC Super User/TAC User/Impersonate User roles for any account other than PANW
                if (supportAccount.Id != ApiConstants.PANW_SUPPORT_ACCOUNT_ID && (role.Id == (long)RoleType.TACSuperUser || role.Id == (long)RoleType.TACUser ||
																																								 role.Id == (long)RoleType.TACAdmin || role.Id == (long)RoleType.ImpersonateUser))
                {
                    continue;
                }
                #endregion

				objectDefaultRolesValues.Add(role.Id, supportAccount.Id);
			}

			updateHelper = new ListUpdateHelper(databaseDefaultRolesValues, objectDefaultRolesValues, supportAccount.LastUpdateByUserAccountId,
							supportAccountDefaultRolesDataAccess.Create, supportAccountDefaultRolesDataAccess.Delete);
			updateHelper.UpdateDatabase();
		}

		/// <summary>
		/// This metod is used for updating support Account Details
		/// </summary>
		/// <param name="supportAccount"></param>
		/// <returns>SupportAccount</returns>
		public SupportAccount UpdateSupportAccount(SupportAccount supportAccount, long restrictedByUserAccountId) {

			SupportAccount resultSupportAccount = null;
			SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();
			SupportAccountDefaultRolesDataAccess supportAccountDefaultRolesDataAccess = new SupportAccountDefaultRolesDataAccess();

			using (TransactionScope transactionScope = new TransactionScope()) {

				resultSupportAccount = new SupportAccount();

				UpdateDefaultRoles(supportAccount);
				// Update Support Account details
				resultSupportAccount = supportAccountDataAccess.Update(supportAccount, restrictedByUserAccountId);
				resultSupportAccount.DefaultRoles = GetsupportAccountDefaultRoles(supportAccount.Id, new DataParameters());
				transactionScope.Complete();
			}

			return resultSupportAccount;
		}


		/// <summary>
		/// This method is used for retriving Support account details based on accountId
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns>SupportAccount</returns>
		public SupportAccount GetSupportAccount(long supportAccountId, long restrictedByUserAccountId) {

			SupportAccount supportAccount = null;
			// Geting SupportAccount Details
			SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();

			supportAccount = supportAccountDataAccess.Read(supportAccountId, restrictedByUserAccountId);

			supportAccount.DefaultRoles = GetsupportAccountDefaultRoles(supportAccountId, new DataParameters());
			return supportAccount;
		}

		public Supportaccount GetSupportAccountById(long id) {
			return _repository.Get<Supportaccount>(id);
		}

		public string GetSupportAccountNameById(long id) {
			var name = string.Empty;
			var supportAccount = _repository.Get<Supportaccount>(id);

			if (supportAccount != null) {
				name = supportAccount.Name;
			}

			return name;
		}


		/// <summary>
		/// The purpose of this task is to read the roles for a support account: DT-0007
		/// </summary>
		/// <param name="supportAccountId"></param>
		/// <param name="parameters"></param>
		/// <returns>List<Role></returns>
		public List<Role> GetsupportAccountDefaultRoles(long supportAccountId, DataParameters parameters) {

			SupportAccountDefaultRolesDataAccess supportAccountDefaultRolesDataAccess = new SupportAccountDefaultRolesDataAccess();

			return supportAccountDefaultRolesDataAccess.Read(supportAccountId, parameters);
		}


		/// <summary>
		/// The purpose of this method to get account type name : DT-0065
		/// </summary>
		/// <param name="accountTypeId"></param>
		/// <returns>AccountType</returns>
		public AccountType GetAccountType(long accountTypeId) {

			AccountType resultAccountType = null;
			if (!_accountTypeDictionary.TryGetValue(accountTypeId, out resultAccountType)) {

				// If the result is null let's reload the AccountTypes to refresh them into our cache dictionary
				GetAccountTypes(new DataParameters());
				_accountTypeDictionary.TryGetValue(accountTypeId, out resultAccountType);
			}

			return resultAccountType;
		}


		/// <summary>
		/// The purpose of this method to get account type list name : DT-0065
		/// </summary>
		/// <param name="dataParameters"></param>
		/// <returns>List<AccountType></returns>
		public List<AccountType> GetAccountTypes(DataParameters dataParameters) {

			_accountTypeDictionary.Clear();

			List<AccountType> resultAccountType = new List<AccountType>();
			AccountTypeDataAccess accountTypeDataAccess = new AccountTypeDataAccess();
			resultAccountType = accountTypeDataAccess.Read(dataParameters);

			foreach (var accountType in resultAccountType) {
				_accountTypeDictionary.Add(accountType.Id, accountType);
			}

			return resultAccountType;
		}



		public List<SupportAccountListItem> GetSupportAccountbyUserAccountId(long userAccountId, DataParameters parameters) {

			List<SupportAccountListItem> supportAccountListItems = null;
			SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();


			supportAccountListItems = supportAccountDataAccess.ReadSupportAccountbyUserAccountId(userAccountId, parameters);

			return supportAccountListItems;
		}


		public List<SupportAccountMembershipListItem> GetSupportAccountMembershipsbyUserAccountId(long userAccountId, long restrictedByUserAccountId, DataParameters parameters) {


			SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();


			return supportAccountDataAccess.ReadSupportAccountMembershipsbyUserAccountId(userAccountId, restrictedByUserAccountId, parameters);


		}

		public MatchingSupportAccount[] GetSupportAccountsBySfdcid(string sfdcid) {
            SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();
            return supportAccountDataAccess.GetSupportAccountsBySfdcid(sfdcid);
		}

        

        public List<SelectorAccountListItem> GetSupportAccountsAndGroupsByEmailId(string emailId) {
			List<SelectorAccountListItem> selectorAccountListItems = null;
			SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();

			selectorAccountListItems = supportAccountDataAccess.ReadSupportAccountsAndGroupsByEmailId(emailId);

			return selectorAccountListItems;
		}

		public bool AddUpdateSelectedSupportAccount(string email, long membershipId, long supportAccountId) {
			SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();

			return supportAccountDataAccess.AddUpdateSelectedSupportAccount(email, membershipId, supportAccountId);
		}		

		public string GetSalesforceIdBySupportAccountId(long id) {
			var salesforceId = string.Empty;
			var supportAccount = _repository.Get<Supportaccount>(id);

			if (supportAccount != null) {
				salesforceId = supportAccount.SalesforceAccountId;
			}

			return salesforceId;
		}

		public List<NameValue> GetSupportAccountsBySapId(long id) {
			SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();
			return supportAccountDataAccess.GetSupportAccountsBySapId(id);

		}

		public EvalToCustModel ConvertEvalAccountToCust(long supportAccountId, string deviceCode, string customerCode, long userAccountId) {

			var validationResults = ValidateEvalAccountToCust(supportAccountId, deviceCode, customerCode);

			if (!string.IsNullOrEmpty(validationResults.Error)) {
				return validationResults;
			}

			if (validationResults.IsActiveOtherAccount) {

				//register user to account as default role and email super
				var userAccount = _userAccountLogic.GetUserAccount(userAccountId, userAccountId);

				var mem = _membershipLogic.CreateMembership(new Model.Membership {
					UserAccount = new UserAccount { Id = userAccountId },
					SupportAccountId = validationResults.OtherSupportAccountId,
					ActivationDate = DateTime.Now,
					Description = "Eval user added to related customer account by device code: " + deviceCode,
					CreatedOn = DateTime.Now,
					CreatedByEmail = userAccount.Email,
					CreatedByUserAccountId = userAccountId,
				});

				_membershipLogic.AssignMembershipRoles(mem.Id, userAccountId, validationResults.OtherSupportAccountId, userAccountId);


                EmailHelper.SendUserRegistrationEmailToSuperUsers(userAccount.FirstName, userAccount.LastName, supportAccountId, userAccountId, mem.Id);
			} else {
				//delete other account and re-assign SAP Mappings to new account
				if ((validationResults.OtherSupportAccountId != supportAccountId)) {
					_supportAccountRepository.RemoveDuplicateAccountAndFixSapMappings(validationResults.OtherSupportAccountId,
																																						supportAccountId);
				}

				_supportAccountRepository.ConvertEvalSupportAccountToCustomer(supportAccountId);
			}

			return validationResults;
		}

		/// <summary>
		/// Validate that this support account is an EVAL account that can be converted to a CUST account
		/// </summary>
		/// <param name="supportAccountId"></param>
		/// <param name="deviceCode"></param>
		/// <param name="customerCode"></param>
		/// <returns></returns>
		public EvalToCustModel ValidateEvalAccountToCust(long supportAccountId, string deviceCode, string customerCode) {

			var isEvalSupportAccount = _userAccountLogic.IsEvalSupportAccount(supportAccountId);

			if (!isEvalSupportAccount) {
				var exception = string.Format("Only Eval Accounts can be converted to a Customer Account.  SupportAccountId: {0} DeviceCode: {1}", supportAccountId, deviceCode);
				throw new Exception(exception);
			}

			//Validate the support account is an eval account and the device code is in SAP
			//Look at SAP support account for device code and verify it has 0 assets and 0 memberships

			if (string.IsNullOrEmpty(deviceCode)) {
				return new EvalToCustModel { Error = "Serial Number or Auth Code is required" };
			}

			var isValidLic = _licenseLogic.IsAuthCodeValid(deviceCode);
			var isValidSn = _deviceLogic.CheckIsSerialNumberValid(deviceCode);

			if (!isValidSn && !isValidLic) {
				return new EvalToCustModel { Error = "Serial Number or Auth Code must be valid.  Please contact customer support" };
			}

			var isEvalDeviceCode = _userAccountLogic.IsEvalDeviceCode(deviceCode);

			if (isEvalDeviceCode) {
				return new EvalToCustModel { Error = "You must provide a purchased serial number or auth code.  Eval-IDs are not allowed." };
			}

			var sapAccountDetails = GetSapAccountDetails(deviceCode);

			if (sapAccountDetails == null) {
				return new EvalToCustModel { Error = "The device code provided could not be matched to an account.  Please contact customer support" };
			}

			var accountsAreDifferent = sapAccountDetails.SupportAccountId != supportAccountId;

			//Match customer code to sapaccountdetails
			var sapIdSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsBySapId(customerCode);
			var salesOrderNumberSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsBySalesOrderNumber(customerCode);

			var customerSupportAccountIds = sapIdSupportAccountIds.Union(salesOrderNumberSupportAccountIds).ToArray();
			var custCodeMatchesDeviceCode = customerSupportAccountIds.Contains(sapAccountDetails.SupportAccountId);

			if (!custCodeMatchesDeviceCode) {
				return new EvalToCustModel { Error = "The device code provided could not be matched to an account.  Please contact customer support" };
			}


			return new EvalToCustModel {
				CompanyName = sapAccountDetails.CompanyName,
				IsActiveOtherAccount = sapAccountDetails.IsActive && accountsAreDifferent,
				OtherSupportAccountId = sapAccountDetails.SupportAccountId,
			};
		}

		public SapAccountModel GetSapAccountDetails(string deviceCode) {

			//look up device code in SAP Sales order history
			var snSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsByDeviceSerialNumber(deviceCode);
			var authCodeSupportAccountIds = _userRegistrationRepository.GetSalesOrderSupportAccountIdsByLicenseAuthCode(deviceCode);
			var supportAccountIds = snSupportAccountIds.Union(authCodeSupportAccountIds).Distinct().ToArray();

			//verify it exists there and maps to 1 and only 1 support account
			if (supportAccountIds.Count() != 1) {
				return null;
			}

			var supportAccountId = supportAccountIds.Single();

			return new SapAccountModel {
				SupportAccountId = supportAccountId,
				CompanyName = _supportAccountRepository.GetSupportAccountNameBySupportAccountId(supportAccountId),
				IsActive = _supportAccountRepository.IsActiveSupportAccount(supportAccountId),
			};
		}


        public bool IsSupportAccountExists(string supportAccountName)
        {
            return _supportAccountRepository.IsSupportAccountExists(supportAccountName);
        }

        public bool CreateSupportAccount(string name, int type, long createdBy, out long SupportAccountId)
        {
            return _supportAccountRepository.CreateSupportAccount(name, type, createdBy, out SupportAccountId);
        }


        public long? GetSupportAccountIdByName(string supportAccountName)
        {
            return _supportAccountRepository.GetSupportAccountIdByName(supportAccountName);
        }

				public bool CreateAwsAccountSupportAccountMapping(string awsAccountId, long supportAccountId)
				{
					return _supportAccountRepository.CreateAwsAccountSupportAccountMapping(awsAccountId, supportAccountId);
				}

	    public bool RegistrationRequiresSuApproval(long supportAccountId)
	    {
	        return _supportAccountRepository.RegistrationRequiresSuApproval(supportAccountId);
	    }

	    public void SetRegistrationRequiresSuApproval(long supportAccountId, bool registrationRequiresSuApproval)
	    {
	        _supportAccountRepository.SetRegistrationRequiresSuApproval(supportAccountId, registrationRequiresSuApproval);
	    }

        public bool IsCsspAccount(long supportAccountId)
        {
            return _supportAccountRepository.IsCsspAccount(supportAccountId);
        }

        public bool IsFedrampAccount(long supportAccountId)
        {
            return _supportAccountRepository.IsFedrampAccount(supportAccountId);
        }

        public bool IsMSSPAccount(long supportAccountId)
        {
            return _supportAccountRepository.IsMSSPAccount(supportAccountId);
        }

        public SupportAccount GetSeLabKitSupportAccountId(long userAccountId)
        {
            return _supportAccountRepository.GetSeLabKitSupportAccountId(userAccountId);
        }

        public SupportAccount CreateSeLabKitSupportAccount(long userAccountId, string salesforceAccountId, int accountTypeId, string accountName,
            string phoneNumber, string address1, string address2, string city, string region, string postalCode, long? countryId, long createdByUserAccountId)
        {   
            return _supportAccountRepository.CreateSeLabKitSupportAccount(userAccountId, salesforceAccountId, accountTypeId, accountName, phoneNumber, address1,
                address2, city, region, postalCode, countryId, createdByUserAccountId);
        }

        public bool IsThirdPartyIdpBetaAccount(string domain)
        {
           SupportAccountDataAccess supportAccountDataAccess=new SupportAccountDataAccess();
           return supportAccountDataAccess.IsThirdPartyIdpBetaAccount(domain);
        }

        public int CanImpersonate(long supportAccountId)
        {
            return _supportAccountRepository.CanImpersonate(supportAccountId);
        }
    }
}
