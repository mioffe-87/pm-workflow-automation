using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class SoftwareUpdateFileInfo
    {
        public string FolderName { get; set; }
        public string FileName { get; set; }
        public string Platform { get; set; }
        public string Product { get; set; }
        public string ContentType { get; set; }
        public string GroupIds { get;set;}
        public string ProductFilterId { get; set; }
        public string CarrierId { get; set; }
        public string Label { get; set; }
        public string CurrentVersionId { get; set; }
    }
}
