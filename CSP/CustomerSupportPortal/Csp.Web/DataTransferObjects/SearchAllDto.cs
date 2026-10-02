using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model; 

namespace Csp.Web.DataTransferObjects {
	public class SearchAllDto:DefaultDto {
		public SearchAllViewModel SearchAll {
			get;
			set;
		}

		public string Title { get;set; }
	}
}