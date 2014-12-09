using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace FixOne.Engine.Settings.ValueParsers
{
	public class ArrayOfStringsParser : IValueParser
	{
		#region IValueParser Members

		public Type ResultType
		{
			get { return typeof(string[]); }
		}

		public object Parse(string values)
		{
			if (string.IsNullOrEmpty(values))
				return null;

			var items = values.Split(new string [] {"<item>", "</item>"}, StringSplitOptions.RemoveEmptyEntries);
			return items;
		}

		public string Encode(object value)
		{
			if (value == null)
				return null;

			if(value is string [])
				return string.Join("", string.Format("<item>{0}</item>", value as string[]));

			return null;
		}

		public bool AreEquals(object value1, object value2)
		{
			return false;
			//return value1 is byte[] && value2 is byte[] && value1.Equals(value2);
		}

		#endregion
	}
}
