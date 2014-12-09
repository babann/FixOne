using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
namespace Test.Entities
{
	[TestClass]
	public class TestParsers
	{
		public TestParsers()
		{
		}

		private TestContext testContextInstance;

		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		[TestMethod]
		public void TestGenericParser()
		{
			var string1 = "8=FIX.4.49=7535=A49=IntGBIorders56=Broker34=152=20100310-12:50:1698=0108=30141=Y10=072";
			var string2 = "8=FIX.4.49=5735=049=IntGBIorders56=Broker34=252=20100310-12:50:4510=244"
					+"8=FIX.4.49=5735=049=IntGBIorders56=Broker34=352=20100310-12:51:1510=243";

			var headers = FixOne.Parsers.Generic.FixMessageHeaderParser.ParseHeaders(System.Text.ASCIIEncoding.ASCII.GetBytes(string1), null);
			Assert.AreEqual(headers.Count(), 1);

			headers = FixOne.Parsers.Generic.FixMessageHeaderParser.ParseHeaders(System.Text.ASCIIEncoding.ASCII.GetBytes(string2), null);
			Assert.AreEqual(headers.Count(), 2);
		}

	}
}
