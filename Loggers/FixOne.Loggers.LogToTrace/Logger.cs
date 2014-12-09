using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using FixOne.Entities.Logging;
using System.Xml.Linq;

namespace FixOne.Loggers.LogToTrace
{
	public class Logger : Common.Interfaces.IEngineLogger
	{
		private const string NAME = "Log to System Trace";
		private const string CONFIG_SECTION_NAME = "LogToTrace";

		private Engine.Settings.LoggerSettings _settings;

		public Logger()
		{
			_settings = new Engine.Settings.LoggerSettings(CONFIG_SECTION_NAME);
		}

		#region IEngineLogger Members

		public string Name
		{
			get
			{
				return NAME;
			}
		}

		public bool Enabled
		{
			get;
			set;
		}

		public void LogMessage(LogMessage message)
		{
			if (message == null)
				return;

			if (message.Level < _settings.LoggingLevel)
				return;

			try
			{
				Trace.WriteLine(string.Format("{0} : {1} : {2}", message.EventTime, message.Level.ToString(), message.Message));
			}
			catch(Exception exc) 
			{
				int i = 1 + 1;
			}
		}

		public void InitSettings(XDocument settingsDocument)
		{
			_settings.Load(settingsDocument);
			Trace.WriteLine(string.Format("Logger '{0}' initialized with level set to '{1}'.", Name, _settings.LoggingLevel.ToString()));
		}

		public IEnumerable<XElement> GetSettings()
		{
			//TODO:
			throw new NotImplementedException();
		}

		public void ApplySettings(IEnumerable<XElement> settings)
		{
			try
			{
				_settings.MergeSettings (settings);
				Trace.WriteLine(string.Format("Logger '{0}' successfully applied new configuration. New level set to '{1}'.", Name, _settings.LoggingLevel.ToString()));
			}
			catch(Exception exc) {
				Trace.WriteLine(string.Format("Logger '{0}' failed to apply new configuration. Error: {1}.", Name, exc.Message));
			}
		}

		#endregion
	}
}
