using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashCodeCalculator
{
	class Program
	{
		static Dictionary<SessionIdentifier, string> storage = new Dictionary<SessionIdentifier, string>();

		static void Main(string[] args)
		{
			SessionIdentifier ident1 = new SessionIdentifier("sender1", "target1");
			SessionIdentifier ident2 = new SessionIdentifier("target1", "sender1");
			SessionIdentifier ident3 = new SessionIdentifier("sender2", "target1");

			Console.WriteLine("ident1 equals ident2 = {0}", ident1.Equals(ident2));
			Console.WriteLine("ident2 equals ident1 = {0}", ident2.Equals(ident2));
			Console.WriteLine("ident1 equals ident3 = {0}", ident1.Equals(ident3));

			Console.WriteLine("ident1 hash {0}", ident1.GetHashCode());
			Console.WriteLine("ident2 hash {0}", ident2.GetHashCode());
			Console.WriteLine("ident3 hash {0}", ident3.GetHashCode());

			storage.Add(ident1, "1");
			if (storage.ContainsKey(ident2))
				Console.WriteLine("ident2 found. value is {0}", storage[ident2]);
			else
				Console.WriteLine("ident2 not found :(");

			if (storage.ContainsKey(ident3))
				Console.WriteLine("ident3 found. value is {0}", storage[ident3]);
			else
				Console.WriteLine("ident3 not found :)");

			Console.ReadLine();
		}

		private class SessionIdentifier
		{
			public string Side1
			{
				get;
				private set;
			}

			public string Side2
			{
				get;
				private set;
			}

			public SessionIdentifier(string side1, string side2)
			{
				Side1 = side1;
				Side2 = side2;
			}

			public override int GetHashCode()
			{
				return Side1.GetHashCode() ^ Side2.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				if (obj is SessionIdentifier)
				{
					var ident = obj as SessionIdentifier;

					return (Side1 == ident.Side1 && Side2 == ident.Side2) || (Side1 == ident.Side2 && Side2 == ident.Side1);
				}

				return false;
			}
		}
	}
}
