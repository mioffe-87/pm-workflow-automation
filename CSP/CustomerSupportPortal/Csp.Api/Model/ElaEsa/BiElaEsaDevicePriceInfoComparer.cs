using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class BiElaEsaDevicePriceInfoComparer : IEqualityComparer<BiElaEsaDevicePriceInfo>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(BiElaEsaDevicePriceInfo item1, BiElaEsaDevicePriceInfo item2)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(item1, item2))
            {
                return true;
            }

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(item1, null)
                || Object.ReferenceEquals(item2, null))
            {
                return false;
            }

            //Check whether the properties are equal.
            return item1.EaType == item2.EaType
                && item1.AuthCode == item2.AuthCode
                && item1.SupportAccountId == item2.SupportAccountId
                && item1.SerialNumber == item2.SerialNumber;
        }

        // If Equals() returns true for a pair of objects then GetHashCode() must return the same value for these objects.
        public int GetHashCode(BiElaEsaDevicePriceInfo obj)
        {
            return Object.ReferenceEquals(obj, null)
                ? 0 //Check whether the object is null
                :   //Calculate the hash code for the item.
                    (obj.SerialNumber == null ? 0 : obj.SerialNumber.GetHashCode())
                    ^ obj.EaType.GetHashCode()
                    ^ obj.SerialNumber.GetHashCode()
                    ^ obj.SupportAccountId.GetHashCode()
                    ^ obj.AuthCode.GetHashCode();
        }
    }
}
