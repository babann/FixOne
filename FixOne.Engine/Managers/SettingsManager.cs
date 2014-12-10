using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FixOne.Entities;
using FixOne.Engine.Settings;
using System.Diagnostics;
using FixOne.Common;

namespace FixOne.Engine.Managers
{
	/// <summary>
	/// Settings manager.
	/// </summary>
	internal class SettingsManager
	{
		#region Private Fields
		/// <summary>
		/// The current instance of settings manager.
		/// </summary>
		private static SettingsManager _instance = Common.Singleton<SettingsManager>.Instance;

		#endregion

		#region Internal Properties
		/// <summary>
		/// Gets the current instance.
		/// </summary>
		internal static SettingsManager Instance {
			get {
				return _instance;
			}
		}

		/// <summary>
		/// Gets a value indicating whether settings are loaded.
		/// </summary>
		/// <value><c>true</c> if settings loaded; otherwise, <c>false</c>.</value>
		internal bool SettingsLoaded {
			get;
			private set;
		}

		/// <summary>
		/// Gets the current settings.
		/// </summary>
		/// <value>The current settings.</value>
		internal IEngineSettings CurrentSettings {
			get;
			private set;
		}

		/// <summary>
		/// Gets the configured sessions.
		/// </summary>
		/// <value>The configured sessions.</value>
		internal IEnumerable<FixSessionInfo> ConfiguredSessions {
			get;
			private set;
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="FixOne.Engine.Managers.SettingsManager"/> class.
		/// </summary>
		private SettingsManager ()
		{
			SettingsLoaded = false;
		}

		#region Internal Interface

		/// <summary>
		/// Init this instance.
		/// </summary>
		internal void Init()
		{
			try {
				ApplySettings(
					LoadSettings(Common.PathHelper.MapPath ("engine.xml"), true));
			} catch (Entities.Exceptions.FixSessionConfigurationException exc) {
				Trace.WriteLine ("Can't start the Settings Manager. Please see recent exception.");
			}
		}

		/// <summary>
		/// Applies the settings.
		/// </summary>
		internal void ApplySettings(IEngineSettings settings)
		{
			if (settings == null)
				throw new FixOne.Entities.Exceptions.FixSessionConfigurationException ("Settings are not loaded");

			CurrentSettings = settings;
			if(!string.IsNullOrWhiteSpace(settings.SessionsDefinitionFileName))
				ConfiguredSessions = FixSessionInfo.GetFromXml (Common.PathHelper.MapPath (settings.SessionsDefinitionFileName));

			SettingsLoaded = true;
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Loads the settings from the file specified.
		/// </summary>
		/// <returns>The settings or null if rethrow=false and there was an exception.</returns>
		private IEngineSettings LoadSettings(string path, bool rethrow)
		{
			try {
				EngineSettings settings = new EngineSettings ();
				settings.LoadFrom (path);
				return settings;
			} catch (Exception exc) {
				Trace.WriteLine (exc.Message);
				if (exc.InnerException != null) {
					Trace.WriteLine ("Inner exception:");
					Trace.WriteLine (exc.InnerException.Message);
				}

				if (rethrow)
					throw;

				return null;
			}
		}

		#endregion
	}
}
