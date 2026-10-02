using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Csp.Api.Logic;
using Csp.Api.SalesforceWebApi;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.Persistence;

namespace Csp.Web
{
    /// <summary>
    /// Summary description for PartnerEval
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]

    //private key = 35b8d46a-bdf6-4014-b6cc-8b5a26117c63
    public class PartnerEval : System.Web.Services.WebService
    {
        private string privateKey = "35b8d46a-bdf6-4014-b6cc-8b5a26117c63";

        private readonly IEncryptionProvider _encryptionProvider;
        private readonly IPartnerEvalLogic _partnerEvalLogic;

        public PartnerEval()
        {
            _encryptionProvider = new EncryptionProvider();
            _partnerEvalLogic = new PartnerEvalLogic(new PartnerEvalRepository());
        }

        [WebMethod]
        public List<DeviceDetails> IsValidPartnerEval(string serialNumber, string partnerLoginId, string key)
        {
            List<DeviceDetails> data = new List<DeviceDetails>();
            var isKeyValid = _encryptionProvider.IsKeyValid(serialNumber + partnerLoginId, key, privateKey);
            var error = new DeviceDetails { Error = "Invalid key." };
            if (!isKeyValid)
            {
                data.Add(error);
                return data;
            }

            return _partnerEvalLogic.AllInfoReturned(serialNumber, partnerLoginId);
            //var licenses = data.Licenses;
            //var licenseData = new LicenseDetails[]{};

            //foreach (var li in licenses)
            //{
            //    licenseData.
            //}

            //return new DeviceDetails
            //{
            //    SerialNumber = data.SerialNumber,
            //    DeviceType = data.DeviceType,
            //    DeviceTag = data.DeviceTag,
            //    Licenses = data.Licenses,

            //};


            //return new DeviceDetails
            //{
            //    SerialNumber = "010401001433",
            //    DeviceType = "PAN-PA-3060-NFR",
            //    DeviceTag = "Sepia-b",
            //    RegistrationDate = new DateTime(2015, 09, 22),
            //    Licenses = new []
            //    {
            //        new LicenseDetails
            //        {
            //            AuthCode = "97982611",
            //            LicenseType = "PAN-PA-3060-BND-NFR3",
            //            RegistrationDate = new DateTime(2015,09,22),
            //            ExpirationDate = new DateTime(2016,09,22),
            //            Description = "NFR unit bundle subscription (Threat prevention, URL filtering, Global protect & gateway, WildFire, Standard support)",
            //        },
            //        new LicenseDetails
            //        {
            //            AuthCode = "null",
            //            LicenseType = "PAN-SVC-STND-3060",
            //            RegistrationDate = new DateTime(2015,09,22),
            //            ExpirationDate = new DateTime(2016,09,22),
            //            Description = "Standard support year 1, PA-3060",
            //        },
            //        new LicenseDetails
            //        {
            //            AuthCode = "null",
            //            LicenseType = "PAN-PA-3060-TP",
            //            RegistrationDate = new DateTime(2015,09,22),
            //            ExpirationDate = new DateTime(2016,09,22),
            //            Description = "Threat prevention subscription year 1, PA-3060",
            //        }, 
            //    },
            //    City = "Singapore",
            //    Country = "Singapore",
            //};
        }
    }

    //public class DeviceDetails
    //{
    //    public string SerialNumber { get; set; }
    //    public string DeviceType { get; set; }
    //    public string DeviceTag { get; set; }
    //    public DateTime? RegistrationDate { get; set; }
    //    public LicenseDetails[] Licenses { get; set; }
    //    public string City { get; set; }
    //    public string Country { get; set; }
    //    public string Error { get; set; }
    //}

    //public class LicenseDetails
    //{
    //    public string AuthCode { get; set; }
    //    public string LicenseType { get; set; }
    //    public string Description { get;set; }
    //    public DateTime RegistrationDate { get; set; }
    //    public DateTime ExpirationDate { get; set; }
    //}
}