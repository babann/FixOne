using System;
using MonoMac.AppKit;

namespace FixOne.MacOs.Shared
{
	public class MainWindowDelegate : NSWindowDelegate
	{
		public override void WillClose (MonoMac.Foundation.NSNotification notification)
		{
			EngineController.StopEngine ();
			Environment.Exit(0);
		}

		public override void DidResize (MonoMac.Foundation.NSNotification notification)
		{
		}
	}
}

