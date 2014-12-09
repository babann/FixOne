using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Conveyor
{
	class MainClass
	{
		private static Conveyor<int[], string> c = new Conveyor<int[], string> ();
		private const int MAX_DELAY = 100;
		private const bool VERBOSE = true;

		public static void Main (string[] args)
		{
			Console.WriteLine ("VERBOSE mode {0}", (VERBOSE ? "On" : "Off"));

			singleThreadTest ();
			Console.WriteLine ("Please hit [Enter] to run next test");
			Console.ReadLine ();
			multiThreadTest ();
			Console.WriteLine ("Please hit [Enter] to run next test");
			Console.ReadLine ();
			multiThreadRealTest ();
		}

		private static void singleThreadTest()
		{
			int idx = 0;
			Console.WriteLine ("Single Thread Test");

			c.Clear ();

			c.Push (new int[]{ idx++ });
			c.Push (new int[]{ idx++ });
			c.Push (new int[]{ idx++ });

			c.Process ((d) => {
				return d[0].ToString ();
			});

			c.Process ((d) => {
				return d[0].ToString ();
			});

			Console.WriteLine (c.Pop (MAX_DELAY));
			Console.WriteLine (c.Pop (MAX_DELAY));

			c.Process ((d) => {
				return d[0].ToString ();
			});

			Console.WriteLine (c.Pop (MAX_DELAY));


			Console.WriteLine ("Now the value should be empty because there were only 3 items conveyed. Value: '{0}'", c.Pop (MAX_DELAY));
			Console.WriteLine ("Single Thread Test Complete");
		}

		private static void multiThreadTest()
		{
			int MAX_OBJECTS = 100;
			int PROCESSING_THREADS = 4;

			int idx = 0;
			Random random = new Random ();
			List<string> original = new List<string> ();
			List<string> result = new List<string> ();
			Thread[] threads = new Thread[PROCESSING_THREADS];
			CountdownEvent cdevent = new CountdownEvent (PROCESSING_THREADS);

			Console.WriteLine ("Multi Thread Test");
			Console.WriteLine ("Going to push {0} items and process/pop them in {1} threads.", MAX_OBJECTS, PROCESSING_THREADS);
			c.Clear ();

			while (idx < MAX_OBJECTS) {
				original.Add (idx.ToString ());
				c.Push (new int[]{ idx++ });
			}

			Func<bool> func = () => {
				while (c.TotalItemsToProcess > 0) {
					c.Process ((d) => {
						Thread.Sleep(random.Next(MAX_DELAY));
						return d [0].ToString ();
					});

					lock(c)
					{
						var val = c.Pop(MAX_DELAY);

						if(!VERBOSE)
							Console.WriteLine ("[{0}] {1}", Thread.CurrentThread.Name, val);
						if(val != default(string))
							result.Add(val);
					}
				}
				cdevent.Signal();
				return true;
			};

			for (int ii = 0; ii < PROCESSING_THREADS; ii++) {
				threads [ii] = new Thread (() => {
					func ();
				});
				threads [ii].Name = "T" + (ii + 1).ToString ();
				threads [ii].IsBackground = true;
				threads [ii].Start ();
			}

			cdevent.Wait ();

			Console.WriteLine ("Multi Thread Test Complete. Validating.");

			bool valid = true;
			if (original.Count != result.Count) {
				valid = false;
				Console.WriteLine ("Validation failed: count is invalid.");
			}
			else {
				for (int ii = 0; ii < original.Count; ii++)
					if (original [ii] != result [ii]) {
						Console.WriteLine ("Validation failed: sequence does not match.");
						valid = false;
						break;
					}
			}

			if(valid)
				Console.WriteLine ("Validation succeed. Count and order of pushed and poped items is equal.");

		}

		private static void multiThreadRealTest()
		{
			int MAX_OBJECTS = 10000;
			int PROCESSING_THREADS = 8;
			int READING_THREADS = 4;
			int WRINTING_THREADS = 4;

			int idx = 0;
			Random random = new Random ();
			Stopwatch sw = new Stopwatch ();
			List<string> original = new List<string> ();
			List<string> result = new List<string> ();
			Thread[] proessingThreads = new Thread[PROCESSING_THREADS];
			Thread[] readingThreads = new Thread[READING_THREADS];
			Thread[] wrintingThreads = new Thread[WRINTING_THREADS];

			CountdownEvent cdevent = new CountdownEvent (READING_THREADS + PROCESSING_THREADS + WRINTING_THREADS);

			Console.WriteLine ("Real Multi Thread Test");
			Console.WriteLine ("Going to push {0} items in {1} thread(s), process them in {2} thread(s) and then pop out in {3} thread(s).",
				MAX_OBJECTS, WRINTING_THREADS, PROCESSING_THREADS, READING_THREADS);

			Console.WriteLine ("It may take up to {0} sec due to random delays added for simulation.",
				MAX_DELAY * MAX_OBJECTS * 3 / 1000);
			c.Clear ();

			Func<bool> writingFunc = () => {
				Console.WriteLine (string.Format("Starting [{0}] thread to PUSH the data.", Thread.CurrentThread.Name));
				while (idx < MAX_OBJECTS) {
					lock(random)
					{
						original.Add (idx.ToString ());
						c.Push (new int[]{ idx++ });
						if(!VERBOSE)
							Console.WriteLine ("[{0}] {1}", Thread.CurrentThread.Name, idx - 1);
					}
					Thread.Sleep(random.Next(MAX_DELAY));
				}
				Console.WriteLine ("Thread [{0}] complete.", Thread.CurrentThread.Name);
				cdevent.Signal();
				return true;
			};

			Func<bool> processfunc = () => {
				while(idx == 0)
					Thread.Sleep(1); //not yet started to populate

				Console.WriteLine (string.Format("Starting [{0}] thread to PROCESS the data.", Thread.CurrentThread.Name));
				while(c.TotalItemsToProcess > 0 || idx < MAX_OBJECTS) {
					c.Process ((d) => {
						if(!VERBOSE)
							Console.WriteLine ("[{0}] {1}", Thread.CurrentThread.Name, d[0]);
						Thread.Sleep(random.Next(MAX_DELAY));
						return d [0].ToString ();
					});
				}
				cdevent.Signal();
				Console.WriteLine ("Thread [{0}] complete.", Thread.CurrentThread.Name);
				return true;
			};

			Func<bool> readFunc = () => {
				while(idx == 0)
					Thread.Sleep(1); //not yet started to populate

				Console.WriteLine ("Starting [{0}] thread to POP the data.", Thread.CurrentThread.Name);
				while(c.TotalItemsToProcess > 0 || idx < MAX_OBJECTS ) {
					lock(c)
					{
						var val = c.Pop(MAX_DELAY);

						if(!VERBOSE)
							Console.WriteLine ("[{0}] {1}", Thread.CurrentThread.Name, val);
						if(val != default(string))
							result.Add(val);
					}
				}
				Console.WriteLine ("Thread [{0}] complete.", Thread.CurrentThread.Name);
				cdevent.Signal();
				return true;
			};

			sw.Start ();
			for (int ii = 0; ii < WRINTING_THREADS; ii++) {
				wrintingThreads [ii] = new Thread (() => {
					writingFunc ();
				});
				wrintingThreads [ii].Name = "W" + (ii + 1).ToString ();
				wrintingThreads [ii].IsBackground = true;
				wrintingThreads [ii].Start ();
			}

			for (int ii = 0; ii < PROCESSING_THREADS; ii++) {
				proessingThreads [ii] = new Thread (() => {
					processfunc ();
				});
				proessingThreads [ii].Name = "P" + (ii + 1).ToString ();
				proessingThreads [ii].IsBackground = true;
				proessingThreads [ii].Start ();
			}

			for (int ii = 0; ii < READING_THREADS; ii++) {
				readingThreads [ii] = new Thread (() => {
					readFunc ();
				});
				readingThreads [ii].Name = "R" + (ii + 1).ToString ();
				readingThreads [ii].IsBackground = true;
				readingThreads [ii].Start ();
			}

			cdevent.Wait ();
			sw.Stop ();

			Console.WriteLine ("Real Multi Thread Test Complete in {0} sec. Validating.", sw.ElapsedMilliseconds / 1000);

			bool valid = true;
			if (original.Count != result.Count) {
				valid = false;
				Console.WriteLine ("Validation failed: count is invalid.");
			}
			else {
				for (int ii = 0; ii < original.Count; ii++)
					if (original [ii] != result [ii]) {
						Console.WriteLine ("Validation failed: order does not match.");
						valid = false;
						break;
					}
			}

			if(valid)
				Console.WriteLine ("Validation succeed. Count and order of pushed and poped items is equal.");

		}
	}
}
