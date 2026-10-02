using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using System.Data.Services.Client;
using System.Data.Services.Common;
using ODataTester.UnitTestService;

namespace ServiceTester
{
	[TestFixture()]
	public class CUDTester
	{
		private UnitTestService _ctx;

		[TestFixtureSetUp()]
		public void Init()
		{
			CreateContext();
		}

		private void CreateContext()
		{
			_ctx = new UnitTestService(new Uri(@"http://lhalias:3931/UnitTestService.svc"));
			_ctx.IgnoreResourceNotFoundException = true;
		}

		[Test]
		public void InsertUpdateDeleteAddress()
		{
			// first get an empty list of addresses, as no address is there. 
			var addresses = _ctx.Address.ToList();
			Assert.AreEqual(0, addresses.Count);

			var newAddress = new AddressEntity()
			{
				City = "The Hague",
				Country = "NL",
				StreetName = "Zeilstraat",
				HouseNumber = 99,
				Zipcode = "2586 RB",
				TestRunId = null
			};
			_ctx.AddToAddress(newAddress);
			_ctx.SaveChanges();

			Assert.IsTrue(newAddress.AddressId > 0);

			CreateContext();

			var fetchedAddress = _ctx.Address.FirstOrDefault();
			Assert.IsNotNull(fetchedAddress);
			Assert.IsTrue(fetchedAddress.AddressId > 0);
			Assert.AreEqual(newAddress.StreetName, fetchedAddress.StreetName);

			fetchedAddress.Country = "Nederland";
			_ctx.UpdateObject(fetchedAddress);
			_ctx.SaveChanges();

			CreateContext();
			fetchedAddress = _ctx.Address.FirstOrDefault();
			Assert.IsNotNull(fetchedAddress);
			Assert.IsTrue(fetchedAddress.AddressId > 0);
			Assert.AreEqual("Nederland", fetchedAddress.Country);
				
			_ctx.DeleteObject(fetchedAddress);
			_ctx.SaveChanges();

			CreateContext();
			fetchedAddress = _ctx.Address.FirstOrDefault();
			Assert.IsNull(fetchedAddress);
		}
	}
}