using System;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;
using System.Configuration;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;
using System.Web;

namespace Csp.Api.Logic
{
    public class ScheduledJobsLogic : IScheduledJobsLogic
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly IBulkRegistrationService _bulkRegistrationService;
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly IMembershipLogic _membershipLogic;
        private readonly ISalesforceService _salesforceService;
        private readonly IVmRegistrationService _vmRegistrationService;
        private readonly ISiteLicenseLogic _siteLicenseLogic;
        private readonly IOtpLogic _otpLogic;
        private readonly IDeviceTransferLogic _deviceTransferLogic;
        private readonly IEmailNotificationLogic _emailNotificationLogic;
        private readonly IEvalAccountCreationService _evalAccountCreationService;
        public readonly string _systemUserAccountId = ConfigurationManager.AppSettings["SystemUserAccountId"];

        private bool _isSuccess = false;
        private string _message = string.Empty;
        private CommonApiResponse<ScheduledJobs> _response = null;

        public ScheduledJobsLogic(IApplicationSettings applicationSetting, IBulkRegistrationService bulkRegistrationService,
            IUserAccountLogic userAccountLogic, IMembershipLogic membershipLogic, 
            ISalesforceService salesforceService, IVmRegistrationService vmRegistrationService,
            ISiteLicenseLogic siteLicenseLogic, IOtpLogic OtpLogic, IDeviceTransferLogic deviceTransferLogic, IEmailNotificationLogic emailNotificationLogic, IEvalAccountCreationService evalAccountCreationService)
        {
            _applicationSettings = applicationSetting;
            _bulkRegistrationService = bulkRegistrationService;
            _userAccountLogic = userAccountLogic;
            _membershipLogic = membershipLogic;
            _salesforceService = salesforceService;
            _vmRegistrationService = vmRegistrationService;
            _siteLicenseLogic = siteLicenseLogic;
            _otpLogic = OtpLogic;
            _deviceTransferLogic = deviceTransferLogic;
            _emailNotificationLogic = emailNotificationLogic;
            _evalAccountCreationService = evalAccountCreationService;
        }

        public CommonApiResponse<ScheduledJobs> ProcessBulkRegistrations()
        {
            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.BulkRegistration))
                {
                    CronJobHelper.QueueJob(CronJobEnum.BulkRegistration);
                    _bulkRegistrationService.Process();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.BulkRegistration, CronJobStatus.Success);
                }
                _isSuccess = true;
            }
            catch (Exception ex)
            {
                GetErrorMessage(CronJobEnum.BulkRegistration, ApiConstants.ProcessBulkRegistrationsErrorMessage, ex);
            }
            finally
            {
                _response = GetCommonResponse(_isSuccess, _message);
            }
            return _response;
        }

        public CommonApiResponse<ScheduledJobs> SyncMembershipsWithSfdc()
        {
            
            try
            {
                long systemUserAccountId;
                long.TryParse(_systemUserAccountId, out systemUserAccountId);
                var memberships = _membershipLogic.GetAllMembershipsWithoutSfdcContact();

                if (!CronJobHelper.IsJobPending(CronJobEnum.SfdcAddUpdateContact))
                {
                    CronJobHelper.QueueJob(CronJobEnum.SfdcAddUpdateContact);

                    foreach (var membership in memberships)
                    {
                        var userAccount = new UserAccount();

                        try
                        {
                            userAccount = _userAccountLogic.GetUserAccount(membership.UserAccountId, systemUserAccountId);
                        }
                        catch
                        {
                            continue;
                        }
                        _salesforceService.AddUpdateSalesforceContact(userAccount, membership.SupportAccountId,
                            membership.SalesforceAccountId, membership.MembershipId, systemUserAccountId);
                    }
                    CronJobHelper.UpdateJobStatus(CronJobEnum.SfdcAddUpdateContact, CronJobStatus.Success);
                }
                _isSuccess = true;
            }
            catch (Exception ex)
            {
                GetErrorMessage(CronJobEnum.SfdcAddUpdateContact, ApiConstants.SyncMembershipsWithSfdcErrorMessage, ex);
            }
            finally
            {
                _response = GetCommonResponse(_isSuccess, _message);
            }
            return _response;
        }

        public CommonApiResponse<ScheduledJobs> ProcessPinExpirationRequest()
        {
            try
            {
                long systemUserAccountId;
                long.TryParse(_systemUserAccountId, out systemUserAccountId);
                if (!CronJobHelper.IsJobPending(CronJobEnum.PinExpirationRequest))
                {
                    CronJobHelper.QueueJob(CronJobEnum.PinExpirationRequest);                    
                    _otpLogic.ProcessPinExpirationRequest(systemUserAccountId);
                    CronJobHelper.UpdateJobStatus(CronJobEnum.PinExpirationRequest, CronJobStatus.Success);
                }
                _isSuccess = true;
            }
            catch (Exception ex)
            {
                GetErrorMessage(CronJobEnum.PinExpirationRequest, ApiConstants.ProcessPinExpirationRequestErrorMessage, ex);
            }
            finally
            {
                _response = GetCommonResponse(_isSuccess, _message);
            }
            return _response;
        }

        public CommonApiResponse<ScheduledJobs> ProcessOtpExpirationRequest()
        {
            try
            {
                long systemUserAccountId;
                long.TryParse(_systemUserAccountId, out systemUserAccountId);
                if (!CronJobHelper.IsJobPending(CronJobEnum.OtpExpirationRequest))
                {
                    CronJobHelper.QueueJob(CronJobEnum.OtpExpirationRequest);                  
                    _otpLogic.ProcessOtpExpirationRequest(systemUserAccountId);
                    CronJobHelper.UpdateJobStatus(CronJobEnum.OtpExpirationRequest, CronJobStatus.Success);
                }
                _isSuccess = true;
            }
            catch (Exception ex)
            {
                GetErrorMessage(CronJobEnum.OtpExpirationRequest, ApiConstants.ProcessOtpExpirationRequestErrorMessage, ex);
            }
            finally
            {
                _response = GetCommonResponse(_isSuccess, _message);
            }
            return _response;

        }

        public CommonApiResponse<ScheduledJobs> ProcessVmElaTokenNotification()
        {
            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.VmElaTokenNotification))
                {
                    CronJobHelper.QueueJob(CronJobEnum.VmElaTokenNotification);                    
                    _siteLicenseLogic.SendVmElaTokenUsageNotification();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.VmElaTokenNotification, CronJobStatus.Success);
                }
                _isSuccess = true;
            }
            catch (Exception ex)
            {
                GetErrorMessage(CronJobEnum.VmElaTokenNotification, ApiConstants.ProcessVmElaTokenNotificationErrorMessage, ex);
            }
            finally
            {
                _response = GetCommonResponse(_isSuccess, _message);
            }
            return _response;
        }

      
        public CommonApiResponse<ScheduledJobs> ProcessBulkOtpRequest()
        {
            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.BulkOtpRequest))
                {
                    CronJobHelper.QueueJob(CronJobEnum.BulkOtpRequest);                   
                    _otpLogic.ProcessPendingBulkOtpRequests();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.BulkOtpRequest, CronJobStatus.Success);
                }
                _isSuccess = true;
            }
            catch (Exception ex)
            {
                GetErrorMessage(CronJobEnum.BulkOtpRequest, ApiConstants.ProcessBulkOtpRequestErrorMessage, ex);
            }
            finally
            {
                _response = GetCommonResponse(_isSuccess, _message);
            }
            return _response;
        }

        public CommonApiResponse<ScheduledJobs> ProcessBulkDeviceTransfer()
        {
            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.BulkDeviceTransfer))
                {
                    CronJobHelper.QueueJob(CronJobEnum.BulkDeviceTransfer);                    
                    _deviceTransferLogic.ProcessBulkDeviceTransfer();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.BulkDeviceTransfer, CronJobStatus.Success);
                }
                _isSuccess = true;
            }
            catch (Exception ex)
            {
                GetErrorMessage(CronJobEnum.BulkDeviceTransfer, ApiConstants.ProcessBulkDeviceTransferErrorMessage, ex);
            }
            finally
            {
                _response = GetCommonResponse(_isSuccess, _message);
            }
            return _response;
        }

        public CommonApiResponse<ScheduledJobs> ProcessEmailNotificationStatusUpdate()
        {
            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.EmailNotifications))
                {
                    CronJobHelper.QueueJob(CronJobEnum.EmailNotifications);                  
                    _emailNotificationLogic.UpdatePendingEmailNotificationStatus();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.EmailNotifications, CronJobStatus.Success);
                }
                _isSuccess = true;
            }
            catch (Exception ex)
            {
                GetErrorMessage(CronJobEnum.EmailNotifications, ApiConstants.ProcessEmailNotificationStatusUpdate, ex);
            }
            finally
            {
                _response = GetCommonResponse(_isSuccess, _message);
            }
            return _response;

        }

        public CommonApiResponse<ScheduledJobs> ProcessEvalAccountCreationRequest()
        {
            try
            {
                if (!CronJobHelper.IsJobPending(CronJobEnum.EvalAccountCreation))
                {
                    CronJobHelper.QueueJob(CronJobEnum.EvalAccountCreation);                   
                    _evalAccountCreationService.CreateEvalAccountsForVmEvals();
                    CronJobHelper.UpdateJobStatus(CronJobEnum.EvalAccountCreation, CronJobStatus.Success);
                }
              
            _isSuccess = true;
            }
            catch (Exception ex)
            {
                GetErrorMessage(CronJobEnum.EmailNotifications, ApiConstants.ProcessEvalAccountCreationErrorMessage, ex);
            }
            finally
            {
                _response = GetCommonResponse(_isSuccess, _message);
            }
            return _response;


        }

        private CommonApiResponse<ScheduledJobs> GetCommonResponse(bool isSuccess, string message)
        {
            return new CommonApiResponse<ScheduledJobs>()
            {
                ResponseObject = new ScheduledJobs()
                {
                    IsSuccess = isSuccess,
                    ErrorMessage = message
                }
            };
        }

        private void GetErrorMessage(CronJobEnum cronJobEnum, string errorMessage, Exception ex)
        {
            _message = string.Format("{0} {1}", errorMessage, ex.Message);
            CronJobHelper.UpdateJobStatus(cronJobEnum, CronJobStatus.Failed);
            if (HttpContext.Current != null)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMessage));
            }
            else
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error() { Message = errorMessage });
            }
        }
    }
}
