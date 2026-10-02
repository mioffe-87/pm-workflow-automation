using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Api.Model;
using Newtonsoft.Json;

namespace Csp.Web.DataTransferObjects
{
    public class AllocateCnSeriesTokensDto : DefaultDto
    {
        public int AvailableTokens { get; set; }

        public int MaxNewRecords { get; set; }

        public List<AllocateCnSeriesTokensToPanoramaDto> AvailablePanoramas { get; set; }

        public List<AllocateCnSeriesTokensToPanoramaDto> Panoramas { get; set; }

        public List<string> Cnxs { get; set; }

        // Fixing the issue w/ HTTP Form object deserialisation
        public string panoramasString = null;
        public string PanoramasString 
        {
            get
            {
                return panoramasString;
            }
            set
            {
                panoramasString = value;
                Panoramas = JsonConvert.DeserializeObject<List<AllocateCnSeriesTokensToPanoramaDto>>(panoramasString);
            }
        }

        public string ErrorMessage { get; set; }

        public AllocateCnSeriesTokensDto()
        {
            Panoramas = new List<AllocateCnSeriesTokensToPanoramaDto>();
        }
    }
}