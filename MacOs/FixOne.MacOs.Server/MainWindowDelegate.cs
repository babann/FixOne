using System;
using MonoMac.AppKit;
using FixOne.MacOs.Shared;

namespace FixOne.MacOs.Server
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

