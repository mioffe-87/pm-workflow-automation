using Csp.Api.SalesforceWebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model.Attributes;
using System.Data;

namespace Csp.Api.Model
{
    public class TrapsELAAuthCodeDetails : BaseCspModel
    {
        [DataAccess(VirtualMachineFields.AuthorizationCode, DataType = DbType.String)]
        public string AuthCode { get; set; }

        [DataAccess(VirtualMachineFields.RegistrationDate, DataType = DbType.DateTime)]
        public DateTime? RegistrationDate { get; set; }

        [DataAccess(VirtualMachineFields.ExpirationDate, DataType = DbType.DateTime)]
        public DateTime? ExpirationDate { get; set; }

        [DataAccess(VirtualMachineFields.ShipDate, DataType = DbType.DateTime)]
        public DateTime? ShipDate { get; set; }

        [DataAccess(VirtualMachineFields.LicenseTermMonths, DataType = DbType.Int32)]
        public int LicenseTermMonths { get; set; }

        [DataAccess(VirtualMachineFields.IsActive, DataType = DbType.Boolean)]
        public bool IsActive { get; set; }

        [DataAccess(VirtualMachineFields.PartNumber, DataType = DbType.String)]
        public string PartNumber { get; set; }

        [DataAccess(VirtualMachineFields.VmStubDeviceId, DataType = DbType.Int64)]
        public long? VmStubDeviceId { get; set; }

        [DataAccess(VirtualMachineFields.PanDbIdentification, DataType = DbType.String)]
        public string PanDbIdentification { get; set; }

    }

    public class TrapsELAAuthCodeResult
    {

        public TrapsELAAuthCodeResult()
        {

        }

        public TrapsELAAuthCodeResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; set; }
    }
}
