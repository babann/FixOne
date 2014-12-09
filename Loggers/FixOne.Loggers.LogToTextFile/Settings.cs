using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FixOne.Engine.Settings;

namespace FixOne.Loggers.LogToTextFile
{
	internal class Settings : Engine.Settings.LoggerSettings
	{
		[XSetting("Settings", "path", null)]
		public string LogFilesPath
		{
			get;
			set;
		}

		internal Settings(string sectionName)
			: base(sectionName)
		{
		}
	}
}
