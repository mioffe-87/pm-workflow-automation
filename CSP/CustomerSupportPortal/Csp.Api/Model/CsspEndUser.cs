using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Attributes;

namespace Csp.Api.Model
{
    public class CsspEndUser
    {
        public long CSSPEndUsersId { get; set; }
        public long CSSPSupportAccountId { get; set; }
        public long? ProvisionedDeviceId { get; set; }
        public string ProvisionedDeviceIds { get; set; }

        [Required(ErrorMessage = "Customer AccountId is required")]
        [RegularExpression(@"^(?:[A-Za-z0-9\s_@%!#?,.\\:/\^\|\$\&\*-])+$", ErrorMessage = "Invalid Character. Allowed Characters are @ % | \\ / ! # $ ^ ? : - _^,&*.")]
        public string CustomerAccountId { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        public string CompanyName { get; set; }


        public string DnBNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Country is required")]
		public long? CountryId {
			set;
			get;
		}

        public string Country { get; set; }

        [StringLength(45, ErrorMessage = "Maximum length is 45 characters")]
        public string Region { get; set; }

        [StringLength(100, ErrorMessage = "Maximum length is 100 characters"),
        Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [StringLength(15, ErrorMessage = "Maximum length is 15 characters"),
        Required(ErrorMessage = "Postal Code is required")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Email is required"),
		Email(ErrorMessage = "Please enter a valid email.")]
        public string EndUserContactEmail { get; set; }

        [StringLength(40, ErrorMessage = "Maximum length is 40 characters"),
        Required(ErrorMessage = "Phone is required")]
        public string PhoneNumber
        {
            get;
            set;
        }

        [RegularExpression(@"[a-zA-Z0-9.-:-//]+(.[a-zA-Z/]{2,5})", ErrorMessage = "Website is invalid")]
        public string CompanyWebsite
        {
            get;
            set;
        }

        public string Industry
        {
            get;
            set;
        }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string FullAddress
        {
            get
            {
                if (string.IsNullOrEmpty(Address))
                {
                    return string.Empty;
                }
                else
                {
                    return Address + ", " + City + ", " + Region + " " + PostalCode + ", " + Country;
                }
            }
        }

    }
}
