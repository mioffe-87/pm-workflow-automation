using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class FwFlexDateFormatConverter : IsoDateTimeConverter
    {
        public FwFlexDateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
