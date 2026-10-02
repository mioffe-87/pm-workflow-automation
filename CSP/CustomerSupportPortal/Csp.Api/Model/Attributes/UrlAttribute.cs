using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Attributes {
    public class UrlAttribute : RegularExpressionAttribute {
        public UrlAttribute() : base(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \?=.-]*)*\/?$") { 
        }
	}
}
