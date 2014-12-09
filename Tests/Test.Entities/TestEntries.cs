using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Test.Entities
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class TestEntries
	{
		public TestEntries()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
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

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void TestFixSessionInfoDeserialization()
		{
			var xml1 = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>"
						+ "<Sessions>"
							+ "<Session Role=\"Acceptor\" HeartBeat=\"30\">"
								+ "<SenderCompId>Server</SenderCompId>"
								+ "<TargetCompId>Client</TargetCompId>"
							+ "</Session>"
						+ "</Sessions>";
			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>"
						+ "<Sessions>"
							+ "<Session Role=\"Initiator\" HeartBeat=\"30\" Acceptor=\"10.17.1.217\" Port=\"10001\">"
								+ "<SenderCompId>ServerOffice</SenderCompId>"
								+ "<TargetCompId>Client</TargetCompId>"
							+ "</Session>"
							+ "<Session Role=\"Initiator\" HeartBeat=\"30\" Acceptor=\"192.168.1.4\" Port=\"10001\">"
								+ "<SenderCompId>ServerHome</SenderCompId>"
								+ "<TargetCompId>Client</TargetCompId>"
							+ "</Session>"
						+ "</Sessions>";

			var path = System.IO.Path.GetTempFileName();

			System.IO.File.WriteAllText(path, xml1);
			var sessions = FixOne.Entities.FixSessionInfo.GetFromXml(path);
			Assert.AreEqual(sessions.Count(), 1);

			System.IO.File.WriteAllText(path, xml2);
			sessions = FixOne.Entities.FixSessionInfo.GetFromXml(path);
			Assert.AreEqual(sessions.Count(), 2);

			System.IO.File.Delete(path);
		}
	}
}
