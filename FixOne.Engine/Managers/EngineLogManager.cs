using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using FixOne.Entities;
using FixOne.Entities.Logging;

namespace FixOne.Engine.Managers
{
	public class EngineLogManager : Common.GenericManager<Common.Interfaces.IEngineLogger, EngineLogManager>
	{

		#region Private Fields

		ConcurrentQueue<LogMessage> messages = new ConcurrentQueue<LogMessage> ();
		Thread logWriterThread;
		bool exit = false;
		bool started = false;

		#endregion

		private EngineLogManager ()
			: base (SettingsManager.Instance.CurrentSettings.LoggersPath)
		{
			
		}

		#region Public Methods

		public void Start ()
		{
			if (started)
				return;

			List<LogMessage> initializationErrors = new List<LogMessage> ();
			var modules = AvailableModules.ToList ();

			foreach (var module in modules) {
				try {
					module.InitSettings (SettingsManager.Instance.CurrentSettings.CurrentSettingsDocument);
				} catch (Exception exc) {
					initializationErrors.Add (new LogMessage (LogMessageLevel.Error, string.Format ("Failed to initialize logger '{0}'. Error: {1}. Module will be disabled.", module.Name, exc.Message)));
					DisableModules (module.Name);
				}
			}

			if (initializationErrors.Any ())
				initializationErrors.ForEach (msg => messages.Enqueue (msg));

			started = true;

			exit = false;
			logWriterThread = new Thread (() => {
				while (!exit) {
					if (EnabledModules != null && EnabledModules.Any ()) {
						int bufferSize = 1000;
						List<LogMessage> buffer = new List<LogMessage> (bufferSize);
						int cnt = 0;
						LogMessage msg;
						lock (writeLocker) {
							while (messages.TryDequeue (out msg) && cnt++ < bufferSize)
								buffer.Add (msg);
						}
						
						if (buffer.Any ()) {
							foreach (var logger in EnabledModules) {
								for (int ii = 0; ii < buffer.Count; ii++) {
									try {
										logger.LogMessage (buffer [ii]);
									} catch (Exception exc) {
										DisableModules (logger.Name);
										messages.Enqueue (new LogMessage (LogMessageLevel.Error, string.Format ("Failed to log message with logger '{0}'. Error: {1}. Module will be disabled.", logger.Name, exc.Message)));
									}
								}
							}
						}
					}

					Thread.Sleep (1);
				}
			});
			logWriterThread.IsBackground = true;
			logWriterThread.Name = "LOGWRT";
			logWriterThread.Start ();
		}

		public void Stop ()
		{
			exit = true;
			
			//TODO: add flush functionality to guaranty all the queued messages are written to the log

			started = false;
		}

		internal void LogEntry (LogMessage message)
		{
			messages.Enqueue (message);
		}

		public void Debug (string message)
		{
			messages.Enqueue (new LogMessage (LogMessageLevel.Debug, message));
		}

		public void Debug (string format, params object[] values)
		{
			messages.Enqueue (new LogMessage (LogMessageLevel.Debug, string.Format (format, values)));
		}

		public void Info (string message)
		{
			messages.Enqueue (new LogMessage (LogMessageLevel.Info, message));
		}

		public void Info (string format, params object[] values)
		{
			messages.Enqueue (new LogMessage (LogMessageLevel.Info, string.Format (format, values)));
		}

		public void InfoIf (bool condition, string message)
		{
			if (condition)
				messages.Enqueue (new LogMessage (LogMessageLevel.Info, message));
		}

		public void InfoIf (bool condition, string format, params object[] values)
		{
			if (condition)
				messages.Enqueue (new LogMessage (LogMessageLevel.Info, string.Format (format, values)));
		}

		public void Warning (string message)
		{
			messages.Enqueue (new LogMessage (LogMessageLevel.Warning, message));
		}

		public void Warning (string format, params object[] values)
		{
			Warning (string.Format (format, values));
		}

		public void Error (string message)
		{
			messages.Enqueue (new LogMessage (LogMessageLevel.Error, message));
		}

		public void Error (string format, params object[] values)
		{
			Error (string.Format (format, values));
		}

		public void Error (string message, Exception exc)
		{
			StringBuilder msg = new StringBuilder (message);
			msg.Append (Environment.NewLine);
			msg.Append (exc.Message);
			msg.Append (exc.StackTrace);
			if (exc.InnerException != null) {
				msg.Append (exc.InnerException.Message);
				msg.Append (exc.InnerException.StackTrace);
			}
			messages.Enqueue (new LogMessage (LogMessageLevel.Error, msg.ToString ()));
		}

		public void Error (string format, Exception exc, params object[] values)
		{
			Error (string.Format (format, values), exc);
		}

		#endregion

	}
}
