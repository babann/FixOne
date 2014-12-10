using System;
using System.Xml.Linq;

namespace FixOne.Common
{
	public interface IEngineSettings
	{
			byte[] InstanceIP {
				get;
				set;
			}

			int ListenPort {
				get;
				set;
			}

			string LoggersPath {
				get;
				set;
			}

			string DictionariesPath {
				get;
				set;
			}

			string PersistentStoragesPath {
				get;
				set;
			}

			int NumerOfWorkersPerSession {
				get;
				set;
			}

			int DefaultHeartBeatInterval {
				get;
				set;
			}

			bool AcceptAllInitiators {
				get;
				set;
			}

			bool InitiatorsAllowReconnect {
				get;
				set;
			}

			int InitiatorsReconnectAttempts {
				get;
				set;
			}

			int InitiatorReconnectDelay {
				get;
				set;
			}

			string SessionsDefinitionFileName {
				get;
				set;
			}

			string[] EnabledLoggers {
				get;
				set;
			}

			string[] EnabledStorages {
				get;
				set;
			}

			string DefaultStorage {
				get;
				set;
			}

			bool DeepDebugMode {
				get;
				set;
			}

			XDocument CurrentSettingsDocument {
				get ;
			}
	}
}

