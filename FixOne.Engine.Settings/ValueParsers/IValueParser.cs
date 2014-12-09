using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Engine.Settings.ValueParsers
{
	public interface IValueParser
	{
		Type ResultType { get; }

		object Parse(string value);

		string Encode(object value);

		bool AreEquals(object value1, object value2);
	}
}
