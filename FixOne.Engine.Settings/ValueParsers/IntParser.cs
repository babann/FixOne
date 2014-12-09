using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Engine.Settings.ValueParsers
{
	public class IntParser : IValueParser
	{
		#region IValueParser Members

		public Type ResultType
		{
			get { return typeof(int); }
		}

		public object Parse(string value)
		{
			int val;
			return int.TryParse(value, out val) ? (object)val : default(int);
		}

		public string Encode(object value)
		{
			return value == null ? null : value.ToString();
		}

		public bool AreEquals(object value1, object value2)
		{
			return value1 is int && value2 is int && (int)value1 == (int)value2;
		}

		#endregion
	}
}
