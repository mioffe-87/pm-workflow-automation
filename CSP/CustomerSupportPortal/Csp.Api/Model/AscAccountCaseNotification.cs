using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Csp.Api.Model.Attributes;

namespace Csp.Api.Model
{
    [Serializable]
    public class AscAccountCaseNotification : IEquatable<AscAccountCaseNotification> {
        public string Id { get; set; }
        public string SfdcAccountId { get; set; }

        [StringLength(80, ErrorMessage = "Maximum length is 80 characters"),
        Email(ErrorMessage = "Please enter a valid email.")]
        public string EmailAddress { get; set; }
        public string FullName { get; set; }
        public bool IsCaseCommentNotification { get; set; }
        public bool IsCaseCreationNotification { get; set; }
        public bool IsCriticalCaseCreationNotification { get; set; }

        public bool Equals(AscAccountCaseNotification other) {
            if (other == null)
                return false;

            if (this.Id == other.Id && this.SfdcAccountId == other.SfdcAccountId && this.EmailAddress == other.EmailAddress
                && this.IsCaseCommentNotification == other.IsCaseCommentNotification && this.IsCaseCreationNotification == other.IsCaseCreationNotification)
                return true;
            else
                return false;
        }
    }
}
