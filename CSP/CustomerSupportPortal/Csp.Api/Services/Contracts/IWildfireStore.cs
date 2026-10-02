using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts {
	public interface IWildfireStore
	{
		bool StoreToWildfireCores(string apikey, int companyid, DateTime expiration);
        WildfireApiResponse WildfireReconcile(long supportAccountId);
        WildfireApiResponse GenerateWildFireApiKey(WildfireApiRequest wildfireRequest);
        ResponseWildfireKeyList GetWildfireKeys(string supportAccountId, int page, int size);
        void RevokeWildfireKey(string apiKey);
	}
}
