using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using FixOne.Common;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using FixOne.Server.Services.ConfigurationService;
using System.ServiceModel;

namespace FixOne.MacOs.Server
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		Settings.ServerSettings _currentServerSettings;
		WebServiceHost configurationServiceHost;

		MainWindowController mainWindowController;

		public AppDelegate ()
		{
			_currentServerSettings = new Settings.ServerSettings ();
			_currentServerSettings.LoadFrom (PathHelper.MapPath ("engine.xml"));
		}

		public override void FinishedLaunching (NSObject notification)
		{
			StartServices ();

			mainWindowController = new MainWindowController ();
			mainWindowController.Window.MakeKeyAndOrderFront (this);
		}

		public override NSApplicationTerminateReply ApplicationShouldTerminate (NSApplication sender)
		{
			if(configurationServiceHost != null && configurationServiceHost.State == CommunicationState.Opened)
				configurationServiceHost.Close ();

			return NSApplicationTerminateReply.Now;
		}

		private void StartServices()
		{
			//http://msdn.microsoft.com/en-us/library/ee476510(v=vs.100).aspx

			string baseAddress = string.Format("http://{0}:{1}/{2}", 
				Environment.MachineName, 
				_currentServerSettings.ConfigurationServicePort, 
				_currentServerSettings.ConfiguratorServiceEntryPoint);

			//ServiceHost configuratorSvcHost = new ServiceHost (typeof(ConfigurationService), new Uri (baseAddress));
			//configuratorSvcHost.AddServiceEndpoint (typeof(IConfigurationService), new WebHttpBinding (), "").Behaviors.Add (new WebHttpBehavior ());
			//configuratorSvcHost.Open ();

			configurationServiceHost = new WebServiceHost(typeof(ConfigurationService), new Uri (baseAddress));
			ServiceEndpoint sep = configurationServiceHost.AddServiceEndpoint(typeof(IConfigurationService), new WebHttpBinding(), "");
			WebHttpBehavior whb = sep.Behaviors.Find<WebHttpBehavior>();
			if (whb != null)
			{
				whb.AutomaticFormatSelectionEnabled = false;
				whb.DefaultOutgoingResponseFormat = _currentServerSettings.OutputFormat;
			}
			else
			{
				WebHttpBehavior webBehavior = new WebHttpBehavior();
				webBehavior.AutomaticFormatSelectionEnabled = false;
				webBehavior.DefaultOutgoingResponseFormat = _currentServerSettings.OutputFormat;
				sep.Behaviors.Add(webBehavior);
			}
			configurationServiceHost.Open();  
		}
	}
}

