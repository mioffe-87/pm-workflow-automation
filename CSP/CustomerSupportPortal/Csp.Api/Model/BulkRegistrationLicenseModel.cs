using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Model
{
    public class BulkRegistrationLicenseModel
    {
        public long Id { get; set; }
        public long BulkRegistrationDeviceId { get; set; }

        public string AuthorizationCode { get; set; }
        public long PartMasterId { get; set; }
        public string Type { get; set; }
        public DateTime? ExpectedExpirationDate { get; set; }
        public string SalesOrderNumber { get; set; }

        public long LicenseId { get; set; }
        public List<PrepBulkRegistration.BundleModel> Bundles { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool IsActivationSuccessful { get; set; }
        public string ActivationNotes { get; set; }
        public DateTime? ActivatedOn { get; set; }
    }
}
