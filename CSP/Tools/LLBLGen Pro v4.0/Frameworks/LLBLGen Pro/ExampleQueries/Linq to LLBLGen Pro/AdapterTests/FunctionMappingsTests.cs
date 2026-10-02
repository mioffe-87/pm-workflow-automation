using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NW26.Adapter.DatabaseSpecific;
using NW26.Adapter.EntityClasses;
using NW26.Adapter.Linq;
using SD.Tools.OrmProfiler.Interceptor;


namespace LinqTester.AdapterTests
{
	/// <summary>
	/// Class which contains the tests which focuss on function mappings of .NET methods/properties to db functions. Fetches of entities and dyn. lists are
	/// combined in this class as the focus isn't on these two separate types of fetches but solely on function calls. 
	/// </summary>
	[TestFixture]
	public class FunctionMappingsTests
	{
		#region Array method / property mapping tests
		/// <summary>
		/// Gets the employees based on length test on photo field. Test for Array.Length
		/// </summary>
		[Test]
		public void ArrayLengthTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						where e.Photo.Length > 4000
						select e;
				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(7, count);
			}
		}
		#endregion

		#region Cast/Convert method / property mapping tests
		/// <summary>
		/// Tests a cast from int to short in the projection.
		/// </summary>
		[Test]
		public void ConvertToShortUsingCastTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						select (Int16)o.OrderId;

				foreach(var v in q)
				{
					Assert.AreEqual(typeof(Int16), v.GetType());
				}
			}
		}


		/// <summary>
		/// Gets the order id as boolean. Test for ToBoolean
		/// </summary>
		[Test]
		public void ConvertToBooleanTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Convert.ToBoolean(o.OrderId)
						&& o.OrderId==10254
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(10254, v.OrderId);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void ConvertToBooleanWithIIFInProjectionTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10254
						&& (Convert.ToBoolean(o.OrderId) ? "T" : "F")=="T"
						select new
						{
							Value = Convert.ToBoolean(o.OrderId) ? "T":"F",
							o.OrderId
						};

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(10254, v.OrderId);
					Assert.AreEqual("T", v.Value);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void ConvertToBooleanInProjectionTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10254
						select new
						{
							Value = Convert.ToBoolean(o.OrderId),
							o.OrderId
						};

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(10254, v.OrderId);
					Assert.IsTrue(v.Value);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void ConvertToBooleanWithToStringTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Convert.ToBoolean(o.OrderId).ToString()=="True"
						&& o.OrderId==10254
						select new
						{
							Value = Convert.ToBoolean(o.OrderId),
							o.OrderId
						};

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(10254, v.OrderId);
					Assert.IsTrue(v.Value);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		/// <summary>
		/// Filters on a bit field converted to byte. 
		/// </summary>
		[Test]
		public void ConvertToByteTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				byte compareValue = (byte)1;
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where Convert.ToByte(p.Discontinued)==compareValue
						select p;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Discontinued);
				}
				Assert.AreEqual(55, count);
			}
		}

		/// <summary>
		/// Filters on a bit field converted to Int16. 
		/// </summary>
		[Test]
		public void ConvertToShortTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				short compareValue = (short)1;
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where Convert.ToInt16(p.Discontinued) == compareValue
						select p;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Discontinued);
				}
				Assert.AreEqual(55, count);
			}
		}

		/// <summary>
		/// Filters on a bit field converted to Int32
		/// </summary>
		[Test]
		public void ConvertToIntTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				int compareValue = 1;
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where Convert.ToInt32(p.Discontinued) == compareValue
						select p;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Discontinued);
				}
				Assert.AreEqual(55, count);
			}
		}

		/// <summary>
		/// Filters on a bit field converted to Int64
		/// </summary>
		[Test]
		public void ConvertToLongTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				long compareValue = 1;
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where Convert.ToInt64(p.Discontinued) == compareValue
						select p;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Discontinued);
				}
				Assert.AreEqual(55, count);
			}
		}

		/// <summary>
		/// Filters on a bit field converted to char
		/// </summary>
		[Test]
		public void ConvertToCharTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				char compareValue = '1';
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where Convert.ToChar(p.Discontinued) == compareValue
						select p;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Discontinued);
				}
				Assert.AreEqual(55, count);
			}
		}


		[Test]
		public void ConvertToDateTimeTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId==10254
						select Convert.ToDateTime(o.OrderId);

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual(new DateTime(1928, 1, 29), v);
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void ConvertToDecimalTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				decimal compareValue = 1.0M;
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where Convert.ToDecimal(p.Discontinued) == compareValue
						select p;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Discontinued);
				}
				Assert.AreEqual(55, count);
			}
		}

		[Test]
		public void ConvertToDoubleTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				double compareValue = 1.0;
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where Convert.ToDouble(p.Discontinued) == compareValue
						select p;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Discontinued);
				}
				Assert.AreEqual(55, count);
			}
		}

		[Test]
		public void ConvertToSingleTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				float compareValue = 1.0f;
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where Convert.ToSingle(p.Discontinued) == compareValue
						select p;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Discontinued);
				}
				Assert.AreEqual(55, count);
			}
		}

		[Test]
		public void ConvertToStringTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				string compareValue = "1";
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where Convert.ToString(p.Discontinued) == compareValue
						select p;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Discontinued);
				}
				Assert.AreEqual(55, count);
			}
		}
		#endregion

		#region String method / property mapping tests
		[Test]
		public void StringCompareTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where String.Compare(c.Country, "USA")==0
						select c;

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(13, customers.Count);
			}
		}

		[Test]
		public void StringIndexOf1Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country.IndexOf('A')==1
						select c;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(1, v.Country.IndexOf("A", 0, StringComparison.InvariantCultureIgnoreCase));
					count++;
				}
				Assert.AreEqual(3, count);
			}
		}


		[Test]
		public void StringIndexOf2Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country.IndexOf('A', 2) == 4
						select c;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(4, v.Country.IndexOf("A", 2, StringComparison.InvariantCultureIgnoreCase));
					count++;
				}
				Assert.AreEqual(17, count);
			}
		}

		[Test]
		public void StringLastIndexOf1Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country.LastIndexOf("NA") == 2
						select c;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(2, v.Country.ToUpper().LastIndexOf("NA"));
					count++;
				}
				Assert.AreEqual(3, count);
			}
		}

		[Test]
		public void StringLastIndexOf2Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country.LastIndexOf("NA", 5) == 2
						select c;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(2, v.Country.ToUpper().LastIndexOf("NA", 5));
					count++;
				}
				Assert.AreEqual(3, count);
			}
		}


		/// <summary>
		/// Gets all customers who have A company name which length is longer than20 characters. Test for the function mapping of String.Length and 
		/// calls Trim() in front of it to see if it can handle complicated scenarios.
		/// </summary>
		[Test]
		public void StringTrimAndLengthCombinedCallTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.CompanyName.Trim().Length > 20
						select c;

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(30, customers.Count);
			}
		}

		[Test]
		public void StringPadLeft1Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						select c.Country.Trim().PadLeft(20);

				foreach(var v in q)
				{
					Assert.AreEqual(20, v.Length);
				}
			}
		}


		[Test]
		public void StringPadLeft2Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country!=null
						select c.Country.PadLeft(20, '-');

				foreach(var v in q)
				{
					Assert.AreEqual(20, v.Trim().Length);
				}
			}
		}


		[Test]
		public void StringPadRight1Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						select c.Country.Trim().PadRight(20);

				foreach(var v in q)
				{
					Assert.AreEqual(20, v.Length);
				}
			}
		}


		[Test]
		public void StringPadRight2Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						select c.Country.Trim().PadRight(20, '-');

				foreach(var v in q)
				{
					Assert.AreEqual(20, v.Trim().Length);
				}
			}
		}


		[Test]
		public void StringRemove1Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country!=null
						select c.Country.Remove(4);

				foreach(var v in q)
				{
					Assert.IsTrue(v.Length <= 4);
				}
			}
		}


		[Test]
		public void StringRemove2Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where new List<string>() { "Argentina", "Mexico"}.Contains(c.Country)
						select c.Country.Remove(2,3);

				List<string> resultData = new List<string>() { "Artina", "Meo" };
				foreach(var v in q)
				{
					Assert.IsTrue(resultData.Contains(v));
				}
			}
		}


		[Test]
		public void StringReplaceTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country!=null
						select c.Country.Replace('A', 'X');

				foreach(var v in q)
				{
					Assert.IsFalse(v.ToLowerInvariant().Contains('a'));
				}
			}
		}
		

		[Test]
		public void StringSubstring1Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where new List<string>() { "Argentina", "Mexico" }.Contains(c.Country)
						select c.Country.Substring(4);

				List<string> resultData = new List<string>() { "ntina", "co" };
				foreach(var v in q)
				{
					Assert.IsTrue(resultData.Contains(v));
				}
			}
		}


		[Test]
		public void StringSubstring2Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where new List<string>() { "Argentina", "Mexico" }.Contains(c.Country)
						select c.Country.Substring(2, 2);

				List<string> resultData = new List<string>() { "ge", "xi" };
				foreach(var v in q)
				{
					Assert.IsTrue(resultData.Contains(v));
				}
			}
		}


		[Test]
		public void StringToUpperTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.CustomerId == "ALFKI"
						select c.Country.ToUpper();

				foreach(var v in q)
				{
					Assert.AreEqual("GERMANY", v);
				}
			}
		}


		[Test]
		public void StringToUpperTest2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				string value = "g";
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						where c.Country.ToUpper().StartsWith(value.ToUpper(), true, null)
						select c;

				foreach(var v in q)
				{
					Assert.IsTrue(v.Country.StartsWith("G"));
				}
			}
		}


		[Test]
		public void StringToUpperTest3WithGroupBy()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						group e by new { e.City, e.Country } into g
						select new { city = g.Key.City, CityUpper = g.Key.City.ToUpper(), CityLenth = g.Key.City.Length };

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(v.city.ToUpper(), v.CityUpper);
					Assert.AreEqual(v.CityUpper.Length, v.CityLenth);
					count++;
				}
				Assert.AreEqual(5, count);
			}
		}



		[Test]
		public void StringToLowerTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.CustomerId == "ALFKI"
						select c.Country.ToLower();

				foreach(var v in q)
				{
					Assert.AreEqual("germany", v);
				}
			}
		}


		[Test]
		public void StringLengthPropertyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country.Length > 6
						select c;

				int count = 0;
				foreach(var v in q)
				{
					Assert.IsTrue(v.Country.Length > 6);
					count++;
				}
				Assert.AreEqual(31, count);
			}
		}

		/// <summary>
		/// Tests a double string concatenation using '+'.
		/// </summary>
		[Test]
		public void StringConcatTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						select e.FirstName + " " + e.LastName;

				List<string> names = q.ToList();
				Assert.AreEqual(9, names.Count);
				foreach(string fullName in names)
				{
					Assert.IsTrue(fullName.Contains(' '));
					Assert.IsTrue(fullName.Length > 0);
				}
			}
		}
		

		/// <summary>
		/// Gets the order id as string. Test for 'ToString' call on a non-string field.
		/// </summary>
		[Test]
		public void StringToStringTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10254
						select o.OrderId.ToString();

				foreach(var v in q)
				{
					Assert.AreEqual("10254", v);
				}
			}
		}


		/// <summary>
		/// Tests string's Chars indexer and conversion of char to int under the hood (as the 'A' constant is passed as Int32 in the expression tree by .NET).
		/// </summary>
		[Test]
		public void StringCharIndexerTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.CompanyName[0] == 'A'
						select c;

				Assert.AreEqual(4, q.Count());
			}
		}
		

		[Test]
		public void CustomFullTextSearchOnOneFieldToFindEmployees()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter, new NorthwindFunctionMappings());
				var q = from e in metaData.Employee
						where NorthwindFunctions.FullTextSearch(e.Notes, "BA")
						select e;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Notes.Contains("BA"));
				}
				Assert.AreEqual(4, count);
			}
		}



		[Test]
		public void CustomFullTextSearchWithMultipleQueriesAndContains()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter, new NorthwindFunctionMappings());
				var q1 = from e in metaData.Employee
						 where NorthwindFunctions.FullTextSearch(e.Notes, "BA")
						 select e.EmployeeId;
				var q2 = from e in metaData.Employee
						 where NorthwindFunctions.FullTextSearch(e.Notes, "English")
						 select e.EmployeeId;
				var q3 = from e in metaData.Employee
						 where NorthwindFunctions.FullTextSearch(e.Notes, "BS")
						 select e.EmployeeId;

				var q4 = from e in metaData.Employee
						 where q1.Contains(e.EmployeeId) || q2.Contains(e.EmployeeId) || q3.Contains(e.EmployeeId)
						 select e;

				int count = 0;
				foreach(var v in q4)
				{
					count++;
					Assert.IsTrue(v.Notes.Contains("BA") || v.Notes.Contains("English") || v.Notes.Contains("BS"));
				}
				Assert.AreEqual(6, count);
			}
		}
		#endregion

		#region Custom method / property mapping tests
		/// <summary>
		/// Gets all order ids from chops and the order total by using a db function. This is a custom mapping and it utilizes the custom function mapping
		/// feature of Linq to LLBLGen Pro 
		/// </summary>
		[Test]
		public void CustomFunctionMappingCallTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter, new NorthwindFunctionMappings());
				var q = from o in metaData.Order
						where o.CustomerId == "CHOPS"
						select new { o.OrderId, OrderTotal = NorthwindFunctions.CalculateOrderTotal(o.OrderId, true) };
				int count = 0;
				decimal totalOfOrders = 0.0M;
				foreach(var v in q)
				{
					count++;
					totalOfOrders += v.OrderTotal;
				}
				Assert.AreEqual(8, count);
				Assert.AreEqual(21045.0M, totalOfOrders);
			}
		}
		#endregion

		#region DateTime method / property mapping tets
		[Test]
		public void DateTimeAddDaysTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10254
						select o.OrderDate.Value.AddDays(10);

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(new DateTime(2004, 5, 8), v.Date);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DateTimeAddHoursTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10254
						select o.OrderDate.Value.AddHours(10);

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(new DateTime(2004, 4, 28, 22, 30, 13, 700), v);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DateTimeAddMillisecondsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10254
						select o.OrderDate.Value.AddMilliseconds(10);

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(new DateTime(2004, 4, 28, 12, 30, 13, 710), v);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DateTimeAddMinutesTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10254
						select o.OrderDate.Value.AddMinutes(10);

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(new DateTime(2004, 4, 28, 12, 40, 13, 700), v);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DateTimeAddMonthsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10254
						select o.OrderDate.Value.AddMonths(10);

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(new DateTime(2005, 2, 28), v.Date);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DateTimeAddSecondsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10254
						select o.OrderDate.Value.AddSeconds(10);

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(new DateTime(2004, 4, 28, 12, 30, 23, 700), v);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DateTimeAddYearsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10254
						select o.OrderDate.Value.AddYears(10);

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(new DateTime(2014, 4, 28), v.Date);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void DateTimeCompareTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where DateTime.Compare(o.OrderDate.Value, new DateTime(2004, 4, 28, 12, 30, 13, 700))==0
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(10254, v.OrderId);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void DateTimeDatePropertyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate.Value.Date == new DateTime(2004, 4, 28)
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(10254, v.OrderId);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void DateTimeDatePropertyTest2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				DateTime toCompareWith = new DateTime(2004, 4, 28);
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate >= toCompareWith.Date
						&& o.OrderDate < toCompareWith.AddDays(1)
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(10254, v.OrderId);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void DateTimeDayPropertyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate.Value.Day == 4
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(4, v.OrderDate.Value.Day);
					count++;
				}
				Assert.AreEqual(26, count);
			}
		}

		[Test]
		public void DateTimeDayOfWeekPropertyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate.Value.DayOfWeek == DayOfWeek.Thursday
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(DayOfWeek.Thursday, v.OrderDate.Value.DayOfWeek);
					count++;
				}
				Assert.AreEqual(163, count);
			}
		}

		[Test]
		public void DateTimeHourPropertyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate.Value.Hour == 12
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(12, v.OrderDate.Value.Hour);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DateTimeMillisecondPropertyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate.Value.Millisecond == 700
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(700, v.OrderDate.Value.Millisecond);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}
			
		[Test]
		public void DateTimeMinutePropertyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate.Value.Minute == 30
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(30, v.OrderDate.Value.Minute);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DateTimeMonthPropertyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate.Value.Month == 4
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(4, v.OrderDate.Value.Month);
					count++;
				}
				Assert.AreEqual(105, count);
			}
		}

		[Test]
		public void DateTimeSecondPropertyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate.Value.Second == 13
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(13, v.OrderDate.Value.Second);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DateTimeYearPropertyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate.Value.Year == 2004
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(2004, v.OrderDate.Value.Year);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}
		#endregion

		#region Math method / property calls test
		[Test]
		public void MathPowTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from od in metaData.OrderDetail
						where od.OrderId==10254
						select new { 
								Power = Math.Pow(Convert.ToDouble(od.ProductId), 2.0),
								ProductId = od.ProductId};

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(Math.Pow(Convert.ToDouble(v.ProductId), 2.0), v.Power);
					count++;
				}
				Assert.AreEqual(9, count);
			}
		}
		#endregion

		#region Decimal method / property calls tests
		[Test]
		public void DecimalAddTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Add(o.Freight.Value, 1.0M) == 23.98M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(22.98M, v.Freight.Value);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DecimalCeilingTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Ceiling(o.Freight.Value) == 23M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(23M, Decimal.Ceiling(v.Freight.Value));
					count++;
				}
				Assert.AreEqual(8, count);
			}
		}

		[Test]
		public void DecimalCompareTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Compare(o.Freight.Value, 22.98M)==0
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(22.98M, v.Freight.Value);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DecimalDivideTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Divide(o.Freight.Value, 2.0M) == 11.49M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(22.98M, v.Freight.Value);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DecimalFloorTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Floor(o.Freight.Value) == 22M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(22.0M, Decimal.Floor(v.Freight.Value));
					count++;
				}
				Assert.AreEqual(9, count);
			}
		}

		[Test]
		public void DecimalMultiplyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Multiply(o.Freight.Value, 2.0M) == 45.96M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(22.98M, v.Freight.Value);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DecimalNegateTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Negate(o.Freight.Value) == -22.98M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(22.98M, v.Freight.Value);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DecimalRemainderTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Remainder(o.Freight.Value, 2.0M) == 0.98M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(0.98M, Decimal.Remainder(v.Freight.Value, 2.0M));
					count++;
				}
				Assert.AreEqual(8, count);
			}
		}

		[Test]
		public void DecimalRound1Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Round(o.Freight.Value) == 23M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(23M, Decimal.Round(v.Freight.Value));
					count++;
				}
				Assert.AreEqual(9, count);
			}
		}

		[Test]
		public void DecimalRound2Test()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Round(o.Freight.Value, 1) == 22.8M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(22.8M, Decimal.Round(v.Freight.Value, 1));
					count++;
				}
				Assert.AreEqual(2, count);
			}
		}

		[Test]
		public void DecimalSubstractTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Subtract(o.Freight.Value, 1.0M) == 21.98M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(22.98M, v.Freight.Value);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}

		[Test]
		public void DecimalTruncateTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where Decimal.Truncate(o.Freight.Value) == 22M
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(22M, Decimal.Truncate(v.Freight.Value));
					count++;
				}
				Assert.AreEqual(9, count);
			}
		}
		#endregion

		#region Bitshift operator calls test
		[Test]
		public void BitshiftLeftTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.EmployeeId<<8 > 286
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.IsTrue(v.EmployeeId << 8 > 286);
					count++;
				}
				Assert.AreEqual(709, count);
			}
		}

		[Test]
		public void BitshiftRightTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.EmployeeId >> 2 > 1
						select o;

				int count = 0;
				foreach(var v in q)
				{
					Assert.IsTrue(v.EmployeeId >> 2 > 1);
					count++;
				}
				Assert.AreEqual(132, count);
			}
		}

		#endregion

		#region In-memory method / property calls tests
		/// <summary>
		/// Gets the order data in anonymous type with constants and in memory function calls. Tests the in-memory function call facility in projections.
		/// </summary>
		[Test]
		public void InMemoryCallToUnmappedMethodInProjectionOfAnonymousType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.CustomerId=="CHOPS"
						select new
						{
							DaysInMonth = DateTime.DaysInMonth((int)o.OrderDate.Value.Year, (int)o.OrderDate.Value.Month),
							OrderId = o.OrderId,
							Constant = true
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Constant);
					Console.WriteLine("DaysInMonth: {0}, OrderId: {1}", v.DaysInMonth, v.OrderId);
				}
				Assert.AreEqual(8, count);
			}
		}


		/// <summary>
		/// Gets the list of days in month of orders of chops. This is a test which calls a local function in the projection and the resultset is a value list.
		/// </summary>
		[Test]
		public void InMemoryCallToUnmappedMethodInValueListProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.CustomerId == "CHOPS"
						select DateTime.DaysInMonth((int)o.OrderDate.Value.Year, (int)o.OrderDate.Value.Month);

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue((v == 30) || (v == 31));
				}
				Assert.AreEqual(8, count);
			}
		}


		[Test]
		public void CustomMappedVersionOfDaysInMonthUsageInWhereClauseTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter, new NorthwindFunctionMappings());
				var q = from o in metaData.Order
						where DateTime.DaysInMonth(o.OrderDate.Value.Year, o.OrderDate.Value.Month) == 30
						select o;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.OrderDate.HasValue && (DateTime.DaysInMonth(v.OrderDate.Value.Year, v.OrderDate.Value.Month) == 30));
				}
				Assert.AreEqual(252, count);
			}
		}


		[Test]
		public void InMemoryMethodCallWithNestedSetResultAsParameter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var ordersFromGermany = from o in metaData.Order
										where o.Customer.Country=="Germany"
										select o.OrderId;

				List<int> orderIds = ordersFromGermany.ToList();
				Assert.AreEqual(121, orderIds.Count);

				var q = from o in metaData.Order
						select new	{
								o.OrderId,
								Value = InMemoryMethodCallsForTests.IsUsefulEntity(o.Customer)
							};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if(v.Value)
					{
						Assert.IsTrue(orderIds.Contains(v.OrderId));
					}
					else
					{
						Assert.IsFalse(orderIds.Contains(v.OrderId));
					}
				}
			}
		}
		#endregion

		[TestFixtureSetUp]
		public void SetupTests()
		{
		}
	}


	public class InMemoryMethodCallsForTests
	{
		public static bool IsUsefulEntity(CustomerEntity customerEntity)
		{
			if(customerEntity != null)
			{
				return customerEntity.Country == "Germany";
			}
			else
			{
				return false;
			}
		}
	}

}
