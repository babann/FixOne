using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using System.IO;

namespace FixOne.Engine.Settings
{
	public class EngineSettings : XDocSettings
	{
		[XSetting ("Instance", "address", typeof(ValueParsers.IpAddressParser), new byte[] { 127, 0, 0, 1 })]
		public byte[] InstanceIP {
			get;
			set;
		}

		[XSetting ("Instance", "port", typeof(ValueParsers.IntParser), 10001)]
		public int ListenPort {
			get;
			set;
		}

		[XSetting ("Loggers", "path", @"..\Loggers")]
		public string LoggersPath {
			get;
			set;
		}

		[XSetting ("Dictionaries", "path", @"..\Dictionaries")]
		public string DictionariesPath {
			get;
			set;
		}

		[XSetting ("Storages", "path", @"..\Storages")]
		public string PersistentStoragesPath {
			get;
			set;
		}

		[XSetting ("Defaults", "numOfWP", typeof(ValueParsers.IntParser), 4)]
		public int NumerOfWorkersPerSession {
			get;
			set;
		}

		[XSetting ("Defaults", "hrtBit", typeof(ValueParsers.IntParser), 30)]
		public int DefaultHeartBeatInterval {
			get;
			set;
		}

		[XSetting ("Settings", "acceptAll", typeof(ValueParsers.BoolParser), false)]
		public bool AcceptAllInitiators {
			get;
			set;
		}

		[XSetting ("Settings", "allowReconnect", typeof(ValueParsers.BoolParser), false)]
		public bool InitiatorsAllowReconnect {
			get;
			set;
		}

		[XSetting ("Settings", "reconnectAttempts", typeof(ValueParsers.IntParser), 0)]
		public int InitiatorsReconnectAttempts {
			get;
			set;
		}

		[XSetting ("Settings", "reconnectDelay", typeof(ValueParsers.IntParser), 30)]
		public int InitiatorReconnectDelay {
			get;
			set;
		}

		[XSetting ("Settings", "sessions", "sessions.xml")]
		public string SessionsDefinitionFileName {
			get;
			set;
		}

		[XSetting ("EnabledLoggers", null, typeof(ValueParsers.ArrayOfStringsParser), null)]
		public string[] EnabledLoggers {
			get;
			set;
		}

		[XSetting ("EnabledStorages", null, typeof(ValueParsers.ArrayOfStringsParser), null)]
		public string[] EnabledStorages {
			get;
			set;
		}

		[XSetting ("EnabledStorages", "default", null)]
		public string DefaultStorage {
			get;
			set;
		}

		[XSetting ("Settings", "deepDebugMode", typeof(ValueParsers.BoolParser), false)]
		public bool DeepDebugMode {
			get;
			set;
		}

		public EngineSettings ()
			: base ("Engine")
		{
		}

		public XDocument CurrentSettingsDocument {
			get {
				return base.internalDocument;
			}
		}
	}
}
