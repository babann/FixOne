using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Entities
{
	internal class SessionIdentifier
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
