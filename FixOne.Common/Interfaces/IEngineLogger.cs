using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FixOne.Entities.Logging;
using System.Xml.Linq;

namespace FixOne.Common.Interfaces
{
	public interface IEngineLogger : IModule
	{
		void LogMessage(LogMessage message);

		void InitSettings(XDocument settingsDocument);

		IEnumerable<XElement> GetSettings();

		void ApplySettings(IEnumerable<XElement> settings);
	}
}
