using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using System.Diagnostics;
using System.Threading;
using System.ServiceModel;

using FixOne.Server.Services.ConfigurationService;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace FixOne.MacOs.Server
{
	class MainClass
	{
		static void Main (string[] args)
		{
			NSApplication.Init ();
			NSApplication.Main (args);
		}
	}
}

