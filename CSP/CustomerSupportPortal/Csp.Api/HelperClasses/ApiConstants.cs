using System.Collections.Generic;


namespace Csp.Api.HelperClasses {
    public static class ApiConstants {

        public const string DEVICE_CATEGORY = "Device Tag";
        public const string DEVICE_CATEGORIES = "Device Tags";
        public const string ROLES = "Role";
        public const string ACCOUNTTYPES = "Account Type";
        public const string MEMBERSHIP = "Membership";
        public const string WILDFIRENFR = "WildFire";

        public const string PANW_EMPLOYEE_EMAIL_EXTENSION = "@paloaltonetworks.com";
        public const string PANW_EMPLOYEE_DOMAIN = ".paloaltonetworks.com";

        public const string HYPERVISOR_VERSION_GOOGLE = "VM-GCP";
        public const string HYPERVISOR_VERSION_AWS = "VM-AWS";
        public const string HYPERVISOR_VEERSION_AZURE = "VM-AZR";
        public const long PANW_SUPPORT_ACCOUNT_ID = 245;
        public const long PANW_SYSTEM_USER_ACCOUNT_ID = 1;
        public const string PANW_SYSTEM_USER_EMAIL = "donotreplycsp@paloaltonetworks.com";
        public const long PANW_COVEO_ANONYMOUS_USER_ACCOUNT_ID = 99095;
        public const string PANW_COVEO_ANONYMOUS_USER_EMAIL = "svc-kbsiteguest@paloaltonetworks.com";

        public const string RMA_ALERTS_ICONCLASS = "fa fa-ambulance";
        public const string CASE_ALERTS_ICONCLASS = "fa fa-briefcase";

        public const string UPDATE = "update";
        public const string DELETE = "delete";


        public const string APPLICATION_NAME = "csp";
        public const string REQUIRES_SU_APPROVAL = "Require Superuser Approval Setting";
        public const string PENDING = "Pending";
        public const string COMPLETE = "Complete";
        public const string LICENSING_API_CHANGE = "Licensing Api Change";
        public const string LICENSING_API_EXPIRY = "Licensing Api Expiry Notification";

        public const string VM_ELA_GRANT_TOKEN_ACCESS = "VM ELA Grant Token Access";
        public const string VM_ELA_GRANT_TOKEN_ACCEPTED = "VM ELA Grant Token Accepted";
        public const string VM_ELA_TOKEN_USAGE_NOTIFICATION = "VM ELA Token Usage Notification";

        public const string DAY_ONE_CONFIG_NOTIFICATION = "Loadable Configuration Notification";

        public const string USER_TRANSFER_NOTIFICATION = "User Account Transfer Notification";

        public const string MEMBERSHIP_DECISION = "Membership Decision";
        public const string MEMBERSHIP_DECISION_PENDING = "Membership Decision Pending";

        public const string SerialNumberNotValid_Message = "Serial number does not exist.";
        public const string SerialNumberNotEligible_Message = "The serial number entered is for a device that needs to be registered by magic link, please check your email or contact support.";
        public const string SerialNumberRegsitered_Message = "This serial number is already registered.";

        public const string ConsentRequest = "Consent Request";

        public const string CN_SN_SERIES_ELA_GRANT_TOKEN_ACCESS = "CN-Series ELA Grant Token Access";
        public const string CN_SN_SERIES_ELA_GRANT_TOKEN_ACCEPTED = "CN-Series ELA Grant Token Accepted";
        public const string CN_SN_SERIES_ELA_TOKEN_USAGE_NOTIFICATION = "CN-Series ELA Token Usage Notification";

        public const string PanoramaProvisionSuccessMessage = "You have successfully provisioned panorama.";
        public const string DeploymentProfileBackupMessage = "Before Update";

        public const string UserNotFound = "UserName not found";
        public const string GoogleAuthFailedMessage = "An error occurred while enabling/disabling Google Auth.";
        public const string WildfireDeleteSuccessMessage = "Successfully deleted {0} data";
        public const string WildfireDeleteFailedMessage = "Failed to delete {0} data.  Please try again later.";

        public const string LicenseDeactivationSucessMessage = "License Deactivation Successful.";
        public const string LicenseDeactivationFailedMessage = "An error occurred during license deactivation.  Please contact customer support.";

        public const string DeviceTransferSuccessMessage = "Device transfer successful.";
        public const string DeviceTransferRejectedMessage = "Device transfer rejected.";
        public const string UserType = "Cust";

        public const string BlockListedDomainMessage = "Domain name of the email address is not supported by Palo Alto Networks. If this is your email address, please open a case at https://support.paloaltonetworks.com.";
        public const string PublicDomainMessage = "Please Enter a Public Domain Email Address\r\n" +
                  " You will be added to a CSP account that requires public domain email addresses, e.g., sally@gmail.com.\r\n" +
                  " If you have questions on your email address, please contact a Super User of this CSP account.";
        public const string PrivateDomainMessage = "Please Enter a Private Domain Email Address\r\n" +
                  " You will be added to a CSP account that requires private domain email addresses, e.g., sally@acme.com.\r\n" +
                  " If you have questions on your email address, please contact a Super User of this CSP account.";

        public const string TOKEN_SUMMARY_GRAPHQL = "{{\"query\":\"query ({0})"
                + "{{tokens({1},excludeOverlyExpire: true){{tokenId,salesAccountId,salesAccountName,supportAccountId,supportType,productId,productName,"
                + "sku,isEval,ascAccountId,effectiveDate,expirationDate,unitOfMeasure,quantityPurchased,quantityNetAllocated,quantityNetBalance,"
                + "quantityNetConsumption,parentTokenId,discontinuedSubscriptions,creditPoolRenewalStatus, status,transactionType,poolName,genType,isAirgapEligible}}}}\","
                + "\"variables\":{{{2}}}}}";

        public const string TOKEN_SUMMARY_FROM_ENTITLEMENT_GRAPHQL =
"{{\"query\":\"query ($tokenId: [Long], $status: [String], $productId: [String]){{license(tokenId:$tokenId,status:$status,productId:$productId){{"
+ "        entitlementGroupId,"
+ "        salesAccountId,"
+ "        salesAccountName,"
+ "        supportAccountId,"
+ "        ascAccountId,"
+ "        supportType,"
+ "        entitlements {{"
+ "            entitlementId,"
+ "            status,"
+ "            tokenId,"
+ "            productId,"
+ "            productName,"
+ "            productType,"
+ "            sku,"
+ "            genType,"
+ "            orderDate,"
+ "            startDate,"
+ "            endDate,"
+ "            instanceUrl,"
+ "            licenseFeatures {{"
+ "                quantity,"
+ "                unitOfMeasure"
+ "            }}, "
+ "            configuration {{"
+ "                region"
+ "            }}"
+ "        }}"
+ "}}}}\",\"variables\":{{\"tokenId\":[{0}],\"status\":\"{1}\",\"productId\":\"{2}\"}}}}";

        public const string TOKENID_TYPE = "$tokenId: [Long]";
        public const string TOKENID_VARIABLE = "tokenId:$tokenId";
        public const string TOKENID_VALUE = "\"tokenId\":[{0}]";

        public const string SUPPORTACCOUNTID_TYPE = "$supportAccountId: [Long]";
        public const string SUPPORTACCOUNTID_VARIABLE = "supportAccountId:$supportAccountId";
        public const string SUPPORTACCOUNTID_VALUE = "\"supportAccountId\":{0}";

        public const string SUPPORTACCOUNTID_PRODUCTID_TYPE = "$supportAccountId: [Long], $productId: [String]";
        public const string SUPPORTACCOUNTID_PRODUCTID_VARIABLE = "supportAccountId:$supportAccountId, productId:$productId";
        public const string SUPPORTACCOUNTID_PRODUCTID_VALUE = "\"supportAccountId\":{0},\"productId\":{1}";

        public const string TOKEN_SUMMARY_QUANTITY_ALLOCATION = "{{\"query\":\"query ($tokenId: [Long])"
                                                        + "{{quantityAllocation(tokenId:$tokenId){{allocationId,quantityAllocated,quantityConsumed,consumptionRenewalStatus}}}}\","
                                                        + "\"variables\":{{\"tokenId\":{0}}}}}";

        public const string TOKEN_QUANTITY_ALLOCATION = "{{\"query\":\"query ($allocationId: [String])"
                                                + "{{quantityAllocation(allocationId:$allocationId){{allocationId,quantityAllocated,quantityConsumed}}}}\","
                                                + "\"variables\":{{\"allocationId\":\"{0}\"}}}}";

        public const string UserAccountEmailPreferencesPath = "{0}/v1/support-user/email-preferences/{1}";
        public const string UserAccountDataPath = "{0}/v1/users?email={1}";
        public const string OktaSSODataPath = "{0}/sso/v1/user-status?email={1}";
        public const string CspMembershipPath = "{0}/v1/memberships?support_account_id={1}&email={2}";
        public const string OktaPasswordResetPath = "{0}/api/okta/v1/handle-status?email={1}&status={2}";

        public const string SupportUserAccountGetDataPathWithoutSearch = "{0}/v1/memberships/support-account/{1}?page={2}&size={3}&sort={4}&status={5}&requestor_user_account_id={6}&extension={7}";
        public const string UserAccountGetPrivileges = "{0}/v1/privileges?support_account_id={1}&user_account_id={2}";
        public const string SupportUserAccountGetDataPathWithSearch = "{0}/v1/memberships/support-account/{1}?search_field={2}&search_value={3}&page={4}&size={5}&sort={6}&status={7}&requestor_user_account_id={8}&extension={9}";
        public const string SupportUserAccountAddDataPath = "{0}/v1/memberships";
        public const string SupportUserAccountUpdateDataPath = "{0}/v1/membership";
        public const string SupportUserAccountDeleteDataPath = "{0}/v1/memberships/{1}?support_account_id={2}&user_account_id={3}";
        public const string SupportUserAccountGetPendingDataPath = "{0}&requestor_user_account_id={1}";
        public const string AssignableRolesEndpointUrl = "{0}/v1/roles/assignable-roles?user_account_id={1}&support_account_id={2}";
        public const string ImpersonateServiceUrl = "{0}/impersonate/v1";
        public const string SiteLicenseEndpoint = "{0}/entitlements/v1/sitelicense/{1}";
        public const string SideNavigationMenuEndpoint = "{0}/v1/menu-items?user_account_id={1}&support_account_id={2}";
        public const string PendoDetailsEndpoint = "{0}/v1/pendo/create-jwt-and-sign";
        public const string SideNavigationSubMenuEndpoint = "{0}/v1/sub-menu-items?user_account_id={1}&support_account_id={2}&menu_item_id={3}";
        public const string SideNavigationCombinedMenuEndpoint = "{0}/v2/menu-items?user_account_id={1}&support_account_id={2}";
        public const string CreateUserEndpoint = "{0}/v1/users";
        public const string CountriesEndpoint = "{0}/v1/countries";
        public const string RegionsEndpoint = "{0}/v1/regions/{1}";
        public const string FiltersEndpoint = "{0}/v1/filters?filter={1}&user_account_id={2}&support_account_id={3}&category_id={4}";
        public const string FiltersWithoutCategoryEndpoint = "{0}/v1/filters?filter={1}&user_account_id={2}&support_account_id={3}";
        public const string AssetDataEndpoint = "{0}/v1/assets";
        public const string AssetAttributesEndpoint = "{0}/v1/asset-attributes?serial_number={1}";
        public const string AssetsDownloadEndpoint = "{0}/v2/assets-download";
        public const string AssetDataDownloadEndpoint = "{0}/v1/assets-download?user_account_id={1}&support_account_id={2}";
        public const string AscAssetDataDownloadEndpoint = "{0}/v1/asc-assets-download?support_account_id={1}&user_account_id={2}";
        public const string AssetMetaDataEndpoint = "{0}/v1/asset-metadata";
        public const string AssetTagEndpoint = "{0}/v2/asset-tag?support_account_id={1}&user_account_id={2}&serial_number={3}";
        public const string AssetTagByIdEndpoint = "{0}/v2/support-account/asset-tag?page={1}&size={2}&sort={3}";
        public const string AssetTagByIdWithoutSortEndpoint = "{0}/v2/support-account/asset-tag?page={1}&size={2}";
        public const string AssetTagByIdSortEndpoint = "{0}/v2/support-account/asset-tag?sort={1}";
        public const string AssetTagByIdWithoutParamEndpoint = "{0}/v2/support-account/asset-tag";
        public const string AssetDetailsEndpoint = "{0}/v1/asset-detail";
        public const string AssetNameUpdateEndpoint = "{0}/v1/devices/update-device-name";
        public const string PanOsVersionUpdateEndpoint = "{0}/v1/devices/update-pan-os-version";
        public const string RemarksUpdateEndpoint = "{0}/v1/devices/update-remarks";
        public const string LocationUpdateEndpoint = "{0}/v1/devices/update-location";
        public const string AuthCodeDetailsEndpoint = "{0}/v1/license/master-auth-code?support_account_id={1}&user_account_id={2}&auth_code={3}";
        public const string AscInfoEndpoint = "{0}/v1/asc-account?account_id={1}";
        public const string ConvertEvalAccountEndpoint = "{0}/v1/convert-eval-account";
        public const string CreateUpdateSelfRegistrationEndpoint = "{0}/v1/self-registration";
        public const string GetSelfRegistrationEndpoint = "{0}/v1/self-registration?support_account_id={1}&user_account_id={2}&registered_app_id={3}";
        public const string UpdateEmailDomainType = "{0}/v1/support-account/{1}/email-domain-types";
        public const string FlexCreditDetailsEndpoint = "{0}/v1/firewall-flex-credits";
        public const string ActivateTrialLicenseEndpoint = "{0}/v1/trial-licenses";
        public const string ActivateLicenseEndpoint = "{0}/v1/license";
        public const string TrialLicenseEndpoint = "{0}/v1/trial-licenses?support_account_id={1}&user_account_id={2}&serial_number={3}";
        public const string LicenseActivationTypesEndpoint = "{0}/v1/activation-types?user_account_id={1}&support_account_id={2}&serial_number={3}";
        public const string PanOSVersionEndpoint = "{0}/v1/pan-os-versions";
        public const string DeviceInfoEndpoint = "{0}/v1/device-info";
        public const string BetaAccountsEndpoint = "{0}/v1/beta-accounts";
        public const string SessionManagementEndpoint = "{0}/v1/session-management?email={1}&support_account_id={2}";
        public const string GoogleAuthEndpoint = "{0}/v1/googleauthusers/{1}";
        public const string VirtualPlatformsEndpoint = "{0}/v1/virtual-platforms";
        public const string SupportAccountEndpoint = "{0}/v1/support-account/{1}";
        public const string UpdateSupportAccountEndpoint = "{0}/v1/support-account?support_account_id={1}&user_account_id={2}";
        public const string DefaultRolesEndpoint = "{0}/v1/roles/default-roles?support_account_id={1}&user_account_id={2}";
        public const string CspApiHeader = "csp-account-details";
        public const int CspRegisterAppId = 1;
        public const string BannerMessagesEndpoint = "{0}/v1/banner-messages/scope";
        public const string UpdateTrainingPartnerEndpoint = "{0}/v1/atp-preference";
        public const string AvailableTrainingPartnersEndpoint = "{0}/v1/atp-available?support_account_id={1}&user_account_id={2}";
        public const string SelectedTrainingPartnersEndpoint = "{0}/v1/atp-preference?support_account_id={1}&user_account_id={2}";
        public const string TrainingCoursesEndpoint = "{0}/v1/training-credits/courses?support_account_id={1}&user_account_id={2}&credit_id={3}";
        public const string TrainingCreditsEndpoint = "{0}/v1/training-credits?support_account_id={1}&user_account_id={2}";

        public const string COMMON_APIS =
            "/api/services/getpendodetails,/api/userprofile/getportalcontextdetails,/api/userprofile/getallnavigationlinks,/api/userprofile/getquickactionlinks,/api/userprofile/getavailableaccounts,/api/userprofile/isfirstlogin,/proxy/notificationsaccessor/token";

        public const string TOKEN_QUANTITY_CONSUMED = "{{\"query\":\"query($supportAccountId:[Long])"
                                           + "{{usageTransactionsV1(supportAccountId:$supportAccountId) {{tokenId,productId,productCode,productName,"
                                           + "salesAccountId,salesAccountName,supportAccountId,consumptionDate,unitOfMeasure,quantityConsumed,"
                                           + "usageType,usageDescription,companyCode,xrefPackId,xrefTransactionId,xrefVendorId,xrefCData}}}}\","
                                           + "\"variables\":{{\"supportAccountId\":{0}}}}}";
        public const string Prisma = "2c377801-fc79-4dbe-a388-988233972555";
        public const string Virtual = "92835631-5a44-4f3a-af2b-c27ddfba60fb";
        public const string Software = "6b8d648e-0ec8-433f-840f-d5cee0c33aca";
        public const string Subscription = "2ce5ff1c-7faa-4fc5-8378-acbbbea67156";
        public const string ActivateOfflineLicenseEndpoint = "{0}/v1/offline-license";
        public const string GetFeatureLicensesEndpoint = "{0}/v1/feature-license?support_account_id={1}&user_account_id={2}&serial_number={3}";
        public const string RollbackUpgradeEndpoint = "{0}/v1/vm-upgrade-rollback?support_account_id={1}&user_account_id={2}&serial_number={3}&transaction_id={4}";
        public const string AddOnsEndpoint = "{0}/v1/asset-add-ons?support_account_id={1}&tenant_id={2}";
        public const string AscContactEndpoint = "{0}/v1/asc-account?user_account_id={1}&support_account_id={2}&asc_support_account_id={3}";
        public const string DevicesForLicenseTransferEndpoint = "{0}/v1/license/destination-assets?serial_number={1}&support_account_id={2}&user_account_id={3}&search_archive={4}";
        public const string TransferAssetLicenseEndpoint = "{0}/v1/transfer-asset-licenses";
        public const string RegisterSpare = "{0}/v1/register-spare";
        public const string LoginServiceEndpoint = "{0}/v1/users/profiles?email={1}";
        public const string AssetTransferEndpoint = "{0}/v1/initiate-asset-transfer";
        public const string AssetTransferReviewEndpoint = "{0}/v1/asset-transfer";
        public const string BulkDeviceTransferEndpoint = "{0}/v1/initiate-bulk-transfer";
        public const string AssociateComponentEndpoint = "{0}/v1/chassis/associate-component";
        public const string UnRegisteredComponentEndpoint = "{0}/v1/chassis/validate-and-get-unregistered-components";
        public const string ChassisComponentsEndpoint = "{0}/v1/chassis-components?user_account_id={1}&support_account_id={2}&serial_number={3}";
        public const string ActivateSiteLicenseEndpoint = "{0}/v1/site-license";
        public static string WFApiKeyEndpoint = "{0}?SupportAccountId={1}";
        public const string MagicLinkEndpoint = "{0}/v1/generate-license-magic-link";
        public const string AssetLicenseEndpoint = "{0}/v2/asset-license?category_id={1}&support_account_id={2}&user_account_id={3}&serial_number={4}";
        public const string OAuthApiCredsEndpoint = "{0}/v1/oauth-creds?support_account_id={1}&user_account_id={2}";
        public const string OAuthApiCredsCreateRegenerateEndpoint = "{0}/v1/oauth-creds";
        public const string OAuthApiCustomerDetailsEndpoint = "{0}/v1/customer-details?client_id={1}";

        public const string AddressValidationEndpoint = "{0}/v1/validate-address";
        public const string RMAEligibilityEndpoint = "{0}/v1/rma-eligibility";
        public const string AssetTagsUpdateEndpoint = "{0}/v1/devices/update-asset-tags";
        public const string ChangetoSpareEndpoint = "{0}/v1/asset-sku-spare";
        public const string OneAssetMetaDataEndpoint = "{0}/v1/asset-metadata?support_account_id={1}&serial_number={2}&user_account_id={3}";
        public const string AssetDataInitialLoadEndpoint = "{0}/v1/data-load/{1}";

        public const string VMLegacyPanOs = "10.0.3_or-below";
        public const string VMNonlegacyPanOs = "10.0.4_or-above";
        public const string CNLegacyPanOs = "10.0";
        public static readonly List<string> CNNonlegacyPanOs = new List<string> { "10.1_and-above", "10.2" };
        public const string BootstrapContainerFileName = "bootstrapcontainer_{0}_{1}.tar.gz";
        public const int RenewalGracePeriod = 30;
        public const string WildfireDeviceAddLog = "Adding device(s) to user for the support account - {0}";
        public const string WildfireDeviceUpdateLog = "Updating device(s) to user for the support account {0}";
        public const string WildfireDeviceDeleteLog = "Deleteing device(s) from the support account {0}";
        public const string GetUserFromOkta = "{0}/v1/users/{1}";
        public const string UpdateUserOnOkta = "{0}/v1/apps/{1}/users/{2}";
        public const string CSP_APPLICATION = "CSP App";
        public const string Impersonation_Identifier = "parentAuth";
        public const string Impersonate_Session_Identifier = "simp";
        public const string RegisterHistory_Identifier = "registerHistory";

        public const string Login_Identifier = "support_kb_okta_lck";
        public const string Login_User_Session_Identifier = "shist";

        public const string ScheduledJobBulkRegistrationEndpoint = "{0}/api/ScheduledJobs/ProcessBulkRegistrations";
        public const string ScheduledJobSfdcSyncMembershipEndpoint = "{0}/api/ScheduledJobs/SyncMembershipsWithSfdc";
        public const string ScheduledJobBulkOtpRequestEndpoint = "{0}/api/ScheduledJobs/ProcessBulkOtpRequest";
        public const string ScheduledJobPinExpirationEndpoint = "{0}/api/ScheduledJobs/ProcessPinExpirationRequest";
        public const string ScheduledJobOtpExpirationEndpoint = "{0}/api/ScheduledJobs/ProcessOtpExpirationRequest";
        public const string ScheduledJobVmElaTokenEndpoint = "{0}/api/ScheduledJobs/ProcessVmElaTokenNotification";
        public const string ScheduledJobVmRegistrationEndpoint = "{0}/api/ScheduledJobs/ProcessVmRegistrations";
        public const string ScheduledJobBulkDeviceTransferEndpoint = "{0}/api/ScheduledJobs/ProcessBulkDeviceTransferRequest";
        public const string ScheduledJobEmailNotificationEndpoint = "{0}/api/ScheduledJobs/ProcessEmailNotificationStatusUpdate";
        public const string ScheduledJobEvalAccountCreationEndpoint = "{0}/api/ScheduledJobs/ProcessEvalAccountCreationRequest";

        public const string ProcessBulkRegistrationsErrorMessage = "Error in call to ProcessBulkRegistrations();";
        public const string SyncMembershipsWithSfdcErrorMessage = "Error in call to SyncMembershipsWithSfdc();";
        public const string ProcessPinExpirationRequestErrorMessage = "Error in call to ProcessPinExpirationRequest();";
        public const string ProcessOtpExpirationRequestErrorMessage = "Error in call to ProcessOtpExpirationRequest();";
        public const string ProcessVmElaTokenNotificationErrorMessage = "Error in call to ProcessVmElaTokenNotification();";
        public const string ProcessVmRegistrationsErrorMessage = "Error in call to ProcessVmRegistrations();";
        public const string ProcessBulkOtpRequestErrorMessage = "Error in call to ProcessBulkOtpRequest();";
        public const string ProcessBulkDeviceTransferErrorMessage = "Error in call to ProcessBulkDeviceTransfer();";
        public const string ProcessEmailNotificationStatusUpdate = "Error in call to ProcessEmailNotificationStatusUpdate();";
        public const string ProcessEvalAccountCreationErrorMessage = "Error in call to ProcessEvalAccountCreation();";
        public const string SupportCasePopupAlertMessage = "It appears there is an issue with your access to the Case Creation form. We've submitted a request to resolve it, and you can track the progress using Case # {0}";
        public const string SupportCaseIssueType = "Unable to access Support cases";
        public const string SupportCaseIssueDescription = "User does not have access to case creation for this CSP account {0}.";
        public const string SupportCaseErrorMessage = "User unable to create cases";

        // Cookie used for securing JWT Web Token
        public const string SupportSessionId = "support_session_id";
        public const string CspRefreshToken = "csp_refresh_token";
        public const string RegisterTenant = "{0}/v1/register-tenant";

        //Self-registratio endpoints
        public const string GenerateSelfRegistrationOtpEndpoint = "{0}/v1/self-registration-otp";
        public const string GeneratePasswordResetOtpEndpoint = "{0}/v1/generate-otp-email?email={1}";
        public const string IsIdpEnabledEndpoint = "{0}/v1/user/third-party-idp-enabled?email={1}";
        public const string ContactSupportEndpoint = "{0}/v1/wufoo";
        public const string ValidateSelfRegistrationOtpEndpoint = "{0}/v1/validate-self-registration-otp";
        public const string ValidatePublicDomain = "{0}/v1/registration/validate-public-domain?support_account_id={1}&email={2}";
        public const string EligibleAccount = "{0}/v1/support-accounts?email={1}";
        public const string ValidateEmail = "{0}/v1/registration/validate-email";
        public const string ValidateCaptcha = "{0}/v1/registration/validate-captcha";
        public const string ValidatePasswordResetCaptcha = "{0}/v1/validate/email-captcha";
        public const string ValidateProductSku = "{0}/v1/registration/product-sku/validate-email";
        public const string ValidateSelfRegistrationCode = "{0}/v1/registration/reg-code/validate-email";
        public const string ValidateClaisec = "{0}/v1/registration/claisec/validate-product";
        public const string ValidateNetsec = "{0}/v1/registration/netsec/validate-product";
        public const string UserRegistrationClaisecEndpoint = "{0}/v1/registration/claisec/users";
        public const string UserRegistrationNetsecEndpoint = "{0}/v1/registration/netsec/users";
        public const string UserRegistrationByCodeEndpoint = "{0}/v1/registration/reg-code/users";

        public const string LAST_LOGIN_DETAILS = "{{\"query\":\"query ($supportAccountId:[Long],$lastLoginDate: String)"
                                                + "{{firewallSummary(supportAccountId:$supportAccountId,lastLoginDate:$lastLoginDate){{ serialNumber, softwareVersion, supportAccountId, lastLoginDate}}}}\","
                                                + "\"variables\":{{\"supportAccountId\":{0},\"lastLoginDate\":\"{1}\"}}}}";

        public const string DeploymentProfileSuccessMessage = "Deployment profile saved successfully.";
        public const string DeploymentProfileFailedMessage = "The system is unable to process your deployment profile. Please try again or contact Customer Support.";
        public const string CloudServicesEndpoint = "{0}/v1/cloud-type";
        public const string CNSeriesFWFlexDevicesEndpoint = "{0}/v1/get-firewall-devices?user_account_id={1}&support_account_id={2}&type={3}&export={4}&search_field={5}&search_value={6}&page={7}&size={8}&sort={9}&subType={10}";
        public const string AirgapLicensesWithoutPanoramaEndpoint = "{0}/v1/download/air-gap-licenses-without-panorama?support_account_id={1}&user_account_id={2}&auth_code={3}";
        public const string AirgapLicensesWithPanoramaEndpoint = "{0}/v1/download/air-gap-licenses-with-panorama?support_account_id={1}&user_account_id={2}&auth_code={3}";
        public const string RetryFailedDeploymentProfileEndpoint = "{0}/v1/retry/failed-deployment-profile?deployment_profile_id={1}";
        public const string PUBLISH_TO_CDL_ENDPOINT = "coresvc/v1/sendToNovaPublishing";
        public const string DEVICE_TRANSFER_EVENT = "TRANSFER";
        public const string AddEmailToSupportAccountEndpoint = "{0}/v1/support-accounts/{1}?support-account-id={2}&email={3}";
        public const string DomainDetailEndpoint = "{0}/v1/domains?domain={1}";
        public const string HasMemberEndpoint = "{0}/v1/support-accounts/has-members/{1}";
        public const string DeleteExpiredTrialLicensesReason = "Delete Expired Trial Licenses";
        public const string DeleteTrialLicensesEndpoint = "{0}/v1/license/inactivate";
        public const string JwtTokenEndpoint = "{0}/v4/login";
        public const string JwtRefreshTokenEndpoint = "{0}/v4/refresh";
        public const string JwtTokenValidationEndpoint = "{0}/v4/verify-access-token";
        public const string AssetRecycleEndpoint = "{0}/v1/asset-recycle";
        public const string SingleOtpEndpoint = "{0}/v1/single-otp";

        //EA Endpoints
        public const string EaContractsEndpoint = "{0}/v1/ea-contract?support_account_id={1}&user_account_id={2}";
        public const string EaContractSummaryEndpoint = "{0}/v1/ea-summary?support_account_id={1}&user_account_id={2}&auth_code={3}";
        public const string EaAssociatedAccountsEndpoint = "{0}/v1/ea-support-account?support_account_id={1}&user_account_id={2}&auth_code={3}";
        public const string EaTotalFirewallsEndpoint = "{0}/v1/ea-firewalls?auth_code={1}&support_account_id={2}&user_account_id={3}&export={4}";
        public const string EaContractsHistoryEndpoint = "{0}/v1/ea-contract-history?support_account_id={1}&user_account_id={2}&auth_code={3}";

        public const string CloudCreditDetailsEndpoint = "{0}/v1/cloud/credit-details?support_account_id={1}&credit_pool_id={2}&page={3}&size={4}";
        public const string CLOUD_NGFW_PRODUCT_ID = "d76941df-8434-4c26-aedc-df49490a9425";
        public const string UserStatusEndpoint = "{0}/v1/user-status?email={1}";
        public const string EnableUserStatusEndpoint = "{0}/v1/memberships/deactivate-activate-user-memberships";

        public const string FirewallFlexDeploymentEndpoint = "{0}/v1/deployment-profile";
        public const string PanOSVersionWithVersionEndpoint = "{0}/v1/pan-os-versions?support_account_id={1}&user_account_id={2}&pan_os_version={3}";
        public const string PanOSVersionWithoutVersionEndpoint = "{0}/v1/pan-os-versions?support_account_id={1}&user_account_id={2}";
        public const string LicenseEntitlementEndpoint = "{0}/entitlements/v1/getuseraccount?support_account_id={1}";
        public const string EstimatedCreditsCostEndpoint = "{0}/v1/credit/estimate?support_account_id={1}&user_account_id={2}";

        public const string FirewallRegistrationEndpoint = "{0}/v1/register/firewall";
        public static string LoginUserServiceBaseUrl = "{0}/v2/users/{1}";
        public const string MembershipByRoleEndpoint = "{0}/v1/memberships?support_account_id={1}&role_id={2}&number={3}&size={4}&sort={5}";
        public const string NotifySuperUserEndpoint = "{0}/v1/notify-super-users";
        public const string EAStatusEndpoint = "{0}/v1/verify-ea-status?support_account_id={1}&serial_number={2}";
        public const string ProdcutActivationDataEndpoint = "{0}/v1/activation-page-data?support_account_id={1}";
        public const string MembershipExtensionEndpoint = "{0}/v1/memberships/extension";
        public const string VerifySfdcContactEndpoint = "{0}/v1/memberships/sales-force-contact";
        public const string CaseCreationHistoryEndpoint = "{0}/v1/case-creation-history-log";
        public const string CaseCreationHistoryDetailsEndpoint = "{0}/v1/case-creation-history-log?user_account_id={1}&support_account_id={2}";
        public const string LoginHelp = "{0}/Support/LoginHelp/EmailAuthentication";
        public const string RemoveBounceListEmailsEndpoint = "{0}/api/okta/v1/remove-from-bounce-list";
        public const string BulkOtpEndpoint = "{0}/v1/bulk-otp-publish";
        public const string UnBlockUserStatusEndpoint = "{0}/v1/memberships/expire-or-activate";
        public const string publishMessageEndpoint = "{0}/v1/common/publish";
        public const string CopilotUrl = "{0}/SupportAccount/GetHelp/{1}";
        public const string GetHelpPathUrl = "{0}%3Fpath%3D{1}";
        public const string GetUserThemeEndpoint = "{0}/v1/user-account-setting?user_account_id={1}&setting_name=THEME_MODE";
        public const string GetAiEligibilityCheckEndpoint = "{0}/v1/sase-redesign-pilot-release-check?user_account_id={1}&support_account_id={2}";
        public const string GetAiAgentImpersonationStatusEndpoint = "{0}/v1/ai-agent-impersonation/status?support_account_id={1}";
         public const string ImpersonateServiceV2Url = "{0}/impersonate/v2";
        public const string InvalidateJwtTokensUrl = "{0}/v1/logout/{1}";
    }
}