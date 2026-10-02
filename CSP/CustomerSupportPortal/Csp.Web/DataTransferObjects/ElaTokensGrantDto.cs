using System;
using System.Collections.Generic;
using Csp.Api.Model;


namespace Csp.Web.DataTransferObjects
{
    public class ElaTokensGrantDto
    {
        public EulaModel Eula { get; set; }
        public IEnumerable<ElaTokensGrantModel> ElaTokensGrantItems { get; set; }

        public ElaTokensGrantDto()
        {
            ElaTokensGrantItems = new List<ElaTokensGrantModel>();
        }
    }
}