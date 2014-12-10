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
		#region Constants

		/// <summary>
		/// The default size of the buffer to dequeu messages before flush
		/// </summary>
		private const int DEFAULT_BUFFER_SIZE = 1000; //TODO: make configurable

		#endregion

		#region Private Fields

		/// <summary>
		/// Messages to be written into logs
		/// </summary>
		ConcurrentQueue<LogMessage> messages = new ConcurrentQueue<LogMessage> ();

		/// <summary>
		/// The log writer thread.
		/// </summary>
		Thread logWriterThread;

		/// <summary>
		/// Stop indicator for thread.
		/// </summary>
		bool isExiting = false;

		/// <summary>
		/// The started flag.
		/// </summary>
		bool isStarted = false;

		/// <summary>
		/// The isStaring flag.
		/// </summary>
		bool isStarting = false;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="FixOne.Engine.Managers.EngineLogManager"/> class.
		/// </summary>
		private EngineLogManager ()
			: base (SettingsManager.Instance.CurrentSettings.LoggersPath)
		{
			
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Start this working thead if loggers manager.
		/// </summary>
		public void Start ()
		{
			if (isStarted || isStarting)
				return;

			isStarting = true;

			List<LogMessage> initializationErrors = new List<LogMessage> ();
			var modules = EnabledModules.ToArray();

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

			isStarted = true;

			isExiting = false;
			logWriterThread = new Thread (threadMethod);
			logWriterThread.IsBackground = true;
			logWriterThread.Name = "LOGWRT";
			logWriterThread.Start ();

			isStarting = false;
		}

		public void Stop ()
		{
			isExiting = true;
			
			//TODO: add flush functionality to guaranty all the queued messages are written to the log

			isStarted = false;
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

		#region Implementation

		/// <summary>
		/// Worker method running in bacground.
		/// </summary>
		private void threadMethod()
		{
			while (!isExiting) {
				if (EnabledModules.Any ())
					doBufferReadWrite (DEFAULT_BUFFER_SIZE);

				Thread.Sleep (1);
			}
		}

		/// <summary>
		/// Proess one iteration of fill buffer - flush data operation
		/// </summary>
		/// <param name="bufferSize">Buffer size.</param>
		private void doBufferReadWrite(int bufferSize)
		{
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

		#endregion

	}
}
