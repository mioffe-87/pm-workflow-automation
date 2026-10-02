using System;
using System.Collections.Generic;

namespace Csp.Api.Services.Contracts {
	public interface IUserRegistrationRepository {
		bool IsEmailInUse(string email);
		bool IsDisplayNameInUse(string displayName);
		IEnumerable<long> GetSalesOrderSupportAccountIdsByDeviceSerialNumber(string serialNumber);
		IEnumerable<long> GetSalesOrderSupportAccountIdsByLicenseAuthCode(string authCode);
		long[] GetSalesOrderSupportAccountIdsBySapId(string sapId);
		long[] GetSalesOrderSupportAccountIdsBySalesOrderNumber(string salesOrderNumber);

		long[] GetSalesOrderSapIdsBySalesOrderNumber(string salesOrderNumber);
		long[] GetSupportAccountIdsBySfAccountId(string sfAccountId, string evalId);

		bool IsValidDeviceSerialNumber(string serialNumber);
		bool IsValidLicenseAuthCode(string authCode);

		bool IsEvalDeviceCode(string deviceCode);
	    SelfRegistrationCodeModel GetSelfRegistrationByCode(string code);
	}

    public class SelfRegistrationCodeModel
    {
        public long SupportAccountId { get; set; }
        public string Code { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsActive { get; set; }

        public bool IsActiveAndUnexpired
        {
            get
            {
                return IsActive && (
                    ExpirationDate == null
                    || ExpirationDate > DateTime.Now
                    );
            }
        }
    }
}
