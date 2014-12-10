using System;
using Custom.Collections.KVV;

namespace Custom.Collections.KVV.Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("KeyValueValuePair collections test.");

			TestObjects ();

			Console.ReadLine ();

		}

		private static void TestObjects()
		{
			Console.WriteLine ("Testing base objects.");

			KeyValueValuePair<int, int, int> pair1 = new KeyValueValuePair<int, int, int> (1, 2, 3);
			KeyValueValuePair<int, int, int> pair2 = new KeyValueValuePair<int, int, int> (1, 2, 3);
			KeyValueValuePair<int, int, int> pair3 = new KeyValueValuePair<int, int, int> (2, 2, 3);
			KeyValueValuePair<int, int, int> pair4 = new KeyValueValuePair<int, int, int> (1, 3, 3);
			KeyValueValuePair<int, int, int> pair5 = new KeyValueValuePair<int, int, int> (1, 2, 4);

			KeyValueValuePair<string, int, int> pair6 = new KeyValueValuePair<string, int, int> ("test", 2, 3);

			KeyValueValuePair<string, string, string> pair7 = new KeyValueValuePair<string, string, string> ("test", "test1", "test2");
			KeyValueValuePair<string, string, string> pair8 = new KeyValueValuePair<string, string, string> ("test", "test1", "test2");
			KeyValueValuePair<string, string, string> pair9 = new KeyValueValuePair<string, string, string> ("mykey", "test1", "test2");

			Console.WriteLine ("Equals: {0}, pair1: {1}; pair2: {2}", pair1.Equals (pair2), pair1.ToString (), pair2.ToString ());
			Console.WriteLine ("Equals: {0}, pair1: {1}; pair2: {2}", pair2.Equals (pair3), pair2.ToString (), pair3.ToString ());
			Console.WriteLine ("Equals: {0}, pair1: {1}; pair2: {2}", pair1.Equals (pair4), pair1.ToString (), pair4.ToString ());
			Console.WriteLine ("Equals: {0}, pair1: {1}; pair2: {2}", pair1.Equals (pair5), pair1.ToString (), pair5.ToString ());
			Console.WriteLine ("Equals: {0}, pair1: {1}; pair2: {2}", pair1.Equals (pair6), pair1.ToString (), pair6.ToString ());
			Console.WriteLine ("Equals: {0}, pair1: {1}; pair2: {2}", pair7.Equals (pair8), pair7.ToString (), pair8.ToString ());
			Console.WriteLine ("Equals: {0}, pair1: {1}; pair2: {2}", pair7.Equals (pair9), pair7.ToString (), pair9.ToString ());

			Console.WriteLine ("Testing base objects complete.");

		}
	}
}
