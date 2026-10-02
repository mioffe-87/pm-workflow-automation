using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Newtonsoft.Json;

namespace Csp.Web.Controllers
{
    [System.Web.Mvc.Authorize]
    public class ConsentRequestController : ApiController
    {
        private readonly IConsentRequestLogic _consentRequestLogic;
        private readonly ISecurityLogic _securityLogic;
        private readonly IUserAccountLogic _userAccountLogic;

        public ConsentRequestController(IConsentRequestLogic consentRequestLogic,ISecurityLogic securityLogic,IUserAccountLogic userAccountLogic)
        {
            _consentRequestLogic = consentRequestLogic;
            _securityLogic = securityLogic;
            _userAccountLogic = userAccountLogic;
        }

        [System.Web.Http.HttpPost]
        [JwtTokenAuthentication]
        public async Task<IHttpActionResult> ConsentRequestProxy([FromBody] ConsentRequest consentRequest)
        {
               

            if (!JwtTokenManager.IsAuthorized(consentRequest.UpdatedUserAccountId, consentRequest.UpdatedSupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            
          
            ErrorResponse errorResponse = new ErrorResponse();
            try
            {
                if (consentRequest.Action == null || consentRequest.RequestorSupportAccountId <= 0 || consentRequest.RequestorUserAccountId <= 0 || consentRequest.UpdatedUserAccountId <= 0 || consentRequest.UpdatedSupportAccountId <= 0)
                {
                    errorResponse.ErrorMessage = "One of the input value is blank or invalid";
                    return this.Json(errorResponse);
                }
                
                consentRequest.Action = consentRequest.Action.ToUpper();
                //Create Request Validation
                if (consentRequest.Action == ConsentAction.Create.ToString().ToUpper())
                {
                    if (consentRequest.DestinationSupportAccountId <= 0 || string.IsNullOrEmpty(consentRequest.DestinationUserEmail))
                    {
                        errorResponse.ErrorMessage = "One of the input value is blank or invalid";
                        return this.Json(errorResponse);
                    }

                    bool isValidSupportAccount = _securityLogic.IsValidSupportAccount(consentRequest.DestinationSupportAccountId);
                    if (!isValidSupportAccount)
                    {
                        errorResponse.ErrorMessage = "Please enter valid active AccountID";
                        return this.Json(errorResponse);
                    }

                    // Validate that sourceUserId (RequestorUserAccountId) matches the authenticated user from the JWT token
                    // This prevents privilege escalation by impersonating another user
                    if (consentRequest.RequestorUserAccountId > 0 &&
                        !JwtTokenManager.IsAuthorized(consentRequest.RequestorUserAccountId, consentRequest.RequestorSupportAccountId, ClaimType.UserAccountId))
                    {
                        return Unauthorized();
                    }

                    //check if Source User is DA
                    bool isSourceUserDA = _securityLogic.HasRole(consentRequest.RequestorSupportAccountId, consentRequest.RequestorUserAccountId, RoleNames.DomainAdministrator);

                    if (!isSourceUserDA)
                    {
                        errorResponse.ErrorMessage = "Source User should be a Domain Administrator";
                        return this.Json(errorResponse);
                    }

                    if (!string.IsNullOrEmpty(consentRequest.DestinationUserEmail))
                    {
                        UserAccount destinationUser = _userAccountLogic.GetUserAccountByEmail(consentRequest.DestinationUserEmail);
                        consentRequest.DestinationUserAccountId = destinationUser == null ? 0 : destinationUser.Id;
                        if (destinationUser == null)
                        {
                            errorResponse.ErrorMessage = "Please enter a valid target user email";
                            return this.Json(errorResponse);
                        }
                    }
                
                //Check if Target User is DA
                bool isTargetUserDA = _securityLogic.HasRole(consentRequest.DestinationSupportAccountId, consentRequest.DestinationUserAccountId, RoleNames.DomainAdministrator);

                if (!isTargetUserDA)
                {
                    errorResponse.ErrorMessage = "Target User should be a Domain Administrator";
                    return this.Json(errorResponse);
                }

            }

                    //Update Request Validation
                    if (consentRequest.Action == ConsentAction.Update.ToString().ToUpper() || consentRequest.Action == ConsentAction.UpdateRoles.ToString().ToUpper())
                    {
                        //check if Updating User is DA
                        bool isUpdatingUserDA = _securityLogic.HasRole(consentRequest.UpdatedSupportAccountId, consentRequest.UpdatedUserAccountId, RoleNames.DomainAdministrator);

                        if (!isUpdatingUserDA)
                        {
                            errorResponse.ErrorMessage = "You are not authorized to access this content.";
                            return this.Json(errorResponse);
                        }

                        if (consentRequest.ConsentStatus <= 0 || string.IsNullOrEmpty(consentRequest.ConsentRequestId))
                        {
                            errorResponse.ErrorMessage = "One of the input value is blank or invalid";
                            return this.Json(errorResponse);
                        }
                    }

                    dynamic configuration = null;
                    configuration = await _consentRequestLogic.ConsentRequestProxy(consentRequest);

                    return this.Json(configuration);

                
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(errorResponse);
        }
    }
}
