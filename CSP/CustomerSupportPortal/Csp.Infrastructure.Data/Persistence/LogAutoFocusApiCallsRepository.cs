using System;
using System.Linq;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence
{
    public class LogAutoFocusApiCallsRepository : ILogAutoFocusApiCallsRepository
    {
        public void LogApiCall(string apiMethodCalled, string inputs, string outputs, long userAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var createdByEmail = db.useraccounts.Single(x => x.UserAccountId == userAccountId).Email;

            db.logautofocusapicalls.Add(new logautofocusapicall
            {
                ApiMethodCalled = apiMethodCalled,
                Inputs = inputs,
                Outputs = outputs,
                CreatedOn = DateTime.Now,
                CreatedByEmail = createdByEmail
            });

            db.SaveChanges();
        }
    }
}
