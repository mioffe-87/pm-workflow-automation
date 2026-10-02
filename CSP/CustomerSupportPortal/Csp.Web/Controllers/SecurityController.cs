using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Web.HelperClasses;
using Csp.Api.HelperClasses;
using System.Web;
using System.Linq;
using Saml;
using System.IO;

namespace Csp.Web.Controllers
{
    public class SecurityController : Controller
    {

        private readonly ISecurityLogic _securityLogic;
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly ISupportAccountUserLogic _supportAccountUserLogic;
        private readonly string _samlCertName = ConfigurationManager.AppSettings["SamlCertName"];
        private readonly string _loginUrl = ConfigurationManager.AppSettings["LoginUrl"];

        public SecurityController(ISecurityLogic iSecurityLogic, IUserAccountLogic iUserAccountLogic, ISupportAccountUserLogic iSupportAccountUserLogic)
        {
            _userAccountLogic = iUserAccountLogic;
            _securityLogic = iSecurityLogic;
            _supportAccountUserLogic = iSupportAccountUserLogic;
        }

        public ActionResult Login()
        {
            var returnUrl = Request.QueryString["returnurl"];
            string samlresponse = Convert.ToString(Request.Form["SAMLResponse"]);
            
            // Validate and sanitize returnUrl to prevent open redirect attacks
            if (!string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = GetSafeReturnUrl(returnUrl);
            }
            
            //In this case no support token and no imperosnate token so user is not logged in any SSO app
            if (!Request.Url.ToString().ToLower().Contains("accountselector")
                && !HttpUtility.UrlDecode(Request.Url.ToString().ToLower()).Contains("activate/flex")
                && !HttpUtility.UrlDecode(Request.Url.ToString().ToLower()).Contains("activate/wfapi")
                && string.IsNullOrEmpty(samlresponse) && string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToActionPermanent("Index", "Support");
            }
            var samlResponse = new Response(_samlCertName, Request.Form["SAMLResponse"]);
            var host = Request.Url.Host;

            var configValues = new ConfigValues();
            var enableSsl = Convert.ToBoolean(configValues.GetAppSetting(AppSettings.EnableSsl));
            var scheme = enableSsl ? "https" : "http";

            if (samlResponse != null && !string.IsNullOrEmpty(samlresponse))
            {
                if (samlResponse.IsValid())
                {
                    string errorMessage = string.Empty;
                    var userEmail = samlResponse.GetNameID();                                        
                    // Security validation: Verify user exists and is active in CSP
                    // This prevents accepting SAML assertions for unknown or unauthorized users
                    var userProfile = _supportAccountUserLogic.GetUserData(userEmail);
                    
                    if (userEmail.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION))
                    {
                        // PANW employee: Allow if new (will be auto-provisioned) OR if active and not blocked
                        if (userProfile != null && (userProfile.IsBlocked))
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(
                                new Exception($"SECURITY: SAML login rejected - PANW user inactive/blocked: {userEmail}"));
                            return RedirectToActionPermanent("Index", "Support");
                        }
                        // If userProfile is null, they'll be auto-provisioned below - that's OK for PANW employees
                    }
                    else
                    {
                        // Non-PANW user: Must exist AND not blocked
                        if (userProfile == null)
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(
                                new Exception($"SECURITY: SAML login rejected - user not found: {userEmail}"));
                            return RedirectToActionPermanent("Index", "Support");
                        }
                        
                        if (userProfile.IsBlocked)
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(
                                new Exception($"SECURITY: SAML login rejected - user blocked: {userEmail}"));
                            return RedirectToActionPermanent("Index", "Support");
                        }
                    }
                    
                    ClearAll(userEmail);
                   
                    //auto provision pan employees
                    AutoProvisionEmployee(userEmail, samlResponse.GetFirstName(), samlResponse.GetLastName(), samlResponse.GetTitle(),samlResponse.GetDepartment());                   

                    //redirect based on relay state
                    string relayStateUrl = Request.Form["RelayState"];
                    
                    // Validate relayStateUrl to prevent open redirect attacks
                    if (!string.IsNullOrEmpty(relayStateUrl))
                    {
                        relayStateUrl = GetSafeReturnUrl(relayStateUrl);
                    }
                    
                    if (!string.IsNullOrEmpty(relayStateUrl))
                    {
                        if (relayStateUrl.ToLower().Contains("impersonatedata"))
                        {
                            if (relayStateUrl.IndexOf("?") != -1)
                                relayStateUrl = relayStateUrl + "&logHistory=true";
                            else
                                relayStateUrl = relayStateUrl + "?logHistory=true";
                        }
                        else if (relayStateUrl.ToLower().Contains("azure"))
                        {
                            string tenantId = samlResponse.GetTenantId();
                            string encryptedTenantId = string.IsNullOrEmpty(tenantId) ? "" : SecurityHelper.CustomReverseEncode(tenantId);
                            string email = string.IsNullOrEmpty(userEmail) ? "" : SecurityHelper.CustomReverseEncode(userEmail);
                            return RedirectToAction("Register", "Home", new { _ptx = encryptedTenantId, _ru = email });
                        }
                    }

                    
                    FormsAuthentication.SetAuthCookie(userEmail, false);
                    #region set login cookie
                    HttpCookie loginCookie = new HttpCookie(ApiConstants.Login_Identifier);
                        loginCookie.Value = userEmail;
                        loginCookie.Expires = DateTime.Now.AddHours(8);
                        loginCookie.Domain = ApiConstants.PANW_EMPLOYEE_DOMAIN;
                        loginCookie.HttpOnly = false;
                        Response.Cookies.Add(loginCookie);
                    #endregion

                    SetLoginUserHistorySession();

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        if (returnUrl.ToLower().Contains("activate/flex")
                            || returnUrl.ToLower().Contains("activate/wfapi"))
                        {
                            returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?") + 1)
                                + HttpUtility.UrlEncode(returnUrl.Substring(returnUrl.IndexOf("?") + 1));
                        } 
                    }                    
                   
                   var redirectURL = scheme + "://" + host +
                                      (Request.Url.IsDefaultPort ? string.Empty : ":" + Request.Url.Port) + relayStateUrl;

                    return Redirect(redirectURL);
                }
                else
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(samlresponse));
                    return RedirectToActionPermanent("Index", "Support");
                }
            }
            else
            {
                ModelState.AddModelError("Unauthenticate", "You are not authorized to view this resource.");
                var samlEndpoint = _loginUrl + "?RelayState=" + returnUrl;
                return Redirect(samlEndpoint);
            }
        }

        private void ClearAll(string email)
        {
            if (!String.IsNullOrEmpty(email))
            {
                HttpContext.Cache.Remove(email);
            }
            CurrentState.ClearAll();
            Session.Clear();
            Session.Abandon();
        }

        private void AutoProvisionEmployee(string userEmail, string firstName, string lastName, string title, string department)
        {
            // Check if user is a Pan Employee and if he exists in UserAccounts.
            if (userEmail.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION))
            {
                ConfigValues configValues = new ConfigValues();
                bool allowedOrg = (configValues.GetAppSetting(AppSettings.AllowedOrgs)).Split(',').Contains(department);



                UserProfileModel userDetails = _supportAccountUserLogic.GetUserData(userEmail);
                if (userDetails == null)
                {
                    // Check if user is a Pan Employee and if he exists in UserAccounts.

                    // Register User in Support Site against 245 account
                    long membershipId;
                    string adminEmail = "donotreplycsp@paloaltonetworks.com";
                    long userAccountId = 0;
                    var uCountry = new Country()
                    {
                        Code = "US",
                        Name = "United States"
                    };
                    var userAdd = new Address()
                    {
                        Address1 = "4401 Great America Pkwy",
                        Address2 = string.Empty,
                        City = "Santa Clara",
                        Country = "US",
                        PostalCode = "95054",
                        CountryId = 233,
                        Region = "California"
                    };
                    var oUser = new UserAccount()
                    {
                        Address = userAdd,
                        CreatedByEmail = adminEmail,
                        CreatedByUserAccountId = 1,
                        CreatedOn = DateTime.Now,
                        DisplayName = userEmail.ToLower().Replace("@paloaltonetworks.com", ""),
                        Email = userEmail,
                        FirstName = firstName,
                        LastName = lastName,
                        Title = title,
                        RegisteredAppId = 23,  // csp autoprovision
                        IsActive = true
                    };
                    string errorMessage = string.Empty;
                    _userAccountLogic.RegisterUserAccount(oUser, 245, Guid.NewGuid().ToString(),
                        Common.GetClientIp(), out membershipId, out errorMessage, out userAccountId, allowedOrg: allowedOrg);
                }
                else if (!userDetails.IsActive)
                {
                    Api.Security.UserStatus userStatusDetails = _securityLogic.GetUserStatus(userEmail);
                    if (userStatusDetails != null & userStatusDetails.Disabled)
                    {
                        //restore user access
                        _supportAccountUserLogic.EnableUser(new Api.Security.EnableUser
                        {
                            email = userEmail,
                            isActive = true,
                            updateByUserAccountId = ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID
                        });
                    }
                    if (userDetails.IsBlocked)
                    {
                        //UnBlock user access
                        _supportAccountUserLogic.UnBlockUser(new Api.Security.UnblockUser
                        {
                            email = userEmail,
                            userAccountId = (long)userDetails.UserAccountId,
                            isBlock = false,
                            updateByUserAccountId = ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID
                        });

                    }
                }
            }
        }

        private void SetLoginUserHistorySession()
        {
            HttpCookie sessionCookie = new HttpCookie(ApiConstants.Login_User_Session_Identifier);
            sessionCookie.HttpOnly = true;
            sessionCookie.Secure = true;
            sessionCookie.Value = Session.SessionID;
            sessionCookie.Expires = DateTime.Now.AddHours(8);
            sessionCookie.Domain = ApiConstants.PANW_EMPLOYEE_DOMAIN;
            Response.Cookies.Add(sessionCookie);
        }

        private void LogToFile(string message)
        {
            try
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                System.IO.File.AppendAllText(logFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Silently fail if logging fails
            }
        }

        private string NormalizeLocalUrl(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
                return null;

            returnUrl = returnUrl.Trim();

            // Block dangerous schemes immediately
            if (returnUrl.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
                returnUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                returnUrl.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase))
            {
                LogToFile($"SECURITY: Blocked dangerous scheme in URL: {returnUrl}");
                return null;
            }

            // If it starts with ~/ convert to /
            if (returnUrl.StartsWith("~/"))
                returnUrl = returnUrl.Substring(1);

            // If it's a relative path without leading slash, add it
            if (!returnUrl.StartsWith("/") && 
                !returnUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                !returnUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                returnUrl = "/" + returnUrl;
            }

            return returnUrl;
        }

        private bool IsValidReturnUrl(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
                return false;

            // Normalize first
            returnUrl = NormalizeLocalUrl(returnUrl);
            
            if (returnUrl == null)
                return false;

            // Use MVC's built-in check for local URLs (most reliable)
            if (Url.IsLocalUrl(returnUrl))
                return true;

            // For absolute URLs, check against whitelist with proper validation
            try
            {
                var uri = new Uri(returnUrl, UriKind.RelativeOrAbsolute);
                
                if (!uri.IsAbsoluteUri)
                    return true;

                // Only allow HTTP and HTTPS schemes
                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                {
                    LogToFile($"SECURITY: Blocked non-HTTP(S) scheme: {uri.Scheme} in URL: {returnUrl}");
                    return false;
                }

                var host = uri.Host.ToLower();
                
                // Block invalid hostnames
                if (string.IsNullOrEmpty(host) || 
                    host.StartsWith(".") ||           
                    host.EndsWith(".") ||             
                    host.Contains("..") ||            
                    host.StartsWith("-") ||           
                    host.Contains("@"))               
                {
                    LogToFile($"SECURITY: Blocked malformed hostname in URL: {returnUrl}");
                    return false;
                }

                // Whitelist approved domains
                var allowedDomains = ConfigurationManager.AppSettings["AllowedRedirectDomains"]?.Split(',') 
                    ?? new[] { "paloaltonetworks.com"};

                // Check if host exactly matches or is a valid subdomain
                bool isAllowed = allowedDomains.Any(domain => 
                {
                    var domainLower = domain.Trim().ToLower();
                    
                    // Exact match
                    if (host.Equals(domainLower, StringComparison.OrdinalIgnoreCase))
                        return true;
                    
                    // Valid subdomain
                    if (host.EndsWith("." + domainLower, StringComparison.OrdinalIgnoreCase))
                    {
                        var subdomain = host.Substring(0, host.Length - domainLower.Length - 1);
                        return !string.IsNullOrEmpty(subdomain) && 
                               !subdomain.StartsWith(".") && 
                               !subdomain.EndsWith(".");
                    }
                    
                    return false;
                });

                if (!isAllowed)
                {
                    LogToFile($"SECURITY: Blocked redirect to non-whitelisted domain: {host}");
                }

                return isAllowed;
            }
            catch (UriFormatException ex)
            {
                LogToFile($"SECURITY: Invalid URI format: {returnUrl}, Error: {ex.Message}");
                return false;
            }
        }

        private string GetSafeReturnUrl(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
                return Url.Action("Index", "Support");

            var normalizedUrl = NormalizeLocalUrl(returnUrl);
            
            if (normalizedUrl != null && IsValidReturnUrl(normalizedUrl))
            {
                if (returnUrl != normalizedUrl)
                {
                    LogToFile($"Return URL normalized: {returnUrl} -> {normalizedUrl}");
                }
                return normalizedUrl;
            }
            
            LogToFile($"SECURITY: Blocked invalid return URL: {returnUrl}");
            return Url.Action("Index", "Support");
        }
    }
}
