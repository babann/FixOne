using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FixOne.Entities.Logging;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;

namespace FixOne.Loggers.LogToTextFile
{
	public class Logger : Common.Interfaces.IEngineLogger
	{
		private const string NAME = "Log to Text File";
		private const string CONFIG_SECTION_NAME = "LogToTextFile";

		private Settings _settings;

		private string _engineLogfileName;
		private DateTime _recentLogCreateDate;

		public Logger()
		{
			_settings = new Settings(CONFIG_SECTION_NAME);
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

			File.AppendAllLines(getFileName(), new string[] { string.Format("{0} : {1} : {2}", message.EventTime, message.Level.ToString(), message.Message) });
		}

		public void InitSettings(XDocument settingsDocument)
		{
			_settings.Load(settingsDocument);

			List<string> messagesToLog = new List<string>();
			messagesToLog.Add(string.Format("{0} : Logger '{1}' initialized with level set to '{2}'.", DateTime.Now, Name, _settings.LoggingLevel.ToString()));

			messagesToLog.AddRange (validatePath ());

			if (messagesToLog.Any())
				File.AppendAllLines(getFileName(), messagesToLog);
		}

		public IEnumerable<XElement> GetSettings()
		{
			//TODO:
			throw new NotImplementedException();
		}

		public void ApplySettings(IEnumerable<XElement> settings)
		{
			List<string> messagesToLog = new List<string> ();
			try
			{
				_settings.MergeSettings (settings);
				messagesToLog.Add(string.Format("Logger '{0}' successfully applied new configuration. New level set to '{1}'.", Name, _settings.LoggingLevel.ToString()));
				messagesToLog.AddRange( validatePath());
			}
			catch(Exception exc) {
				messagesToLog.Add(string.Format("Logger '{0}' failed to apply new configuration. Error: {1}.", Name, exc.Message));
			}

			if (messagesToLog.Any())
				File.AppendAllLines(getFileName(), messagesToLog);
		}

		#endregion

		private string getFileName()
		{
			if (DateTime.Today != _recentLogCreateDate)
			{
				_recentLogCreateDate = DateTime.Today;
				_engineLogfileName = Path.Combine(_settings.LogFilesPath, string.Format("engine_{0}.log", _recentLogCreateDate.ToString("yyyyMMdd")));
			}

			if (!Directory.Exists(_settings.LogFilesPath))
				Directory.CreateDirectory(_settings.LogFilesPath);

			if (!File.Exists(_engineLogfileName))
			{
				var sw = File.CreateText(_engineLogfileName);
				sw.Close();
			}

			return _engineLogfileName;
		}

		private List<string> validatePath()
		{
			List<string> messagesToLog = new List<string>();

			if (string.IsNullOrEmpty(_settings.LogFilesPath))
			{
				_settings.LogFilesPath = Common.PathHelper.MapPath ("..\\Logs\\");
				messagesToLog.Add(string.Format("{0} : Path attribute is empty in configuration. Logs path set to '{1}'.", DateTime.Now, _settings.LogFilesPath));
			}
			else
			{
				_settings.LogFilesPath = Common.PathHelper.MapPath (_settings.LogFilesPath);
				messagesToLog.Add(string.Format("{0} : Mapped logs path is '{1}'.", DateTime.Now.ToString(), _settings.LogFilesPath));
			}

			return messagesToLog;
		}
	}
}
