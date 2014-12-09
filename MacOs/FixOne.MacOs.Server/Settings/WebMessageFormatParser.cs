using System;
using FixOne.Engine.Settings.ValueParsers;
using System.ServiceModel.Web;

namespace FixOne.MacOs.Server
{
	public class WebMessageFormatParser : IValueParser
	{
		private const string XML = "XML";
		private const string JSON = "JSON";

		public WebMessageFormatParser ()
		{
		}	

		#region IValueParser implementation

		public object Parse (string value)
		{
			switch (value) {
			case XML:
				return WebMessageFormat.Xml;
			case JSON:
				return WebMessageFormat.Json;
			default:
				return WebMessageFormat.Xml;
			}
		}

		public string Encode (object value)
		{
			if (value is WebMessageFormat) {
				var format = (WebMessageFormat)value;

				switch (format) {
				case WebMessageFormat.Json:
					return JSON;
				case WebMessageFormat.Xml:
					return XML;
				}

			}

			return XML;
		}

		public bool AreEquals (object value1, object value2)
		{
			return value1.Equals (value2);
		}

		public Type ResultType {
			get {
				return typeof(WebMessageFormat);
			}
		}

		#endregion
	}
}

