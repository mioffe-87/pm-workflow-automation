using System;
using System.Web.Http;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Api.Model;

namespace Csp.Web.Controllers
{
    [RequireCspApiCertAttribute]
    public class ScheduledJobsController : ApiController
    {
        private readonly IScheduledJobsLogic _scheduledJobsLogic;

        public ScheduledJobsController(IScheduledJobsLogic scheduledJobsLogic)
        {
            _scheduledJobsLogic = scheduledJobsLogic;
        }

        /// <summary>
        /// Bulk Registrations
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult ProcessBulkRegistrations()
        {
            CommonApiResponse<ScheduledJobs> result = null;
            try
            {
                result = _scheduledJobsLogic.ProcessBulkRegistrations();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
            }
            return this.Json(GetDefaultResponse());
        }

        [HttpGet]
        public IHttpActionResult SyncMembershipsWithSfdc()
        {
            try
            {
                var result = _scheduledJobsLogic.SyncMembershipsWithSfdc();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
            }
            return this.Json(GetDefaultResponse());
        }

      
        [HttpGet]
        public IHttpActionResult ProcessPinExpirationRequest()
        {
            try
            {
                var result = _scheduledJobsLogic.ProcessPinExpirationRequest();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
            }
            return this.Json(GetDefaultResponse());
        }
        [HttpGet]
        public IHttpActionResult ProcessOtpExpirationRequest()
        {
            try
            {
                var result = _scheduledJobsLogic.ProcessOtpExpirationRequest();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
            }
            return this.Json(GetDefaultResponse());
        }
        [HttpGet]
        public IHttpActionResult ProcessVmElaTokenNotification()
        {
            try
            {
                var result = _scheduledJobsLogic.ProcessVmElaTokenNotification();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
            }
            return this.Json(GetDefaultResponse());
        }
      

        [HttpGet]
        public IHttpActionResult ProcessBulkOtpRequest()
        {
            try
            {
                var result = _scheduledJobsLogic.ProcessBulkOtpRequest();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
            }
            return this.Json(GetDefaultResponse());
        }

        [HttpGet]
        public IHttpActionResult ProcessBulkDeviceTransferRequest()
        {
            try
            {
                var result = _scheduledJobsLogic.ProcessBulkDeviceTransfer();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
            }
            return this.Json(GetDefaultResponse());
        }

        [HttpGet]
        public IHttpActionResult ProcessEmailNotificationStatusUpdate()
        {
            try
            {
                var result = _scheduledJobsLogic.ProcessEmailNotificationStatusUpdate();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
            }
            return this.Json(GetDefaultResponse());
        }

        [HttpGet]
        public IHttpActionResult ProcessEvalAccountCreationRequest()
        {
            try
            {
                var result = _scheduledJobsLogic.ProcessEvalAccountCreationRequest();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
            }
            return this.Json(GetDefaultResponse());
        }

        private CommonApiResponse<ScheduledJobs> GetDefaultResponse()
        {
            return new CommonApiResponse<ScheduledJobs>()
            {
                ResponseObject = new ScheduledJobs()
                {
                    IsSuccess = false,
                    ErrorMessage = string.Empty
                }
            };
        }
    }
}
