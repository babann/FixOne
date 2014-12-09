using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Engine.Settings.ValueParsers
{
	public class BoolParser : IValueParser
	{
		#region IValueParser Members

		public Type ResultType
		{
			get { return typeof(bool); }
		}

		public object Parse(string value)
		{
			bool val;
			return bool.TryParse(value, out val) ? val : default(bool);
		}

		public string Encode(object value)
		{
			return value == null ? null : value.ToString();
		}

		public bool AreEquals(object value1, object value2)
		{
			return value1 is bool && value2 is bool && (bool)value1 == (bool)value2;
		}

		#endregion
	}
}
