using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQtoCSV;
using System.Collections;

namespace Csp.Api.Model
{
    public class CsspProvisionedDevicesMonthlyData<T>
        where T : IEnumerable
    {
        public T Items { get; set; }

        public long SupportAccountId { get; set; }

        public string SupportAccountName { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }
    }
}