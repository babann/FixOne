using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using MonoMac.Foundation;

namespace FixOne.MacOs.Server
{
	public static class EngineController
	{
		private static FixOne.Engine.Engine engine;

		public static List<NSString> Log = new List<NSString>();

		public static void StartEngine()
		{
			ListTraceListener listener = new ListTraceListener();
			Trace.Listeners.Add(listener);

			engine = new FixOne.Engine.Engine();

			engine.BeforeStart += (object s, FixOne.Engine.Events.EngineEventArgs e) =>
			{
				Log.Add(new NSString(string.Format("Starting engine v.{0}", engine.Version)));
			};

			engine.Started += (object s, EventArgs e) =>
			{
				Log.Add(new NSString(string.Format("Engine v.{0} started.", engine.Version)));
			};

			engine.BeforeStop += (object s, FixOne.Engine.Events.EngineEventArgs e) =>
			{
				Log.Add(new NSString(string.Format("Exit signal received.")));
			};

			engine.Stopped += (object s, EventArgs e) =>
			{
				Log.Add(new NSString(string.Format("Engine stopped.")));
			};

			engine.Start();
		}

		public static void StopEngine()
		{
			engine.Stop();
			Log.Add(new NSString(string.Format("Stop signal sent. Waiting to close the socket.")));
			Thread.Sleep(500);
		}
	}
}

