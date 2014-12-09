using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FixOne.Entities;
using FixOne.Engine.Settings;
using System.Diagnostics;

namespace FixOne.Engine.Managers
{
	internal class SettingsManager
	{
		internal static SettingsManager Instance {
			get {
				return Common.Singleton<SettingsManager>.Instance;
			}
		}

		internal EngineSettings CurrentSettings {
			get;
			private set;
		}

		internal IEnumerable<FixSessionInfo> ConfiguredSessions {
			get;
			private set;
		}

		private SettingsManager ()
		{
			try {
				CurrentSettings = new EngineSettings ();
				CurrentSettings.LoadFrom (Common.PathHelper.MapPath ("engine.xml"));
				ConfiguredSessions = FixSessionInfo.GetFromXml (Common.PathHelper.MapPath (CurrentSettings.SessionsDefinitionFileName));
			} catch (Entities.Exceptions.FixSessionConfigurationException exc) {
				Trace.WriteLine ("Can't start the Settings Manager. Error:");
				Trace.WriteLine (exc.Message);
				if (exc.InnerException != null) {
					Trace.WriteLine ("Inner exception:");
					Trace.WriteLine (exc.InnerException.Message);
				}
				CurrentSettings = null;
			}
		}
	}
}
