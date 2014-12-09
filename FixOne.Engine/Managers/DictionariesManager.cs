using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

using FixOne.Common;
using FixOne.Common.Interfaces;
using FixOne.Entities;
using FixOne.Entities.Logging;
using System.Reflection;
using System.ComponentModel;


namespace FixOne.Engine.Managers
{
	class DictionariesManager : GenericManager<IProtocolDictionary, DictionariesManager>
	{
		private DictionariesManager()
			: base(SettingsManager.Instance.CurrentSettings.DictionariesPath)
		{
		}

		internal void Start()
		{
			EngineLogManager.Instance.Info ("Dictionary manager started. {0} dictionaries available.", AvailableModules.Count().ToString());
		}

		/// <summary>
		/// Gets the dictionary suitable for current message (based on tag 8 value)
		/// </summary>
		/// <param name="beginString">Tag 8 value</param>
		/// <returns>Dictionary or throws FixEngineException</returns>
		public IProtocolDictionary GetDictionaryByStringValue(string beginString)
		{
			IProtocolDictionary dictionary = null;
			FixVersion? version = null;

			try
			{
				version = FixOne.Parsers.Generic.FixMessageHeaderParser.ConvertStringToVersion(beginString);
			}
			catch(Exception exc) {
				throw new Entities.Exceptions.FixEngineException (
					string.Format ("Failed to parse version from string '{0}'.", beginString),
					exc);
			}

			if (version.HasValue)
				dictionary = GetDictionaryByFixVersion(version.Value);

			if(dictionary == null)
				throw new Entities.Exceptions.FixEngineException(string.Format("Fix version '{0}' is not supported.", beginString));

			return dictionary;
		}

		public IProtocolDictionary GetDictionaryByFixVersion(FixVersion version)
		{
			return AvailableModules.Where(module => module.SupportedVersion == version).FirstOrDefault();
		}
	}
}
