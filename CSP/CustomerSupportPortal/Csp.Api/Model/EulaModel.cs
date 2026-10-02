using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class EulaModel
    {
        public int Context { get; set; }
        public bool IsEulaInMultipleLanguages { get; set; }
        public int SelectedLanguage { get; set; }
        public int SelectedEulaId { get; set; }
        public IEnumerable<EulaLanguages> Languages { get; set; }
        public IEnumerable<SimpleEula> Eulas { get; set; }
        public SimpleEula DefaultEula { get; set; }
    }

    public class SimpleEula
    {
        public int Id { get; set; }
        public decimal Version { get; set; }
        public string EulaText { get; set; }
        public int LanguageId { get; set; }
        public bool IsDefault { get; set; }
        public string EulaFileName { get; set; }
        public string EusaFileName { get; set; }
        public string EulaDownloadUrl { get; set; }
        public string EusaDownloadUrl { get; set; }
    }

    public class EulaAgreements
    {
        public int EulaContextId { get; set; }
        public int EulaId { get; set; }
        public decimal Version { get; set; }
        public int LanguageId { get; set; }
        public long SupportAccountId { get; set; }
        public long? DeviceId { get; set; }
        public long LicenseId { get; set; }
        public long? BulkRegistrationId { get; set; }
        public string SignedBy { get; set; }
        public DateTime SignedDate { get; set; }
        public string Notes { get; set; }
    }

    public class EulaLanguages
    {
        public int Id { get; set; }
        public string Language { get; set; }
    }
}
