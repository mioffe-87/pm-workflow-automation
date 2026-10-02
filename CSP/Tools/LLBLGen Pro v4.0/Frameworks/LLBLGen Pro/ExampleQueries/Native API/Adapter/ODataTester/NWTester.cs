using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using System.Data.Services.Client;
using System.Data.Services.Common;
using ODataTester.NorthwindService;

namespace ServiceTester
{
	[TestFixture()]
	public class NWTester
	{
		private NorthwindService _ctx;

		[TestFixtureSetUp()]
		public void Init()
		{
			CreateContext();
		}

		private void CreateContext()
		{
			_ctx = new NorthwindService(new Uri(@"http://lhalias:3931/NorthwindService.svc"));
			_ctx.IgnoreResourceNotFoundException = true;
		}

		[Test]
		public void FetchProduct()
		{
		}
	}
}