using System;
using System.Diagnostics;
using MonoMac.Foundation;

namespace FixOne.MacOs.Shared
{
	public class ListTraceListener : TextWriterTraceListener
	{
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			this.WriteLine(data.ToString());
		}

		public override void WriteLine (string message)
		{
			EngineController.Log.Add (new NSString( message));
		}

	}
}

