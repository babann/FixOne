using System;
using System.Collections.Generic;
using MonoMac.Foundation;
using System.Diagnostics;
using System.Threading;

namespace FixOne.MacOs.Shared
{
	public static class EngineController
	{
		public static FixOne.Engine.Engine Engine {
			get;
			private set;
		}

		public static List<NSString> Log = new List<NSString>();

		public static event EventHandler OnEngineStarted;

		public static event EventHandler OnSessionEstablished;

		public static void StartEngine()
		{
			ListTraceListener listener = new ListTraceListener();
			Trace.Listeners.Add(listener);

			Engine = new FixOne.Engine.Engine();

			Engine.BeforeStart += (object s, FixOne.Engine.Events.EngineEventArgs e) =>
			{
				Log.Add(new NSString(string.Format("Starting engine v.{0}", Engine.Version)));
			};

			Engine.Started += (object s, EventArgs e) =>
			{
				Log.Add(new NSString(string.Format("Engine v.{0} started.", Engine.Version)));

				if(OnEngineStarted != null)
					OnEngineStarted(Engine, EventArgs.Empty);
			};

			Engine.BeforeStop += (object s, FixOne.Engine.Events.EngineEventArgs e) =>
			{
				Log.Add(new NSString("Exit signal received."));
			};

			Engine.SessionEstablished += (object sender, FixOne.Engine.Events.SessionEstablishedEventArgs e) => {
				Log.Add(new NSString(string.Format("Session {0} established.", e.SessionName)));

				if(OnSessionEstablished != null)
					OnSessionEstablished(sender, e);
			};

			Engine.Stopped += (object s, EventArgs e) =>
			{
				Log.Add(new NSString("Engine stopped."));
			};

			Engine.Start();
		}

		public static void StopEngine()
		{
			Engine.Stop();
			Log.Add(new NSString("Stop signal sent. Waiting to close the socket."));
			Thread.Sleep(500);
		}
	}
}

