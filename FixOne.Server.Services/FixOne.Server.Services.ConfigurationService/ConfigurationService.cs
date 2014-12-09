using System;
using System.ServiceModel.Web;

namespace FixOne.Server.Services.ConfigurationService
{
	public class ConfigurationService : IConfigurationService
	{
		private FixOne.Engine.Engine _engine
		{
			get{
				return FixOne.MacOs.Shared.EngineController.Engine;
			}
		}

		[WebInvoke(Method = "GET")]
		public string GetData()
		{

			//string formatQueryStringValue = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["format"];
			//if (formatQueryStringValue.Equals("xml", System.StringComparison.OrdinalIgnoreCase))
			//{
			//	WebOperationContext.Current.OutgoingResponse.Format = WebMessageFormat.Xml;
			//}
			//else if (formatQueryStringValue.Equals("json", System.StringComparison.OrdinalIgnoreCase))
			//{
			//	WebOperationContext.Current.OutgoingResponse.Format = WebMessageFormat.Json;
			//}

			return "Test WCF Mono";
		}

		[WebInvoke(Method = "GET")]
		public string[] GetArray()
		{
			//WebOperationContext.Current.OutgoingResponse.Format = WebMessageFormat.Json;
			return new string[] { "Test", "WCF", "Mono" };
		}

		//[WebInvoke(Method = "GET")]
		public string GetEngineVersion()
		{
			return _engine.Version;
		}

		public Status RestartEngine()
		{
			_engine.Stop ();
			_engine.Start ();

			return Status.StatusOK;
		}

		public bool IsEngineRunning()
		{
			return _engine.IsRunning;
		}

		public ConfigurationService ()
		{
		}
	}
}

