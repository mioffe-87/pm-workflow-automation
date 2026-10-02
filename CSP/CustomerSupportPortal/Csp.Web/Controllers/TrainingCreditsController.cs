using System;
using System.Web.Http;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;

namespace Csp.Web.Controllers
{
    public class TrainingCreditsController : ApiController
    {
        private readonly ITrainingCreditsApiLogic _trainingCreditsApiLogic;

        public TrainingCreditsController(ITrainingCreditsApiLogic trainingCreditsApiLogic)
        {
            _trainingCreditsApiLogic = trainingCreditsApiLogic;
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetTrainingCredits(long userAccountId, long supportAccountId)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _trainingCreditsApiLogic.GetTrainingCredits(userAccountId, supportAccountId);
                return Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetTrainingCourses(long userAccountId, long supportAccountId, string creditId)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _trainingCreditsApiLogic.GetTrainingCourses(userAccountId, supportAccountId, creditId);
                return Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetSelectedTrainingPartners(long userAccountId, long supportAccountId)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _trainingCreditsApiLogic.GetSelectedTrainingPartners(userAccountId, supportAccountId);
                return Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetTrainingPartners(long userAccountId, long supportAccountId)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _trainingCreditsApiLogic.GetTrainingPartners(userAccountId, supportAccountId);
                return Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateSelectedTrainingPartners([FromBody] TrainingPartner trainingPartner)
        {

            if (!JwtTokenManager.IsAuthorized(trainingPartner.UserAccountId, trainingPartner.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _trainingCreditsApiLogic.UpdateSelectedTrainingPartners(trainingPartner);
                return Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

    }
}
