using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;

namespace Csp.Api.Model
{
    public class MfaRegistrationModel
    {
        public long UserAccountId { get; set; }

        public string UserEmail { get; set; }

        public MultiFactorAuthType MultiFactorAuthType { get; set; }

        public MfaRegistrationStatus RegistrationStatus { get; set; }

        public bool IsRegistered { get; set; }

        public string StatusCode { get; set; }

        public string StatusMessage { get; set; }

        public long UpdatedByUserAccountId { get; set; }

        public string GoogleAuthenticatorSecret { get; set; }

        public bool UnEnrollGoogleAuth { get; set; } 
    }
}
