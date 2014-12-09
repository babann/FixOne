using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FixOne.Entities.Logging;

namespace FixOne.Engine.Settings
{
	public class LoggerSettings : XDocSettings
	{
		[XSetting("Settings", "level", typeof(ValueParsers.LogMessageLevelParser), LogMessageLevel.Debug)]
		public LogMessageLevel LoggingLevel
		{
			get;
			set;
		}

		public LoggerSettings(string sectionName)
			: base(sectionName)
		{
		}
	}
}
