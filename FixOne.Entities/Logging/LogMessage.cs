using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Entities.Logging
{
	public class LogMessage
	{
		public DateTime EventTime
		{
			get;
			private set;
		}

		public LogMessageLevel Level
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public LogMessage(LogMessageLevel level, string message)
		{
			EventTime = DateTime.Now;
			Level = level;
			Message = message;
		}

		internal void PrependMessage(string prepend)
		{
			Message = prepend + Message;
		}
	}
}
