using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using PAN.Common;

namespace Csp.Api.Logic
{
    public class DeviceTransferLogic : IDeviceTransferLogic
    {

        private readonly IDeviceTransferRepository _deviceTransferRepository;
        private readonly ISupportAccountRepository _supportAccountRepository;
        private readonly ISecurityLogic _securityLogic;
        private readonly ILicenseRepository _licenseRepository;
        private readonly IWildfireRepository _wildfireRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IMembershipDataAccess _membershipDataAccess;
        private readonly IDeviceDataAccess _deviceDataAccess;
        private readonly IWildfireStore _wildfireStore;
        private readonly IAscDeviceRegistrationService _ascDeviceRegistrationService;
        private readonly IMembershipLogic _membershipLogic;
        private readonly ICurrentStateService _currentStateService;
        private readonly ISaasRegistrationRepository _saasRegistrationRepository;
        private readonly ILicenseLogic _licenseLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IWildfireWipeoutService _wildfireWipeoutService;
        private const int PageNumber = 0;
        private const int PageSize = 1;

        public DeviceTransferLogic(IDeviceTransferRepository deviceTransferRepository,
            ISupportAccountRepository supportAccountRepository, ISecurityLogic securityLogic, ILicenseRepository licenseRepository,
            IWildfireRepository wildfireRepository, IDeviceRepository deviceRepository,
            IDeviceDataAccess deviceDataAccess, IWildfireStore wildfireStore,
            IMembershipDataAccess membershipDataAccess, IAscDeviceRegistrationService ascDeviceRegistrationService,
            IMembershipLogic membershipLogic, ICurrentStateService currentStateService,
            ISaasRegistrationRepository saasRegistrationRepository, ILicenseLogic licenseLogic, IDeviceLogic deviceLogic,
            IWildfireWipeoutService wildfireWipeoutWebService)
        {
            _deviceTransferRepository = deviceTransferRepository;
            _supportAccountRepository = supportAccountRepository;
            _securityLogic = securityLogic;
            _licenseRepository = licenseRepository;
            _wildfireRepository = wildfireRepository;
            _deviceRepository = deviceRepository;
            _membershipDataAccess = membershipDataAccess;
            _deviceDataAccess = deviceDataAccess;
            _wildfireStore = wildfireStore;
            _ascDeviceRegistrationService = ascDeviceRegistrationService;
            _membershipLogic = membershipLogic;
            _currentStateService = currentStateService;
            _saasRegistrationRepository = saasRegistrationRepository;
            _licenseLogic = licenseLogic;
            _deviceLogic = deviceLogic;
            _wildfireWipeoutService = wildfireWipeoutWebService;
        }

        /// <summary>
        /// Only CSRs and Admins can see this
        /// and then only if the current device isn't pending a transfer
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="userAccount"></param>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public bool ShowBeginDeviceTransferButton(UserAccount userAccount, long supportAccountId, Device deviceData, bool showBootstrapContainerButton)
        {

            var perms = _securityLogic.GetSupportAccountPermissionsForPrincipal(supportAccountId, userAccount);

            if (!perms.Contains(Permission.BeginTransferDevice) || deviceData.IsTrapsManagementService || deviceData.IsCgx || _deviceLogic.IsNgfwDlpDevice(deviceData.Id) ||
                _deviceTransferRepository.IsDevicePendingTransfer(deviceData.Id))
                return false;

            if (deviceData.IsPanorama && showBootstrapContainerButton)
                return ShowPanoramaDeviceTransferButton(supportAccountId);

            return true;
        }

        /// <summary>
        /// Do not show 'Transfer Ownership' button: CSP account is associated with an valid NGFW DLP license (Expiration date =< 90 days)
        /// Show 'Transfer Ownership' button: CSP account is associated with an invalid NGFW DLP license(Expiration date > 90 days)
        /// </summary>		
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public bool ShowPanoramaDeviceTransferButton(long supportAccountId)
        {
            var dlpTenantId = _deviceDataAccess.GetDlpTenantIdBySupportAccountId(supportAccountId);
            try
            {     
                var licenseEntitlementResponse = LicenseEntitlementApis.GetLicenseEntitlementData<LicenseEntitlementRequest, LicenseEntitlementResponse>(new LicenseEntitlementRequest
                {
                    TenantId = dlpTenantId.ToString(),
                    EntitlementType = new string[]
                    {
                        "ngfw"
                    }
                });
                if (licenseEntitlementResponse.IsValid() && licenseEntitlementResponse.Entitlements.Ngfw != null)
                {
                    return licenseEntitlementResponse.Entitlements.Ngfw.MaxExpiration != null
                        ? (DateTime.Now - licenseEntitlementResponse.Entitlements.Ngfw.MaxExpiration.Value).TotalDays > 90
                        : true;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return true;
        }

        /// <summary>
        /// Only show the button if there are pending devices
        /// for the current user or for any user of the current
        /// support account if the user is a CSR
        /// </summary>
        /// <param name="userAccountId"></param>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public bool ShowReviewPendingTransfersButton(UserAccount userAccount, long supportAccountId)
        {

            //CSR and the user listed in the devicetransfers table can see (if there are devices pending)

            //var perms = _securityLogic.GetSupportAccountPermissionsForPrincipal(supportAccountId, userAccount);



            //if (perms.Contains(Permission.ReviewAllDeviceTransfers)) {
            //    //Get all transfers for this support account
            //    //First get all support account members,
            //    //Then get all pending transfers for those members

            //    return GetAllPendingDeviceTransfersBySupportAccountId(supportAccountId, userAccount.Id).Any();

            //} else {
            //    //Get all transfers to this one user

            //    var pendingTransfers = _deviceTransferRepository.GetDevicesPendingTransferByUserAccountId(userAccount);

            //    return pendingTransfers.Any();
            //}

            var pendingTransfers = _deviceTransferRepository.GetDevicesPendingTransferByUserAccountId(userAccount.Id, supportAccountId);
            return pendingTransfers.Any();
        }

        private IEnumerable<PendingDeviceTransfer> GetAllPendingDeviceTransfersBySupportAccountId(long supportAccountId, long restrictedByUserAccountId)
        {

            var members = _membershipDataAccess.ReadSupportAccountMembers(supportAccountId, restrictedByUserAccountId, new DataParameters());
            var memberUserAccountIds = members.Select(x => x.UserAccountId);

            var allPendingTransfers = _deviceTransferRepository.GetAllDevicesPendingTransfer();

            return allPendingTransfers.Where(all => memberUserAccountIds.Contains(all.DestinationUserAccountId)).ToArray();
        }

        public BeginDeviceTransferModel GetBeginTransferData(long deviceId, long restrictedByUserAccountId, long supportAccountId = 0)
        {

            var deviceDataAccess = new DeviceDataAccess();

            var device = deviceDataAccess.Read(deviceId, restrictedByUserAccountId);
            long? id = _currentStateService.GetSupportAccountId();

            if (!id.HasValue && supportAccountId > 0)
            {
                id = supportAccountId;
            }

            var model = new BeginDeviceTransferModel
            {
                SerialNumber = device.SerialNumber,
                DeviceName = device.DeviceName,
                PartNumber = device.PartNumber,
                DeviceId = deviceId,
                HasLgsAssociated = _saasRegistrationRepository.ContainsSaasAssets(deviceId, id.Value)
            };

            return model;
        }

        public async Task<string> BeginTransfer(BeginDeviceTransferModel model, long userAccountId = 0, long supportAccountId = 0)
        {

            bool fromAccountHasWF = false;
            bool toAccountHasWF = false;
            var deviceDataAccess = new DeviceDataAccess();
            var userAccountDataAccess = new UserAccountDataAccess();

            var isPending = _deviceTransferRepository.IsDevicePendingTransfer(model.DeviceId);
            if (isPending)
                return "This device is already pending transfer";

            if (model.TransferBy.Equals(DeviceTransferBy.SupportAccount))
            {
                //return "Device Transfered to SupportAccount";

                if (model.SupportAccountId.Equals(0))
                    return "Please enter valid support account id.";

                var sourceSupportAccountId = _currentStateService.GetSupportAccountId();

                if (!sourceSupportAccountId.HasValue && supportAccountId > 0)
                    sourceSupportAccountId = supportAccountId;

                if (sourceSupportAccountId != null)
                {
                    var eaMessage = CheckForEA(Convert.ToInt64(sourceSupportAccountId), model.SupportAccountId, model.DeviceId);

                    if (!string.IsNullOrEmpty(eaMessage)) return eaMessage;

                    fromAccountHasWF = _deviceDataAccess.CheckForWFSubscription(Convert.ToInt64(sourceSupportAccountId));
                    if (fromAccountHasWF)
                        toAccountHasWF = _deviceDataAccess.CheckForWFSubscription(model.SupportAccountId);
                }

                var userId = _currentStateService.GetCurrentUser()?.UserAccount?.Id;
                if (!userId.HasValue && userAccountId > 0)
                {
                    userId = userAccountId;
                }
                var result = deviceDataAccess.TransferDeviceToSupportAccount(model.DeviceId, model.SupportAccountId, userId.Value, true);

                //Check if any memberships exist in the customer's account (active or not)
                var hasMembers = _ascDeviceRegistrationService.HasAnyMemberships(model.SupportAccountId);
                //If not, then add the current user as the super user
                if (!hasMembers)
                {
                    //var userAccountId = _currentStateService.GetCurrentUser().UserAccount.Id;
                    var membership = new Membership()
                    {
                        UserAccount = new UserAccount() { Id = userId.Value },
                        SupportAccountId = model.SupportAccountId,
                        ActivationDate = DateTime.Now,
                        Description = "New user created",
                        CreatedOn = DateTime.Now,
                        CreatedByUserAccountId = userId.Value
                    };

                    membership = _membershipLogic.CreateMembership(membership);
                    var membershipId = membership.Id;

                    //Will automatically make them Super
                    _membershipLogic.AssignMembershipRoles(membershipId, userId.Value, model.SupportAccountId,
                                                                                                userId.Value);
                }

                if (!result)
                {
                    return "Failed to transfer device to support account.  Please contact a CSR";
                }
                else if (fromAccountHasWF)
                {
                    ReconcileWildfireApiKeyExpirationDates(Convert.ToInt64(sourceSupportAccountId), model.SupportAccountId, toAccountHasWF, deviceId: model.DeviceId);
                }

                //initiate cdl notification call
                var productFamily = GetProductFamily(model.SerialNumber);
                if (productFamily != null) 
                {
                    if (productFamily.ToUpper().Contains("PANORAMA") || productFamily.ToUpper().StartsWith("M-")
                                            || productFamily.ToUpper().StartsWith("PA-M"))
                    {
                        productFamily = "PANORAMA";
                    } else if (productFamily.ToUpper().StartsWith("PA-") && !productFamily.ToUpper().Contains("VM"))
                    
                    {
                        productFamily = "NGFW";
                    }
                                            
                    if (productFamily.Equals("PANORAMA") || productFamily.Equals("NGFW"))
                    {
                        LicenseApis.GenericLicensingPost<CdlNotificationRequest, String, String>(new CdlNotificationRequest
                        {
                            SerialNumber = model.SerialNumber,
                            EventType = ApiConstants.DEVICE_TRANSFER_EVENT,
                            OldSupportAccountId = Convert.ToInt64(sourceSupportAccountId),
                            NewSupportAccountId = model.SupportAccountId,
                            ProductType = productFamily
                        },
                        ApiConstants.PUBLISH_TO_CDL_ENDPOINT
                        );
                    }
                }
            }
            else
            {
                var emailExists = userAccountDataAccess.IsEmailExists(model.DestinationEmail);
                if (!emailExists)
                    return "The destination email must be a registered user";

                //TODO require the dest email to have access to an account other than the src account - otherwise what's the point of a transfer?

                //Insert to Csp.DeviceTransfers table as pending
                var result = _deviceTransferRepository.CreatePendingTransfer(model);
                if (result <= 0) return "Failed to initiate device transfer.  Please contact a CSR";

                var sourceSupportAccountId = _currentStateService.GetSupportAccountId();

                if (!sourceSupportAccountId.HasValue)
                {
                    sourceSupportAccountId = supportAccountId;
                }

                #region Publish Event to Pubsub

                AppNotificationData deviceTransferAppNotificationData = new AppNotificationData()
                {
                    AccountId = Convert.ToString(sourceSupportAccountId),
                    EntityId = Convert.ToString(result),
                    UserId = Convert.ToString(model.SourceUserAccountId)
                };

                AppNotification transferNotification = new AppNotification()
                {
                    AppData = deviceTransferAppNotificationData,
                    Category = NotificationCategory.DeviceTransfer.ToDescription(),
                    DeliveryChannel = new List<string>() { NotificationDeliveryChannel.Web.ToDescription() },
                    Source = NotificationSource.Csp.ToDescription()
                };
                EventPublisher eventPublisher = new EventPublisher();
                await eventPublisher.PublishEvent(transferNotification);
                #endregion

                //Notify user if needed
                if (model.NotifyUser)
                {
                    NotifyUserOfBeginTransfer(model);
                }
            }


            return null;
        }

        private string GetProductFamily(string serialNumber)
        {
            string query = "SELECT p.productfamily_id FROM pandb.device_master dm INNER JOIN pandb.partmaster p ON dm.part_id = p.part_id WHERE dm.serialnum =?serialNumber";

            using (var connection = new MySqlConnection(Secrets.PanDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand
                {
                    CommandType = CommandType.Text,
                    Connection = connection
                })
                {
                    command.CommandText = query;
                    command.Parameters.AddWithValue("?serialNumber", serialNumber);

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        connection.Close();
                        return result.ToString();
                    }
                }
                connection.Close();
            }
            return null;
        }

        private void NotifyUserOfBeginTransfer(BeginDeviceTransferModel model)
        {

            var userAccountDataAccess = new UserAccountDataAccess();

            var sourceUser = userAccountDataAccess.Read(model.SourceUserAccountId, model.SourceUserAccountId);

            const string subject = "Palo Alto Networks - A device has been transferred to you";

            var sbBody = new StringBuilder();
            sbBody.AppendFormat(@"The device {0} is being transferred to you by {1}.
You can accept the device by logging into your Palo Alto Networks Support account at
http://support.paloaltonetworks.com.

For questions about this transfer, please contact {1}.

For other questions, please contact Palo Alto Networks support at
support@paloaltonetworks.com or call us at
US: 1.866.898.9087 
Outside the US: +1.408.738.7799.

This message comes from an automated system using an unmonitored mailbox.
Please do not respond to this message directly.", model.SerialNumber, sourceUser.Email);

            EmailHelper.SendMail(model.DestinationEmail, subject, sbBody.ToString(), false, "Device Transfer");
        }

        public IEnumerable<PendingDeviceTransfer> GetReviewTransferData(UserAccount userAccount, long supportAccountId, long deviceTransferId)
        {

            //var perms = _securityLogic.GetSupportAccountPermissionsForPrincipal(supportAccountId, userAccount);

            //if (perms.Contains(Permission.ReviewAllDeviceTransfers)) {

            //    return GetAllPendingDeviceTransfersBySupportAccountId(supportAccountId, userAccount.Id);
            //} else {

            //    return _deviceTransferRepository.GetDevicesPendingTransferByUserAccountId(userAccount);
            //}

            return _deviceTransferRepository.GetDevicesPendingTransferByUserAccountId(userAccount.Id, supportAccountId, deviceTransferId);
        }

        public ReviewTransferResult ReviewTransfer(long deviceTransferId, bool isApproved, UserAccount userAccount, long supportAccountId = 0)
        {

            var fromAccountHasWF = false;
            var toAccountHasWF = false;
            var canMakeTransferDecision = CanMakeTransferDecision(deviceTransferId, userAccount, supportAccountId);
            if (!canMakeTransferDecision) throw new SecurityException();

            if (!isApproved)
            {
                _deviceTransferRepository.RejectPendingTransfer(deviceTransferId, userAccount.Id);

                return new ReviewTransferResult
                {
                    IsSuccess = true,
                };
            }

            var transferData = _deviceTransferRepository.GetPreTransferData(deviceTransferId);
            var comments = "Transfer of Ownership requested by " + transferData.CreatedByDisplayName;
            var fromSupportAccountId = _deviceRepository.GetSupportAccountIdByDeviceId(transferData.DeviceId);

            var eaMessage = CheckForEA(fromSupportAccountId, supportAccountId, transferData.DeviceId);

            if (!string.IsNullOrEmpty(eaMessage))
            {
                return new ReviewTransferResult
                {
                    IsSuccess = false,
                    ErrorMessage = eaMessage
                };
            }

            fromAccountHasWF = _deviceDataAccess.CheckForWFSubscription(Convert.ToInt64(fromSupportAccountId));
            if (fromAccountHasWF)
            {
                toAccountHasWF = _deviceDataAccess.CheckForWFSubscription(supportAccountId);
            }

            // 1) Backup in csp.logdevice
            var logDeviceDataAccess = new LogDeviceDataAccess();
            logDeviceDataAccess.Log(transferData.DeviceId, "deviceTransferOwnerShip " + comments, userAccount.Id);

            // 2) Remove all access to the device (the current users' access is no invalid): groupdevices and membership tables
            _deviceTransferRepository.RemoveSourcePermissions(transferData.DeviceId);

            // 3) Change acctid in devicemaster
            var deviceDataAccess = new DeviceDataAccess();
            deviceDataAccess.TransferDeviceInPanDb(transferData.DeviceId, userAccount.Id, comments);
            //_supportAccountRepository.UpdateDeviceSupportAccountInCsp(transferData.DeviceId, supportAccountId);

            // 4) Update the License.SupportAccountId for all licenses of the device
            // TODO - eventually remove the License.SupportAccountId field as it should never be used (but currently is in some places).  Legacy queries need to be updated to reference Device.SupportAccountId instead
            //_licenseRepository.UpdateLicenseSupportAccountId(transferData.DeviceId, supportAccountId);
            deviceDataAccess.TransferDeviceToSupportAccount(transferData.DeviceId, supportAccountId, userAccount.Id, true);

            // 5) Log the approved transfer
            _deviceTransferRepository.ApprovePendingTransfer(deviceTransferId, userAccount.Id);

            // 6) Notify WF API to delete Email Notificatications
            WildfireDeleteNotify(transferData.DeviceId, fromSupportAccountId);
            // 7) Remove the device from WildfireDevice
            _wildfireRepository.RemoveWildfireDevice(transferData.DeviceId);

            // 8) Transfer ASC info (if necessary)
            TransferAscInfo(transferData.DeviceId);

            if (fromAccountHasWF)
            {
                ReconcileWildfireApiKeyExpirationDates(fromSupportAccountId, supportAccountId, toAccountHasWF, deviceId: transferData.DeviceId);
            }

            //initiate cdl notification call
            var serialNumber = _wildfireRepository.GetSerialNumberByDeviceId(transferData.DeviceId);
            if (serialNumber != null)
            {
                var productFamily = GetProductFamily(serialNumber);
                if (productFamily != null) 
                {
                    if (productFamily.ToUpper().Contains("PANORAMA") || productFamily.ToUpper().StartsWith("M-")
                                            || productFamily.ToUpper().StartsWith("PA-M"))
                    {
                        productFamily = "PANORAMA";
                    } else if (productFamily.ToUpper().StartsWith("PA-") && !productFamily.ToUpper().Contains("VM"))
                    
                    {
                        productFamily = "NGFW";
                    }
                                            
                    if (productFamily.Equals("PANORAMA") || productFamily.Equals("NGFW"))
                    {
                        LicenseApis.GenericLicensingPost<CdlNotificationRequest, String, String>(new CdlNotificationRequest
                        {
                            DeviceId = transferData.DeviceId,
                            EventType = ApiConstants.DEVICE_TRANSFER_EVENT,
                            OldSupportAccountId = fromSupportAccountId,
                            NewSupportAccountId = supportAccountId,
                            ProductType = productFamily,
                            SerialNumber = serialNumber
                        },
                        ApiConstants.PUBLISH_TO_CDL_ENDPOINT
                        );
                    }
                }
            }


            _licenseLogic.GrantSiteLicenses(supportAccountId);
            return new ReviewTransferResult
            {
                IsSuccess = true,
            };
        }

        private void WildfireDeleteNotify(long deviceID, long supportAccountId)
        {
            try
            {
                var wildFireUserAccounts = _wildfireRepository.GetExistingWFUsersForDevice(deviceID);
                if (wildFireUserAccounts == null)
                    return;
                // Call Delete Notification for each Wildfire Account Id
                foreach (var wildFireUserAccount in wildFireUserAccounts)
                {
                    var t = new Thread(() => _wildfireWipeoutService.DeleteEmailNotification(wildFireUserAccount, supportAccountId, deviceID));
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

        }

        private void TransferAscInfo(long deviceId)
        {

            var licenseTransferDataAccess = new LicenseTransferDataAccess();

            //Check if any of the licenses in PANDB have an pandb.lic_features.ascid != null but also a csp.license.supportcentersupportaccountid == null (this means there is a problem)
            var ascLicense = licenseTransferDataAccess.LookForAscs(deviceId);
            if (ascLicense == null)
            {
                //there is no asc on the lic so nothing to do
                return;
            }

            //Verify that the CSP license shows no SupportCenterSupportAccountId
            var ascAlreadySetup = _licenseRepository.DoesCspLicenseHaveAsc(ascLicense.AuthCode);
            if (ascAlreadySetup)
            {
                return;
            }

            //Then look up the ASC based on their ASCID (which is a SalesfrceAccountId).
            var ascSupportAccountIds = _licenseRepository.GetSupportAccountIdBySalesforceAccountId(ascLicense.AscSalesforceAccountId);


            if (ascSupportAccountIds.Length == 1)
            {
                //If the sfID matches 1 and only 1 ASC, then update csp.license.SupportCenterSupportAccountId

                _licenseRepository.UpdateLicenseAsc(ascLicense.AuthCode, ascSupportAccountIds.Single());
            }
            else
            {
                //If it matches 0 or more than one, log to the ASC review table and continue (possibly also elmah)

                var reason =
                    ascSupportAccountIds.Length > 1
                        ? "More than one matching ASC SupportAccount found for this Salesforce Account Id."
                        : "No matching ASC SupportAccount found for this Salesforce Account Id.";

                _licenseRepository.LogToAscInformationReview(ascLicense.Lfid, ascLicense.AscSalesforceAccountId, string.Empty, ascLicense.AuthCode, reason);
            }
        }

        private bool CanMakeTransferDecision(long deviceTransferId, UserAccount userAccount, long supportAccountId)
        {

            var perms = _securityLogic.GetSupportAccountPermissionsForPrincipal(supportAccountId, userAccount);

            if (perms.Contains(Permission.ReviewAllDeviceTransfers))
            {

                return true;
            }
            else
            {

                var allowedTransfers = _deviceTransferRepository.GetDevicesPendingTransferByUserAccountId(userAccount.Id, supportAccountId);

                return allowedTransfers.Any(x => x.DeviceTransferId == deviceTransferId);
            }
        }

        private void ReconcileWildfireApiKeyExpirationDates(long fromAccountId, long toAccountId, bool toAccountHasWF, long deviceId = long.MinValue, long deviceTransferId = long.MinValue)
        {
            try
            {
                // do this in a seperate thread else it takes for ever.
                var t = new Thread(() => WidlfireApiCalls(fromAccountId, toAccountId, toAccountHasWF, deviceId, deviceTransferId));
                t.Start();
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error while reconcilling wf api key expiration dates for accounts - {0},{1} during review device transfer. Error is: {2}", fromAccountId, toAccountId, oex.ToString())));
            }
        }

        public void WidlfireApiCalls(long fromAccountId, long toAccountId, bool toAccountHasWF, long deviceId = long.MinValue, long deviceTransferId = long.MinValue)
        {

            try
            {
                #region call api

                // check the source account after transfer
                var fromAccountHasWF = _deviceDataAccess.CheckForWFSubscription(fromAccountId);

                if (fromAccountHasWF)
                {
                    var sourceResponse = _wildfireStore.WildfireReconcile(fromAccountId);
                }
                else
                {
                    var wildfireResponse = _wildfireStore.GetWildfireKeys(fromAccountId.ToString(), PageNumber, PageSize);

                    if (wildfireResponse.data != null && wildfireResponse.data.wildfireAPIkeys != null && wildfireResponse.data.wildfireAPIkeys != null
                        && wildfireResponse.data.wildfireAPIkeys.Count() > 0 && wildfireResponse.data.wildfireAPIkeys[0].status != "REVOKED")
                    {
                        _wildfireStore.RevokeWildfireKey(wildfireResponse.data.wildfireAPIkeys[0].apiKey);
                    }
                }
                if (toAccountHasWF)
                {
                    var targetResponse = _wildfireStore.WildfireReconcile(toAccountId);
                }
                else
                {
                    var deviceRequests = new List<long>();
                    if (deviceId > 0)
                        deviceRequests.Add(deviceId);
                    else
                        deviceRequests = _deviceDataAccess.GetDeviceIdsByBulkDeviceTransferId(deviceTransferId);

                    foreach (var devId in deviceRequests)
                    {
                        var targetResponse = CreateWildfireRequest(toAccountId, devId);
                    }
                }

                #endregion
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error calling wf service when reconcile wf api key for accounts - {0},{1} during review device transfer. Error is: {2}", fromAccountId, toAccountId, oex.ToString())));
            }
        }

        private WildfireApiResponse CreateWildfireRequest(long toAccountId, long deviceId)
        {
            var subType = _wildfireRepository.GetProductTypeByDeviceId(deviceId);
            var request = new WildfireApiRequest()
            {
                supportAccountId = toAccountId,
                serialNumber = subType == "WILDFIRE" ? "" : _wildfireRepository.GetSerialNumberByDeviceId(deviceId),
                sourceType = ConfigurationManager.AppSettings.Get("WFAPISourceType"),
                productType = subType,
                startDate = DateTime.Now,
                endDate = DateTime.Now.AddYears(1)
            };

            return _wildfireStore.GenerateWildFireApiKey(request);
        }

        private string CheckForEA(long srcSupportAccountId, long destSupportAccountId, long? deviceId)
        {
            if (srcSupportAccountId == destSupportAccountId) return string.Empty;

            var srcSupportAccountName = _supportAccountRepository.GetSupportAccountNameBySupportAccountId(srcSupportAccountId);
            var destSupportAccountName = _supportAccountRepository.GetSupportAccountNameBySupportAccountId(destSupportAccountId);

            var source = _deviceTransferRepository.ValidateSupportAccountForDeviceTransfer(srcSupportAccountId, deviceId);

            if (!string.IsNullOrEmpty(source))
            {
                var target = _deviceTransferRepository.ValidateSupportAccountForDeviceTransfer(destSupportAccountId, identifier: source);
                if (string.IsNullOrEmpty(target))
                {
                    source = source.Contains("SITEENT") ? "ELA" : "ESA";
                    return " Cannot transfer this device with " + source + "  subscription from the current Support Account "
                                        + srcSupportAccountName + " - " + srcSupportAccountId + " to the non - " + source + " Support Account "
                                        + destSupportAccountName + " - " + destSupportAccountId + ". Please choose the Support Account that's part of the " + source + ".";
                }
            }
            return string.Empty;
        }

        public bool HasAscTransferBetaAccess(long? supportAccountId)
        {
            return _deviceRepository.HasAscTransferBetaAccess(supportAccountId);
        }

        public void ProcessBulkDeviceTransfer()
        {
            var fromAccountHasWF = false;
            var toAccountHasWF = false;
            try
            {
                var bulkRequests = _deviceDataAccess.GetBulkTransferRequests();

                if (bulkRequests != null || bulkRequests.Count > 0)
                {
                    foreach (BulkDeviceTransferRequest request in bulkRequests)
                    {
                        var isBackupSuccess = _deviceDataAccess.BackupBulkTransferDevices(request.BulkDevicetransferId);

                        //if backup is successful then process the transfer requests
                        if (isBackupSuccess)
                        {
                            fromAccountHasWF = _deviceDataAccess.CheckForWFSubscription(request.SourceAccountId);
                            if (fromAccountHasWF)
                                toAccountHasWF = _deviceDataAccess.CheckForWFSubscription(request.DestinationAccountId);
                            _deviceDataAccess.ProcessBulkDeviceTransfer(request);
                            if (fromAccountHasWF)
                            {
                                ReconcileWildfireApiKeyExpirationDates(request.SourceAccountId, request.DestinationAccountId, toAccountHasWF, deviceTransferId: request.BulkDevicetransferId);
                            }
                        }
                        }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

    }

    public class BeginDeviceTransferModel
    {

        public string SerialNumber { get; set; }
        public string DeviceName { get; set; }
        public string PartNumber { get; set; }


        public long DeviceId { get; set; }
        public long SourceUserAccountId { get; set; }

        public string DestinationEmail { get; set; }
        public bool NotifyUser { get; set; }
        public DeviceTransferBy TransferBy { get; set; }
        public long SupportAccountId { get; set; }
        public bool HasAscTransferAccess { get; set; }
        public bool HasLgsAssociated { get; set; }
    }

    public enum DeviceTransferBy
    {
        EmailRecipient,
        SupportAccount
    }

    public class ReviewDeviceTransferModel
    {
        public long DeviceTransferId { get; set; }
        public long DeviceId { get; set; }
        public string SerialNumber { get; set; }
        public string DeviceName { get; set; }
        public string PartNumber { get; set; }
        public DateTime DeviceTransferBeginDate { get; set; }
        public long FromUserAccountId { get; set; }
        public string FromUserDisplayName { get; set; }
        public DeviceTransferStates DeviceTransferState { get; set; }
    }

    public class ReviewTransferResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum DeviceTransferStates
    {
        Pending,
        Accepted,
        Rejected,
        Cancelled,
    }
}
