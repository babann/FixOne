using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using System.IO;
using FixOne.Common;

namespace FixOne.Engine.Settings
{
	/// <summary>
	/// Represents the Engine settings.
	/// </summary>
	public class EngineSettings : XDocSettings, IEngineSettings
	{
		#region Constants

		/// <summary>
		/// The name of the section in config file.
		/// </summary>
		private const string SECTION_NAME = "Engine";

		#endregion

		#region IEngineSettings implementation

		/// <summary>
		/// Gets or sets the IP address of current isnstance.
		/// </summary>
		/// <value>The instance I.</value>
		[XSetting ("Instance", "address", typeof(ValueParsers.IpAddressParser), new byte[] { 127, 0, 0, 1 })]
		public byte[] InstanceIP {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the listen port of current instance.
		/// </summary>
		/// <value>The listen port.</value>
		[XSetting ("Instance", "port", typeof(ValueParsers.IntParser), 10001)]
		public int ListenPort {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the path to the loggers modules.
		/// </summary>
		/// <value>The loggers path.</value>
		[XSetting ("Loggers", "path", @"..\Loggers")]
		public string LoggersPath {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the path to the dictionaries modules.
		/// </summary>
		/// <value>The dictionaries path.</value>
		[XSetting ("Dictionaries", "path", @"..\Dictionaries")]
		public string DictionariesPath {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the path to the persistent storages modules.
		/// </summary>
		/// <value>The persistent storages path.</value>
		[XSetting ("Storages", "path", @"..\Storages")]
		public string PersistentStoragesPath {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the default numer of workeing threads per session.
		/// </summary>
		/// <value>The numer of workers per session.</value>
		[XSetting ("Defaults", "numOfWP", typeof(ValueParsers.IntParser), 4)]
		public int NumerOfWorkersPerSession {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the default heart beat interval.
		/// </summary>
		/// <value>The default heart beat interval.</value>
		[XSetting ("Defaults", "hrtBit", typeof(ValueParsers.IntParser), 30)]
		public int DefaultHeartBeatInterval {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="FixOne.Engine.Engine"/> accept all initiators.
		/// </summary>
		/// <value><c>true</c> if accept all initiators; otherwise, <c>false</c>.</value>
		[XSetting ("Settings", "acceptAll", typeof(ValueParsers.BoolParser), false)]
		public bool AcceptAllInitiators {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="FixOne.Engine.Engine"/> initiators allow reconnect.
		/// </summary>
		/// <value><c>true</c> if initiators allow reconnect; otherwise, <c>false</c>.</value>
		[XSetting ("Settings", "allowReconnect", typeof(ValueParsers.BoolParser), false)]
		public bool InitiatorsAllowReconnect {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the initiators reconnect attempts.
		/// </summary>
		/// <value>The initiators reconnect attempts.</value>
		[XSetting ("Settings", "reconnectAttempts", typeof(ValueParsers.IntParser), 0)]
		public int InitiatorsReconnectAttempts {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the initiator reconnect delay.
		/// </summary>
		/// <value>The initiator reconnect delay.</value>
		[XSetting ("Settings", "reconnectDelay", typeof(ValueParsers.IntParser), 30)]
		public int InitiatorReconnectDelay {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of the sessions definition file.
		/// </summary>
		/// <value>The name of the sessions definition file.</value>
		[XSetting ("Settings", "sessions", "sessions.xml")]
		public string SessionsDefinitionFileName {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the names of enabled loggers.
		/// </summary>
		/// <value>The enabled loggers.</value>
		[XSetting ("EnabledLoggers", null, typeof(ValueParsers.ArrayOfStringsParser), null)]
		public string[] EnabledLoggers {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the names of enabled storages.
		/// </summary>
		/// <value>The enabled storages.</value>
		[XSetting ("EnabledStorages", null, typeof(ValueParsers.ArrayOfStringsParser), null)]
		public string[] EnabledStorages {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the default storage name.
		/// </summary>
		/// <value>The default storage.</value>
		[XSetting ("EnabledStorages", "default", null)]
		public string DefaultStorage {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="FixOne.Engine.Engine"/> deep debug mode.
		/// </summary>
		/// <value><c>true</c> if deep debug mode; otherwise, <c>false</c>.</value>
		[XSetting ("Settings", "deepDebugMode", typeof(ValueParsers.BoolParser), false)]
		public bool DeepDebugMode {
			get;
			set;
		}

		/// <summary>
		/// Gets the current settings document.
		/// </summary>
		/// <value>The current settings document.</value>
		public XDocument CurrentSettingsDocument {
			get {
				return base.internalDocument;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="FixOne.Engine.Settings.EngineSettings"/> class.
		/// </summary>
		public EngineSettings ()
			: base (SECTION_NAME)
		{
		}

		#endregion
	}
}
