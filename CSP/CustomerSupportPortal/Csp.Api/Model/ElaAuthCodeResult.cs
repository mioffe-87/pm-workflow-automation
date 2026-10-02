using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class ElaAuthCodeResult : AddVmAuthCodeResult
    {
        public ElaAuthCodeResult()
        {
            
        }

        public ElaAuthCodeResult(string errorMessage) : base(errorMessage)
        {            
        }
    }
}
