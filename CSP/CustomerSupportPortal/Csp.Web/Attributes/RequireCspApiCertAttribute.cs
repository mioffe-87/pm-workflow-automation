using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Csp.Web.Attributes
{
    public class RequireCspApiCertAttribute : AuthorizationFilterAttribute
    {
        private readonly string expectedCn = ConfigurationManager.AppSettings.Get("CspApiDotNetServerCertCn") ?? "INVALID_CN";
        private readonly string expectedOu = ConfigurationManager.AppSettings.Get("CspApiDotNetServerCertOu") ?? "INVALID_OU";
        private readonly string expectedO = ConfigurationManager.AppSettings.Get("CspApiDotNetServerCertO") ?? "INVALID_O";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "HTTPS is required."
                };
            }
            else
            {
                X509Certificate2 cert = actionContext.Request.GetClientCertificate();
                if (cert == null)
                {
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                    {
                        ReasonPhrase = "Client Certificate is required."
                    };
                }
                else
                {
                    Func<X509Chain, X509Certificate2, bool> check = (n, c) =>
                    {
                        return n.ChainElements.Count > 1  // cert chain is expected
                            && c.NotBefore < DateTime.Now // check for expiration
                            && c.NotAfter >= DateTime.Now
                            && c.GetNameInfo(X509NameType.SimpleName, false) == expectedCn                  // check for Common Name
                            && Regex.Match(c.Subject, @"OU\s?=(.*?),\s").Groups[1].ToString() == expectedOu // check for Organization Unit
                            && Regex.Match(c.Subject, @"O\s?=(.*?),\s").Groups[1].ToString() == expectedO;  // check for Organization
                    };
                    X509Chain chain = new X509Chain
                    {
                        ChainPolicy = new X509ChainPolicy()
                        {
                            RevocationMode = X509RevocationMode.NoCheck,
                        }
                    };
                    try
                    {
                        if (!chain.Build(cert))
                        {
                            string message = "Client Certificate chain is not valid.";
                            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden) { ReasonPhrase = message };
                        }
                        if (!check(chain, cert))
                        {
                            string message = "Client Certificate is not valid.";
                            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden) { ReasonPhrase = message };
                        }
                    }
                    catch (Exception ex)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden) { ReasonPhrase = "Client Certificate is not valid." };
                    }
                }

                base.OnAuthorization(actionContext);
            }
        }
    }
}