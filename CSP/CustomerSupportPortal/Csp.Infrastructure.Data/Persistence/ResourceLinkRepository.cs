using System.Collections.Generic;
using System.Linq;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Api.Model;

namespace Csp.Infrastructure.Data.Persistence {

	public class ResourceLinkRepository : IResourceLinkRepository {

        private const string SecurityLifeCycleFeature = "SECURITY_LIFE_CYCLE";

		public IEnumerable<ResourceLink> GetResourceLinks() {

			//			var db = SessionDbContext.CspObjectContext;
			//return (
			//					 from rlinks in db.resourcelinks 
			//														 join rgroups in db.resourcelinkgroups on rlinks.ResourceLinkGroupID equals rgroups.ResourceLinkGroupID
			//														 where rlinks.IsActive ==  true && rgroups.IsActive == true 
			//					 orderby rgroups.OrderBy,rlinks.OrderBy
			//	select new ResourceLink {
			//						 GroupName = rgroups.GroupName,
			//						 Name = rlinks.Name,
			//						 Url = rlinks.Url,
			//						// ImageUrl = rlinks.Image,
			//															 HelpId = rlinks.HelpId,
			//															 ResourceId = rlinks.ResourceId,
			//															 ResourceSubLinks = db.resourcesublinks.Select(a => new ResourceSubLink()
			//															 {
			//																	 Name = a.Name,
			//																	 HelpId = a.HelpId,
			//																	 ResourceId = a.ResourceId,
			//																	 ResourcelinkId = a.ResourceLinkId,
			//																	 Url = a.Url,
			//																	 OrderBy = a.OrderBy
			//															 }).Where(a => a.ResourcelinkId == rlinks.Id).OrderBy(a =>a.OrderBy)
			//	}).ToArray();
			return new List<ResourceLink>();
		}

        public bool HasSLRBetaAccess(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            var hasAccess = db.betaaccesses.Any(x => x.FeatureName == SecurityLifeCycleFeature && x.SupportAccountId == supportAccountId);

            return hasAccess;
        }
	}
}
