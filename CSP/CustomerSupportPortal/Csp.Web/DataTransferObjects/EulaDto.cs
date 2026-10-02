using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class EulaDto
    {
        public int Id { get; set; }
        public string Eula { get; set; }
        public string EulaFileName { get; set; }
        public string EulaDownloadUrl { get; set; }
        public int LanguageId { get; set; }
    }
}