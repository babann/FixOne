using NUnit.Framework;
using System;

namespace Custom.Collections.KVV.UnitTest
{
	[TestFixture ()]
	public class Test
	{
		[Test ()]
		public void TestCase ()
		{
			KeyValueValuePair<int, int, int> pair1 = new KeyValueValuePair<int, int, int> (1, 2, 3);
			KeyValueValuePair<int, int, int> pair2 = new KeyValueValuePair<int, int, int> (1, 2, 3);
			KeyValueValuePair<int, int, int> pair3 = new KeyValueValuePair<int, int, int> (2, 2, 3);
			KeyValueValuePair<int, int, int> pair4 = new KeyValueValuePair<int, int, int> (1, 3, 3);
			KeyValueValuePair<int, int, int> pair5 = new KeyValueValuePair<int, int, int> (1, 2, 4);

			KeyValueValuePair<string, int, int> pair6 = new KeyValueValuePair<string, int, int> ("test", 2, 3);

			KeyValueValuePair<string, string, string> pair7 = new KeyValueValuePair<string, string, string> ("test", "test1", "test2");
			KeyValueValuePair<string, string, string> pair8 = new KeyValueValuePair<string, string, string> ("test", "test1", "test2");
			KeyValueValuePair<string, string, string> pair9 = new KeyValueValuePair<string, string, string> ("mykey", "test1", "test2");

			Assert.AreEqual (pair1, pair2);
			Assert.AreNotEqual (pair2, pair3);
			Assert.AreNotEqual (pair1, pair4);
			Assert.AreNotEqual (pair1, pair5);
			Assert.AreNotEqual (pair1, pair6);
			Assert.AreEqual (pair7, pair8);
			Assert.AreNotEqual (pair7, pair9);
		}
	}
}

