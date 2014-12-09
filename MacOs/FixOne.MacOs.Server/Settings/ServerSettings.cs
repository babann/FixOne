using System;
using FixOne.Engine.Settings;
using FixOne.Engine.Settings.ValueParsers;
using System.ServiceModel.Web;

namespace FixOne.MacOs.Server.Settings
{
	public class ServerSettings : XDocSettings
	{
		[XSetting ("ConfigurationService", "port", typeof(IntParser), 8000)]
		public int ConfigurationServicePort{ get; set; }

		[XSetting ("ConfigurationService", "name", "Configurator")]
		public string ConfiguratorServiceEntryPoint { get; set; }

		[XSetting ("ConfigurationService", "format", typeof(WebMessageFormatParser), WebMessageFormat.Xml)]
		public WebMessageFormat OutputFormat { get; set; }

		public ServerSettings ()
			: base("Server")
		{
		}
	}
}

