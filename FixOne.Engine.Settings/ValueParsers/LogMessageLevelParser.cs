using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FixOne.Entities.Logging;
using System.Diagnostics.Contracts;

namespace FixOne.Engine.Settings.ValueParsers
{
	class LogMessageLevelParser : IValueParser
	{
		#region IValueParser Members

		public Type ResultType
		{
			get { return typeof(LogMessageLevel); }
		}

		public object Parse(string level)
		{
			if (string.IsNullOrEmpty(level))
				return null;

			LogMessageLevel value;

			if (Enum.TryParse<LogMessageLevel>(level, out value))
				return value;

			return LogMessageLevel.Debug;
		}

		public string Encode(object value)
		{
			Contract.Requires(value != null);
			Contract.Requires(value.GetType() == typeof(byte[]));

			return value.ToString();
		}

		public bool AreEquals(object value1, object value2)
		{
			return value1 is LogMessageLevel && value2 is LogMessageLevel && value1.Equals(value2);
		}

		#endregion
	}
}
