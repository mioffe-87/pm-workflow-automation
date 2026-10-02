using System;
using System.Threading;
using System.Transactions;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic.Salesforce {
	/// <summary>
	/// Just imagine your fingers around the neck
	/// of salesforce and squeeze...
	/// </summary>
	public class SalesforceThrottler : SalesforceLogic, ISalesforceService {

        private readonly ILookupRepository _lookupRepository;

		/// <summary>
		/// MaxConcurrentRequests are dictated by salesforce and are fixed at 10
		/// The problem is that there are 2 other ways that there is also a
		/// web service and a console app that will make salesforce calls.
		/// 
		/// Eventually they should be moved to their own username/pass, but
		/// for now we need to limit concurrent requests to 8 so that 2 will
		/// be reserved for the other 2 uses
		/// </summary>
		private const int MaxConcurrentRequests = 8;
		private static readonly SemaphoreSlim SharedSemaphoreSlim = new SemaphoreSlim(MaxConcurrentRequests, MaxConcurrentRequests);

		public SalesforceThrottler(ISalesforceLogsRepository salesforceLogsRepository, ILookupRepository lookupRepository) {
			baseSalesforceLogsRepository = salesforceLogsRepository;
		    _lookupRepository = lookupRepository;
		}

		private const int MaxWaitMilliseconds = 15 * 1000;

		public AscContactInfo GetAscContactInfo(string id) {

			var isSuccess = SharedSemaphoreSlim.Wait(MaxWaitMilliseconds);
			if (!isSuccess) {
				throw new Exception("Timed out waiting for Semaphore in GetAscContactInfo");
			}

			try {
				return GetAscContactInfoRetryAble(id);
			} finally {
				SharedSemaphoreSlim.Release();
			}
		}

		public SalesforceUserModel AddUpdateSalesforceContact(UserAccount userAccount, long? supportAccountId, string salesforceId, long membershipId,
                                                                                     long createdByUserAccountId, bool isCsspDefaultAccount = false)
        {
            var salesforceUserModel = new SalesforceUserModel();

            if (userAccount.Address != null && userAccount.Address.CountryId.HasValue &&
                string.IsNullOrEmpty(userAccount.Address.Country))
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    userAccount.Address.Country = _lookupRepository.GetCountryNameById(userAccount.Address.CountryId.Value);
                }
            }

			var isSuccess = SharedSemaphoreSlim.Wait(MaxWaitMilliseconds);
			if (!isSuccess) {
				throw new Exception("Timed out waiting for Semaphore in AddUpdateSalesforceContact");
			}

			try {
                salesforceUserModel = AddUpdateSalesforceContactRetryAble(userAccount, supportAccountId, salesforceId, membershipId, createdByUserAccountId, isCsspDefaultAccount);
			} finally {
				SharedSemaphoreSlim.Release();
			}

            return salesforceUserModel;
        }

		public void UpdateSalesforceContactsAccount(UserAccount userAccount, long? supportAccountId, string currentSfdcId, string newSfdcId) {

			var isSuccess = SharedSemaphoreSlim.Wait(MaxWaitMilliseconds);
			if (!isSuccess) {
				throw new Exception("Timed out waiting for Semaphore in AddUpdateSalesforceContact");
			}

			try {
				UpdateSalesforceContactsAccountRetryAble(userAccount, supportAccountId, currentSfdcId, newSfdcId);
			} finally {
				SharedSemaphoreSlim.Release();
			}
		}
        public void DeleteSalesforceContactsAccount(string email , long supportAccountId)
        {            
                DeleteSalesforceContactsAccountRetryAble(email, supportAccountId);          
        }

        public SalesforceEvalData GetSalesforceEvalDataByEvalId(string evalId) {

			var isSuccess = SharedSemaphoreSlim.Wait(MaxWaitMilliseconds);
			if (!isSuccess) {
				throw new Exception("Timed out waiting for Semaphore in GetSalesforceEvalDataByEvalId");
			}

			try {
				return GetSalesforceEvalDataByEvalIdRetryAble(evalId);
			} finally {
				SharedSemaphoreSlim.Release();
			}
		}

        public bool getSfAccountTypeByEmail(string email)
        {
            var isSuccess = SharedSemaphoreSlim.Wait(MaxWaitMilliseconds);
            if (!isSuccess)
            {
                throw new Exception("Timed out waiting for Semaphore in GetSfAccountTypeByEmail");
            }

            try
            {
                return getSfAccountTypeByEmailRetryAble(email);
            }
            finally
            {
                SharedSemaphoreSlim.Release();
            }
        }
	}
}