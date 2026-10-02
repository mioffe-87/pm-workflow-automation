using System;
using System.IO;
using System.Transactions;
using System.Web.Mvc;
using AutoMapper;
using Csp.Api.DataAccess;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.EntityClasses;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Mail;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using LINQtoCSV;
using PAN.Common.Core.Persistence;
using Salesforce.Common;
using Membership = Csp.Api.Model.Membership;
using Role = Csp.Api.Model.Role;

namespace Csp.Api.Logic {

	public class MembershipLogic : BaseLogic, IMembershipLogic {

		private readonly IRepository _repository;
		private readonly IGroupLogic _groupLogic;
        private readonly IMfaLogic _mfaLogic;
		private readonly ISalesforceService _salesforceService;
		private readonly ISecurityRepository _securityRepository;
		private readonly IMembershipDataAccess _membershipDataAccess;
        private readonly IMembershipRepository _membershipRepository;
        private readonly ICurrentStateService _currentStateService;

        #region Constants

        private const string RoleAssignmentErrorMessage = "{0} Role cannot be added, User is already Threat Researcher at {1} {2}";

        #endregion

        public MembershipLogic(IRepository repository, IGroupLogic groupLogic, ISalesforceService salesforceService, IMembershipDataAccess membershipDataAccess, 
            ISecurityRepository securityRepository, IMembershipRepository membershipRepository, IMfaLogic mfaLogic, ISupportAccountRepository supportAccountRepository,
            ICurrentStateService currentStateService
           )
        {
			_repository = repository;
			_groupLogic = groupLogic;
			_salesforceService = salesforceService;
			_membershipDataAccess = membershipDataAccess;
			_securityRepository = securityRepository;
            _membershipRepository = membershipRepository;
            _mfaLogic = mfaLogic;
            _currentStateService = currentStateService;
        }

		/// <summary>
		/// This method is used to create Membership
		/// </summary>
		/// <param name="membership">membership</param>
        /// <param name="groupSupportAccountId">GroupSupportAccountId</param>
		/// <returns>Membership</returns>
        public Membership CreateMembership(Membership membership, long groupSupportAccountId = 0)
        {
			membership.ActivationDate = membership.ActivationDate.AddDays(-1);

			Membership localMembership = new Membership();
			MembershipDataAccess membershipDataAccess = new MembershipDataAccess();

            //Use Group Support Account Id to check TR License counts if it is a Group Membership
            var supportAccountId = membership.SupportAccountId == 0 ? groupSupportAccountId
                                                                    : membership.SupportAccountId;

            var isMfaEnabled = _securityRepository.IsAccountMfaEnabled(supportAccountId);
            membership.IsMfaEnabled = isMfaEnabled;

			using (TransactionScope transactionScope = new TransactionScope()) {

                //Threat Researcher and Autofocus Trial Roles should not be assigned together
                if ((membership.Roles.Exists(r => r.Id == (int)RoleType.ThreatResearcher) && membership.Roles.Exists(r => r.Id == (int)RoleType.AutoFocusTrial)) ||
                    (membership.Roles.Exists(r => r.Id == (int)RoleType.GroupThreatResearcher) && membership.Roles.Exists(r => r.Id == (int)RoleType.GroupAutoFocusTrial)))
                {
                    localMembership.ThreatResearcherAssignmentError = "Threat Researcher and Autofocus Trial roles cannot be assigned together";
                    return localMembership;
                }

                // Creating Membership
                localMembership = membershipDataAccess.Create(membership);

				foreach (Role role in membership.Roles) {
                    #region TAC Super User/TAC User/Impersonate User
                    // Security Fix: TAC/Impersonate roles require caller to be TAC user/admin on PANW account
                    if (role.Id == (long)RoleType.TACSuperUser || role.Id == (long)RoleType.TACUser ||
                        role.Id == (long)RoleType.TACAdmin || role.Id == (long)RoleType.ImpersonateUser)
                    {
                        // Only allow if target is PANW account AND caller is TAC user/admin
                        if (supportAccountId != ApiConstants.PANW_SUPPORT_ACCOUNT_ID ||
                            !_securityRepository.IsTacUserOrAdmin(membership.CreatedByUserAccountId, ApiConstants.PANW_SUPPORT_ACCOUNT_ID))
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region Threat Researcher role Checks
                    if ((role.Id == (long)RoleType.ThreatResearcher || role.Id == (long)RoleType.GroupThreatResearcher))
                    {
                        string errorMessage;
                        if (!CanAssignThreatResearcherRole(membership.UserAccount.Id, supportAccountId, out errorMessage))
                        {
                            localMembership.ThreatResearcherAssignmentError = errorMessage;
                            return localMembership;
                        }
                        //Check for AutoFocus SiteLicense Count if TR Role Assignment is allowed
                        if (membership.IsValidateThreatResearcherUseCount)
                        {
                            var hasFreeCanaryLicenseUses = _securityRepository.HasFreeCanaryLicenseUses(supportAccountId);
                            if (!hasFreeCanaryLicenseUses)
                            {
                                localMembership.ShowThreatResearcherUseCountWarning = true;
                                //Rollback transaction
                                return localMembership;
                            }
                        }
                    }
                    #endregion

                    #region AutoFocus Trial Role Checks
                    if (role.Id == (int)RoleType.AutoFocusTrial || role.Id == (int)RoleType.GroupAutoFocusTrial)
                    {
                        string errorMessage;
                        if (!CanAssignAutofocusTrialRole(supportAccountId, membership.UserAccount.Id, out errorMessage))
                        {
                            localMembership.AutofocusTrialAssignmentError = errorMessage;
                            return localMembership;
                        }
                    }
                    #endregion

                    #region ATC Manager Role

                    if (role.Id == (long)RoleType.ATCManager)
                    {
                        var seatsAvailable = _securityRepository.IsAtcManagerSeatsAvailable(supportAccountId);
                        if (!seatsAvailable)
                        {
                            localMembership.ShowATCManagerUseCountWarning = true;
                            return localMembership;
                        }
                    }

                    #endregion

                    CreateMembershipRole(localMembership.Id, role.Id, membership.CreatedByUserAccountId);
                }

				localMembership.Roles = GetMembershipRoles(localMembership.Id, new DataParameters());

				transactionScope.Complete();
			}

            //Enable 2 Factor Authentication for user if support account is eligible for MFA
            MultiFactorAuthType mfaType = MultiFactorAuthType.None;
            localMembership.IsMfaEnabled = ProcessMfaRegistration(membership, supportAccountId, out mfaType);
            localMembership.MultiFactorAuthType = mfaType;

            return localMembership;
		}


		/// <summary>
		/// This method is used to get MemberShip based on membershipId
		/// </summary>
		/// <param name="membershipId">membershipId</param>
		/// <returns>Membership</returns>
		public Membership GetMembership(long membershipId, long restrictedByUserAccountId) {

			Membership membership = null;
			MembershipDataAccess membershipDataAccess = new MembershipDataAccess();

			membership = membershipDataAccess.Read(membershipId, restrictedByUserAccountId);

			membership.Roles = GetMembershipRoles(membership.Id, new DataParameters());

			return membership;
		}


		/// <summary>
		/// This method is used to update membership Details
		/// </summary>
		/// <param name="membership">membership</param>
		/// <returns>Membership</returns>
		public Membership UpdateMembership(Membership membership) {

			MembershipDataAccess membershipDataAccess = new MembershipDataAccess();
			Membership localMembership = null;

			membership.ActivationDate = membership.ActivationDate.AddDays(-1);

            //Get Support Account Id for Group TR Role
            var supportAccountId = (membership.SupportAccountId == 0) 
                                                ? GetSupportAccountIdByGroupId(membership.GroupId) : membership.SupportAccountId;

			using (TransactionScope transactionScope = new TransactionScope()) {

				// updaing MemberShip Roles
				/* !! IMPORTANT !! The following line is disabled for test.
				 * The update to membershiproles will likley be done separately
					*/

				// updating MemberShip Details

                var result = UpdateMembershipRoles(membership, supportAccountId ?? 0);

                localMembership = membershipDataAccess.Update(membership);

                localMembership.Roles = GetMembershipRoles(localMembership.Id, new DataParameters());

			    localMembership.ShowThreatResearcherUseCountWarning = result.ShowThreatResearcherUseCountWarning;
			    localMembership.ThreatResearcherAssignmentError = result.ThreatResearcherRoleAssignmentError;
			    localMembership.AutofocusTrialAssignmentError = result.AutofocusTrialRoleAssignmentError;
			    localMembership.AutofocusTrialRoleWarning = result.AutofocusTrialRoleWarning;
			    localMembership.ShowATCManagerUseCountWarning = result.ShowATCManagerUseCountWarning;

				transactionScope.Complete();

			}

            //Enable 2 Factor Authentication for user if support account is eligible for MFA
            MultiFactorAuthType mfaType = MultiFactorAuthType.None;
            localMembership.IsMfaEnabled = ProcessMfaRegistration(membership, supportAccountId ?? 0, out mfaType);
            localMembership.MultiFactorAuthType = mfaType;

            return localMembership;
		}

		public void UpdateMembershipSf(MembershipModel membershipModel) {
			_membershipDataAccess.UpdateMembershipSf(membershipModel);
		}


		public class UpdateMembershipRolesResults {
			public bool ShowThreatResearcherUseCountWarning { get; set; }
            public string ThreatResearcherRoleAssignmentError { get; set; }
            public string AutofocusTrialRoleAssignmentError { get; set; }
            public string AutofocusTrialRoleWarning { get; set; }
            public bool ShowATCManagerUseCountWarning { get; set; }
		}

		/// <summary>
		/// This method is used to update list of roles for an membership
		/// </summary>
        /// <param name="membership">membership</param>
        /// <param name="supportAccountId">supportAccountId</param>
        /// <returns>UpdateMembershipRolesResults</returns>
		private UpdateMembershipRolesResults UpdateMembershipRoles(Membership membership, long supportAccountId) {

			var result = new UpdateMembershipRolesResults();

			Dictionary<long, long> databaseMembershipRolesDataAccessValues = new Dictionary<long, long>();
			Dictionary<long, long> objectMembershipRolesDataAccessValues = new Dictionary<long, long>();
			ListUpdateHelper updateHelper;
			List<Role> databaseRoleList = null;
			MembershipRolesDataAccess membershipRolesDataAccess = new MembershipRolesDataAccess();

			databaseRoleList = membershipRolesDataAccess.Read(membership.Id, new DataParameters());

			foreach (Role role in databaseRoleList) {
				databaseMembershipRolesDataAccessValues.Add(role.Id, membership.Id);
			}

            var alreadyHasTrRole = databaseMembershipRolesDataAccessValues.Any(r => r.Key == (int)RoleType.ThreatResearcher ||
                                                                                    r.Key == (int)RoleType.GroupThreatResearcher);
            var alreadyHasAutofocusTrialRole = databaseMembershipRolesDataAccessValues.Any(r => r.Key == (int)RoleType.AutoFocusTrial ||
                                                                                                r.Key == (int)RoleType.GroupAutoFocusTrial);
            var alreadyHasAtcManagerRole = databaseMembershipRolesDataAccessValues.Any(r => r.Key == (int)RoleType.ATCManager);

            //Threat Researcher and Autofocus Trial Role should not be assigned together
            if (((membership.Roles.Exists(r => r.Id == (int)RoleType.ThreatResearcher) && membership.Roles.Exists(r => r.Id == (int)RoleType.AutoFocusTrial)) ||
                (membership.Roles.Exists(r => r.Id == (int)RoleType.GroupThreatResearcher) && membership.Roles.Exists(r => r.Id == (int)RoleType.GroupAutoFocusTrial)))
                && !alreadyHasTrRole && !alreadyHasAutofocusTrialRole)
            {
                result.AutofocusTrialRoleAssignmentError = "Threat Researcher and Autofocus Trial roles cannot be assigned together";
                return result;
            }

			foreach (Role role in membership.Roles)
            {
                #region TAC Super User/TAC User/Impersonate User
                // Security Fix: TAC/Impersonate roles require caller to be TAC user/admin on PANW account
                if (role.Id == (long)RoleType.TACSuperUser || role.Id == (long)RoleType.TACUser ||
                    role.Id == (long)RoleType.TACAdmin || role.Id == (long)RoleType.ImpersonateUser)
                {
                    // Only allow if target is PANW account AND caller is TAC user/admin
                    if (supportAccountId != ApiConstants.PANW_SUPPORT_ACCOUNT_ID ||
                        !_securityRepository.IsTacUserOrAdmin(membership.LastUpdateByUserAccountId, ApiConstants.PANW_SUPPORT_ACCOUNT_ID))
                    {
                        continue;
                    }
                }
                #endregion

                #region Threat Researcher Role
                if (role.Id == (long)RoleType.ThreatResearcher || role.Id == (long)RoleType.GroupThreatResearcher)
                {
                    //Do they already have it?
                    //Do not check for License count if User is already a Threat Researcher
				    if (!alreadyHasTrRole)
				    {
                        string errorMessage;
                        if (!CanAssignThreatResearcherRole(membership.UserAccount.Id, supportAccountId, out errorMessage))
                        {
                            result.ThreatResearcherRoleAssignmentError = errorMessage;
                            continue;
                        }
                        //Check for AutoFocus SiteLicense Count if TR Role Assignment is allowed
				        if (membership.IsValidateThreatResearcherUseCount)
				        {
				            var hasFreeCanaryLicenseUses = _securityRepository.HasFreeCanaryLicenseUses(supportAccountId);
				            if (!hasFreeCanaryLicenseUses)
				            {
				                result.ShowThreatResearcherUseCountWarning = true;
				                continue;
				            }
				        }
				    }
                }
                #endregion

                #region Autofocus Trial Role
                if ((role.Id == (int)RoleType.AutoFocusTrial || role.Id == (int)RoleType.GroupAutoFocusTrial) && !alreadyHasAutofocusTrialRole)
                {
                    string errorMessage;
                    NameValue category;
                    var isThreatResearcher = _securityRepository.IsThreatResearcher(membership.UserAccount.Id, supportAccountId, out category);
                    if (isThreatResearcher)
                    {
                        //Autofocus Trial role cannot be assigned if user is a Threat Researcher";
                        result.AutofocusTrialRoleAssignmentError = string.Format(RoleAssignmentErrorMessage,"Autofocus Trial", category.Name, string.IsNullOrEmpty(category.Value) ? " level" : @" """ + category.Value + @"""");
                        continue;
                    }
                    //Check if Autofocus Trial role can be assigned to user
                    if (!CanAssignAutofocusTrialRole(supportAccountId, membership.UserAccount.Id, out errorMessage))
                    {
                        result.AutofocusTrialRoleAssignmentError = errorMessage;
                        continue;
                    }
                }
                #endregion

                #region ATC Manager Role

                if (role.Id == (long) RoleType.ATCManager && !alreadyHasAtcManagerRole)
                {
                    var seatsAvailable = _securityRepository.IsAtcManagerSeatsAvailable(supportAccountId);
                    if (!seatsAvailable)
                    {
                        result.ShowATCManagerUseCountWarning = true;
                        continue;
                    }
                }

                #endregion

                #region Domain Administrator

                if (role.Id == (long)RoleType.DomainAdministrator)
                {
                    //var userAccount = _userAccountLogic.GetUserAccount(membership.UserAccount.Id, 1);

                    //var loginServiceLogic = new LoginServiceLogic();
                    //var userprofile = loginServiceLogic.GetUserProfile(userAccount.Email, userAccount.IpAddress, "CSP");

                    //if(userprofile == null)
                    //{
                    //    userprofile = loginServiceLogic.Register(userAccount.Email, "*",
                    //        userAccount.FirstName,
                    //        userAccount.LastName,"",
                    //        userAccount.Address.Address1,
                    //        userAccount.Address.Address2,
                    //        userAccount.Address.City,
                    //        "*",
                    //        userAccount.Address.PostalCode,
                    //        "*", userAccount.PhoneNumber,
                    //        "", false);

                    //    if (userprofile != null)
                    //    {
                    //       // sendactivation email to user
                    //    }
                    //}
                    
                }

                #endregion

                objectMembershipRolesDataAccessValues.Add(role.Id, membership.Id);
			}

            //Remove Autofocus Trial Role when Threat Researcher role is added to Autofocus Trial user
            if (alreadyHasAutofocusTrialRole && (objectMembershipRolesDataAccessValues.ContainsKey((long)RoleType.ThreatResearcher) ||
                objectMembershipRolesDataAccessValues.ContainsKey((long)RoleType.GroupThreatResearcher)))
            {
                objectMembershipRolesDataAccessValues.Remove((long)RoleType.AutoFocusTrial);
                objectMembershipRolesDataAccessValues.Remove((long)RoleType.GroupAutoFocusTrial);
                result.AutofocusTrialRoleWarning = "Autofocus Trial role is removed and Threat Researcher role is added";
            }

			updateHelper = new ListUpdateHelper(databaseMembershipRolesDataAccessValues, objectMembershipRolesDataAccessValues, membership.LastUpdateByUserAccountId,
							membershipRolesDataAccess.Create, membershipRolesDataAccess.Delete);
			updateHelper.UpdateDatabase();

			return result;
		}


		/// <summary>
		/// This Method is used to retrive list of Memberships based on Support Account ID
		/// </summary>
		/// <param name="supportAccountId"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public List<SupportAccountMembershipModel> GetSupportAccountMembers(long supportAccountId, long restrictedByUserAccountId, DataParameters parameters,out List<SupportAccountDomainModel> uniqueAccountDomainModels) {
            MembershipDataAccess membershipDataAccess = new MembershipDataAccess();
            var memberships = membershipDataAccess.ReadSupportAccountMembers(supportAccountId, restrictedByUserAccountId, parameters);

            var membershipList = Mapper.Map<List<SupportAccountMembershipModel>>(memberships);
            //var membershipList = _membershipDataAccess.ReadSupportAccountMembers(supportAccountId, restrictedByUserAccountId, parameters);

            List<SupportAccountDomainModel> uniqueDomains = new List<SupportAccountDomainModel>();
            string domain = string.Empty;
            var isDomainAdmin = _securityRepository.HasRole(supportAccountId, restrictedByUserAccountId, RoleNames.DomainAdministrator);

            MembershipRolesDataAccess membershipRolesDataAccess = new MembershipRolesDataAccess();
            bool isTacAdmin = _currentStateService.GetCurrentUser().UserAccount.Email.Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION) && membershipRolesDataAccess.GetRolesByUserAccountId(restrictedByUserAccountId, ApiConstants.PANW_SUPPORT_ACCOUNT_ID).Select(x => x == RoleNames.TACAdmin).Any();


            foreach (var membership in membershipList)
            {
                var role = membership.Roles.SingleOrDefault(r => r.Value == RoleType.AutoFocusTrial.ToString("D"));
                if (role != null)
                    role.Name = string.Format("{0} (Expiration Date - {1:d})", RoleNames.AutofocusTrial, membership.AutoFocusTrialExpirationDate);

                membership.IsMfaRequired = _mfaLogic.IsMfaRequiredForAnyMembership(supportAccountId, membership.UserAccountId);
                membership.PreviousRoles = membershipDataAccess.GetPreviousRolesByMembershipId(membership.MembershipId);

                domain = membership.Email.Split('@')[1];
                membership.Domain = domain;
               
               if(!string.IsNullOrEmpty(domain) && !(uniqueDomains.Any(x=> x.Domain == domain)))
                   uniqueDomains.Add(new SupportAccountDomainModel(){ Domain = domain});

               if ((isDomainAdmin || isTacAdmin) && !_currentStateService.GetIsImpersonated())
               {
                   membership.Editable = true;
               }
               else
               {
                   //Non-domain admin i.e super user shouldn't edit users with domain admin role
                   membership.Editable =
                       (membership.Roles.SingleOrDefault(
                           r => r.Value == ((int) RoleType.DomainAdministrator).ToString())) == null;
               }
            }

            uniqueAccountDomainModels = uniqueDomains;
            return membershipList;
		}

		public byte[] GetSupportAccountMembersCsv(long supportAccountId, long restrictedByUserAccountId, string searchField, string searchValue)
        {
            var uniquedomains= new List<SupportAccountDomainModel>();
			var dataParameters = new DataParameters { SearchField = searchField, SearchValue = searchValue };

			var data = GetSupportAccountMembers(supportAccountId, restrictedByUserAccountId, dataParameters, out uniquedomains);
            
		    //MembershipDataAccess membershipDataAccess = new MembershipDataAccess();
            // CSP- 15643
		    //List<SupportAccountMembershipModel> deletedUsers = membershipDataAccess.GetDeletedMembershipsBySupportAccountId(supportAccountId);

		    //if (deletedUsers.Any())
		    //    data.AddRange(deletedUsers);

			var cc = new CsvContext();
            
			var outputFileDescription = new CsvFileDescription {
				SeparatorChar = ',',
				FirstLineHasColumnNames = true,
				QuoteAllFields = true,
                IgnoreUnknownColumns = true,
			    EnforceCsvColumnAttribute = true
			};

			using (var ms = new MemoryStream())
			using (TextWriter tw = new StreamWriter(ms)) {
                var membersCsv = Mapper.Map<MembershipListItemCsv[]>(data);

                    var members = membersCsv.Select(m => new { m.Name, m.Email, m.ActivationDate, m.ExpirationDate, m.Description, m.PreviousRoles,m.Roles,m.LastUpdatedBy ,m.CreatedBy, m.Comments});
		   
			    
			    var membersCsvNoMfa = members
			            .Select(x => new MembershipListItemCsv()
			            {
			                Email = x.Email,
                            Name = x.Name,
                            Roles = x.Roles,
                            CreatedBy = x.CreatedBy,
                            LastUpdatedBy = x.LastUpdatedBy,
                            ActivationDate = x.ActivationDate,
                            Description = x.Description,
                            Comments = x.Comments,
                            ExpirationDate = x.ExpirationDate,
                            PreviousRoles = x.PreviousRoles
                       
			            })
			            .ToList();

                    cc.Write(membersCsvNoMfa, tw, outputFileDescription);

				tw.Flush();
				ms.Position = 0;

				return ms.ToArray();
			}
		}


		/// <summary>
		/// This Function is used to Retrieve the List of MemberShips based on DeviceID
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns>List<Membership></returns>
		public List<MembershipListItem> GetDeviceMembers(long deviceId, long restrictedByUserAccountId, DataParameters parameters) {

			MembershipDataAccess membershipDataAccess = new MembershipDataAccess();

			return membershipDataAccess.ReadDeviceMembers(deviceId, restrictedByUserAccountId, parameters);
		}


		/// <summary>
		/// This Function is used to Retrive Membership by GroupId.
		/// </summary>
		/// <param name="groupId">groupId</param>
		/// <returns>List<Membership></returns>
		public List<MembershipListItem> GetGroupMembers(long groupId, long restrictedByUserAccountId, DataParameters parameters) {

            MembershipDataAccess membershipDataAccess = new MembershipDataAccess();
            var supportAccountId = GetSupportAccountIdByGroupId(groupId);
            var hasActiveCanaryLicense = _securityRepository.HasActiveCanaryLicense(supportAccountId ?? 0);

            var groupMembers = membershipDataAccess.ReadGroupMembers(groupId, restrictedByUserAccountId, parameters);
            var autoFocusTrialUsers = _securityRepository.GetAutoFocusTrialUsersForSupportAccount(supportAccountId ?? 0);

            var isDomainAdmin = _securityRepository.HasRole(supportAccountId ?? 0, restrictedByUserAccountId,RoleNames.DomainAdministrator);

            foreach (var groupMember in groupMembers)
            {
                groupMember.HasActiveAutofocusLicense = hasActiveCanaryLicense;
                groupMember.EnableEmailMfa = groupMember.IsMfaEnabled && groupMember.MultiFactorAuthType.Equals(MultiFactorAuthType.Email);
                var autofocusTrialUser = autoFocusTrialUsers.SingleOrDefault(u => u.UserAccountId == groupMember.UserAccountId);

                if (autofocusTrialUser != null)
                {
                    groupMember.AutoFocusTrialUser = hasActiveCanaryLicense && autofocusTrialUser.IsActive && autofocusTrialUser.ExpirationDate > DateTime.Now;
                    groupMember.AutoFocusTrialExpirationDate = autofocusTrialUser.ExpirationDate;
                }

                groupMember.IsMfaRequired = _mfaLogic.IsMfaRequiredForAnyMembership(supportAccountId ?? 0, groupMember.UserAccountId);

                if (isDomainAdmin)
                    groupMember.Editable = true;
                else
                {
                    //Non-domain admin i.e group super user shouldn't edit users with domain admin role
                    groupMember.Editable = (groupMember?.Roles?.SingleOrDefault(r => r.Value == ((int)RoleType.DomainAdministrator).ToString())) == null;
                }
            }

            return groupMembers;
		}

		public byte[] GetGroupMembersCsv(long groupId, long restrictedByUserAccountId, string searchField, string searchValue) {

			var dataParameters = new DataParameters { SearchField = searchField, SearchValue = searchValue };

			var data = GetGroupMembers(groupId, restrictedByUserAccountId, dataParameters);

			var cc = new CsvContext();
			var outputFileDescription = new CsvFileDescription {
				SeparatorChar = ',',
				FirstLineHasColumnNames = true,
				QuoteAllFields = true,
			};

			using (var ms = new MemoryStream())
			using (TextWriter tw = new StreamWriter(ms)) {

				var groupMembersCsv = Mapper.Map<MembershipListItemCsv[]>(data);
				cc.Write(groupMembersCsv, tw, outputFileDescription);

				tw.Flush();
				ms.Position = 0;

				return ms.ToArray();
			}
		}


		/// <summary>
		/// This method is used to Delete MemberShip 
		/// </summary>
		/// <param name="membershipId"></param>
		/// <param name="deletedByUserAccountId"></param>
		/// <returns>void</returns>
		public void DeleteMembership(long membershipId, long deletedByUserAccountId) {

			MembershipDataAccess membershipDataAccess = new MembershipDataAccess();

			membershipDataAccess.Delete(membershipId, deletedByUserAccountId);
		}


		/// <summary>
		/// This method is used to create Membership Role
		/// </summary>
		/// <param name="membershipId"></param>
		/// <param name="roleId"></param>
		/// <param name="createdByUserAccountId"></param>
		/// <returns>void<Membership></returns>
		public void CreateMembershipRole(long membershipId, long roleId, long createdByUserAccountId) {

			MembershipRolesDataAccess membershipRolesDataAccess = new MembershipRolesDataAccess();

			membershipRolesDataAccess.Create(membershipId, roleId, createdByUserAccountId);
		}


		/// <summary>
		/// This method is used to get all Roles based on membershipId
		/// </summary>
		/// <param name="membershipId"></param>
		/// <param name="parameters"></param>
		/// <returns>List<Role></returns>
		public List<Role> GetMembershipRoles(long membershipId, DataParameters parameters) {

			MembershipRolesDataAccess membershipRolesDataAccess = new MembershipRolesDataAccess();

			return membershipRolesDataAccess.Read(membershipId, parameters);
		}


		public bool HasMembershipforSupportAccountByEmail(string email, long supportAccountId) {
			MembershipDataAccess membershipDataAccess = new MembershipDataAccess();
			return membershipDataAccess.HasMembershipforSupportAccountByEmail(email, supportAccountId);
		}

		public bool HasMembershipforGroupByEmail(string email, long groupId) {
			MembershipDataAccess membershipDataAccess = new MembershipDataAccess();
			return membershipDataAccess.HasMembershipforGroupByEmail(email, groupId);
		}

		public bool HasMembershipforDeviceByEmail(string email, long deviceId) {
			MembershipDataAccess membershipDataAccess = new MembershipDataAccess();
			return membershipDataAccess.HasMembershipforDeviceByEmail(email, deviceId);
		}

        public void AssignMembershipRoles(long membershipId, long userAccountId, long supportAccountId, long createdByUserAccountId, bool assignSuperUser = false, bool allowedOrg = true)
        {
			var membershipDataAccess = new MembershipDataAccess();

            membershipDataAccess.AssignMembershipRoles(membershipId, userAccountId, supportAccountId, createdByUserAccountId, assignSuperUser, allowedOrg);
		}

		public void AddUpdateSalesforceContact(Membership membership, string lastUpdateByEmail, long lastUpdateByUserAccountId) {
			if (membership != null) {
				long supportAccountId = 0;
				var userAccountId = (membership.UserAccount != null) ? membership.UserAccount.Id : 0;

				if (membership.SupportAccountId > 0) {
					supportAccountId = membership.SupportAccountId;
				} else if (membership.GroupId > 0) {
					//Changed to lastUpdateByUserAccountId as this is the logged in user that has permission to see the group, not the user that was added. Otherwise it fails
					var group = _groupLogic.GetGroup(membership.GroupId, lastUpdateByUserAccountId);

					supportAccountId = (group != null) ? group.SupportAccountId : 0;
				}

				// Salesforce Contact Creation
				var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
				var repository = DependencyResolver.Current.GetService<IRepository>();

				var userAccount = userAccountLogic.GetUserAccount(userAccountId, lastUpdateByUserAccountId);
				//Changed to lastUpdateByUserAccountId as this is the logged in user that has permission to see the support account, not the user that was added. Otherwise it fails
				var supportAccount = repository.Get<Supportaccount>(supportAccountId);
				var salesforceId = (supportAccount != null) ? supportAccount.SalesforceAccountId : string.Empty;
				_salesforceService.AddUpdateSalesforceContact(userAccount, supportAccountId, salesforceId, membership.Id, lastUpdateByUserAccountId);
			}
		}
        public void DeleteSalesforceContact(string email, long supportAccountId,long lastUpdateByUserAccountId)
        {
            if (email != null)           {               

                // Salesforce Contact Delete
                var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
                var repository = DependencyResolver.Current.GetService<IRepository>();
                var supportAccount = repository.Get<Supportaccount>(supportAccountId);               
                _salesforceService.DeleteSalesforceContactsAccount(email, supportAccountId);
            }
        }

        public long? GetSupportAccountIdByGroupId(long groupId) {
			return _groupLogic.GetSupportAccountIdByGroupId(groupId);
		}

		public bool HasAccessToSupportAccount(long supportAccountId, long userAccountId) {
			return _securityRepository.CanUserSignInToSupportAccount(userAccountId, supportAccountId);
		}

	    public long GetNumberOfPendingMembershipsBySupportAccountId(long supportAccountId, List<RoleSecurity> roleSecurities)
	    {
	        if (!roleSecurities.Any(x => x.SecurityClassification == RoleSecurity.SecurityClassifications.Super))
	            return 0;
	        
            return _membershipRepository.GetNumberOfPendingMembershipsBySupportAccountId(supportAccountId);
	    }

	    public IEnumerable<PendingMembershipModel> GetAllPendingMemberships(long supportAccountId)
	    {
	        return _membershipRepository.GetAllPendingMemberships(supportAccountId);
	    }

	    public string GetMembershipResponseGuidBId(long id) {
			var responseGuid = string.Empty;
			var membership = _repository.Get<EntityClasses.Membership>(id);

			if (membership != null) {
				responseGuid = membership.MembershipResponseGuid;
			}

			return responseGuid;
		}

		public EntityClasses.Membership GetMembershipByResponseGuid(string id) {
			var membership = _repository.Query<EntityClasses.Membership>().FirstOrDefault(m => m.MembershipResponseGuid == id);

			return membership;
		}

		public void UpdateMembershipStatus(EntityClasses.Membership membership) {
			_repository.SaveAndCommit(membership);
		}

		public string GetUserEmailByMembershipId(long id) {
			var userAccount =
				(from user in _repository.Query<Useraccount>()
				 join membership in _repository.Query<EntityClasses.Membership>() on user equals
					 membership.UserAccountId_Useraccount_
				 where
					 membership.MembershipId == id
				 select user).FirstOrDefault();

			return (userAccount != null) ? userAccount.Email : string.Empty;
		}

		public string GetSupportAccountNameByMembershipId(long id) {
			var supportAccount =
				(from account in _repository.Query<Supportaccount>()
				 join membership in _repository.Query<EntityClasses.Membership>() on account equals
					 membership.SupportAccountId_Supportaccount_
				 where
					 membership.MembershipId == id
				 select account).FirstOrDefault();

			return (supportAccount != null) ? supportAccount.Name : string.Empty;
		}

        public List<MembershipDetailForSfdcContactSync> GetAllMembershipsWithoutSfdcContact()
        {
            return _membershipRepository.GetAllMembershipsWithoutSfdcContact().ToList();
        }

        public MembershipDetails GetMembershipDetailsBySfdcPortalUsername(string portalUsername, bool hasMultipleAccounts = false)
        {
            return _membershipRepository.GetMembershipDetailsBySfdcPortalUsername(portalUsername, hasMultipleAccounts);
        }

        public int GetCountOfUsersByRole(long supportAccountId, int roleId)
        {
            return _membershipDataAccess.GetCountOfUsersByRole(supportAccountId, roleId);
        }

        public string GetSalesforceContactIdByUserAndSupportAccountId(long userAccountId, long supportAccountId)
        {
            return _membershipRepository.GetSalesforceContactIdByUserAndSupportAccountId(userAccountId, supportAccountId);
        }

        #region Threat Researcher and Autofocus Trial Role

        public bool IsThreatResearcher(long userAccountId, long supportAccountId, out string errorMessage)
	    {
	        errorMessage = string.Empty;
            NameValue category;
            //Check if User is already a Threat Researcher at Account or Group level
            var isThreatResearcher = _securityRepository.IsThreatResearcher(userAccountId, supportAccountId, out category);
            if (isThreatResearcher)
            {
                errorMessage = string.Format(RoleAssignmentErrorMessage,"Threat Researcher", category.Name, string.IsNullOrEmpty(category.Value) ? " level" : @" """+ category.Value + @"""");
                return true;
            }
	        return false;
	    }

        private bool CanAssignAutofocusTrialRole(long supportAccountId, long userAccountId, out string errorMessage)
        {
            errorMessage = string.Empty;
            bool autofocusTrialAllowed = false;
            //Check if Autofocus license is active
            var hasActiveCanaryLicense = _securityRepository.HasActiveCanaryLicense(supportAccountId);
            if (hasActiveCanaryLicense)
            {
                //AutoFocus Trial is not allowed if AutoFocus site license is within 30 days of expiration 
                autofocusTrialAllowed = _securityRepository.AutofocusTrialAllowed(supportAccountId);
                if (autofocusTrialAllowed)
                {
                    //Check if AF Trial Role has not expired for the user
                    var isAutofocusTrialRoleActive = _securityRepository.IsAutofocusTrialRoleActive(userAccountId, supportAccountId);
                    if (isAutofocusTrialRoleActive.HasValue)
                    {
                        if (isAutofocusTrialRoleActive.Value)
                        {
                            //Check if AutoFocus Trial role has already been assigned to this User
                            NameValue category;
                            var isAutofocusTrialUser = _securityRepository.IsAutoFocusTrialUser(userAccountId, supportAccountId, out category);
                            if (isAutofocusTrialUser)
                            {
                                errorMessage = string.Format("Autofocus Trial role is already assigned to user at {0} {1}", category.Name,
                                               string.IsNullOrEmpty(category.Value) ? " level" : @" """ + category.Value + @"""");
                                autofocusTrialAllowed = false;
                            }
                        }
                        else
                        {
                            errorMessage = "Autofocus Trial role has expired for this user. It can only be assigned one time per user";
                            autofocusTrialAllowed = false;
                        }
                    }
                }
                else
                {
                    errorMessage = "AutoFocus License expires in less than 30 days. Trial role cannot be assigned";
                }
            }
            else
            {
                errorMessage = "Autofocus License has expired, Trial role cannot be assigned";
            }

            return autofocusTrialAllowed;
        }

	    private bool CanAssignThreatResearcherRole(long userAccountId, long supportAccountId, out string errorMessage)
	    {
            errorMessage = string.Empty;
	        bool canAssignThreatResearcherRole = true;
            //Check if Autofocus License is not expired
	        var hasActiveCanaryLicense = _securityRepository.HasActiveCanaryLicense(supportAccountId);
	        if (hasActiveCanaryLicense)
	        {
	            //Check if User is already a Threat Researcher at Account or Group level
	            NameValue category;
	            var isThreatResearcher = _securityRepository.IsThreatResearcher(userAccountId, supportAccountId,out category);
	            if (isThreatResearcher)
	            {
                    canAssignThreatResearcherRole = false;
                    errorMessage = string.Format(RoleAssignmentErrorMessage, "Threat Researcher", category.Name, string.IsNullOrEmpty(category.Value) ? " level" : @" """ + category.Value + @"""");
	            }
	        }
	        else
	        {
	            canAssignThreatResearcherRole = false;
                errorMessage = "Autofocus License has expired, Threat Researcher role cannot be assigned";
	        }

	        return canAssignThreatResearcherRole;
	    }

        #endregion

        #region Enable/Disable 2 Factor Authentication

	    private bool ProcessMfaRegistration(Membership membership, long supportAccountId, out MultiFactorAuthType multifactorAuthType)
	    {
            var isMfaEnabled = membership.IsMfaEnabled;

            //Enable 2 Factor Authentication for user
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();
            var user = userAccountDataAccess.Read(membership.UserAccount.Id, membership.CreatedByUserAccountId);
            multifactorAuthType = user.MultiFactorAuthType;
            if (membership.IsMfaEnabled)
            {
                var enableEmailMfa = user.IsMfaEnabled && membership.EnableEmailMfa &&
                                     user.MultiFactorAuthType.Equals(MultiFactorAuthType.GoogleAuthenticator);

                isMfaEnabled = enableEmailMfa
                                ? _mfaLogic.EnableMfa(user, membership.LastUpdateByUserAccountId, MultiFactorAuthType.Email, string.Empty, true)
                                : user.IsMfaEnabled || _mfaLogic.EnableMfa(user, membership.LastUpdateByUserAccountId);
                multifactorAuthType = enableEmailMfa ? MultiFactorAuthType.Email : user.IsMfaEnabled ? user.MultiFactorAuthType : MultiFactorAuthType.Email; 
            }
            //Disable 2 FA if enabled
            else if (user.IsMfaEnabled && !membership.IsMfaEnabled)
            {
                var isMfaDisabled = _mfaLogic.DisableMfa(user, supportAccountId, membership.LastUpdateByUserAccountId);
                isMfaEnabled = !isMfaDisabled;
                multifactorAuthType = isMfaDisabled ? MultiFactorAuthType.None : user.MultiFactorAuthType;
            }
	        return isMfaEnabled;
	    }
        #endregion
    }
}
