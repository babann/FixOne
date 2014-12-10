using NUnit.Framework;
using System;
using System.Xml.Linq;
using FixOne.Engine.Settings;
using FixOne.Common;
using Moq;
using FixOne.Common.Interfaces;

namespace UnitTests
{
	[TestFixture ()]
	public class Test
	{
		private const string sessionConfigurationElement = 
			"<Session Role=\"Acceptor\" Version=\"FIX44\" HeartBeat=\"30\">\n\t\t<SenderCompId>ServerO</SenderCompId>\n\t\t<TargetCompId>Client</TargetCompId>\n\t</Session>";
	
		private const string engineSettingsDocument = 
			"<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<FixOne>\n\t<LogToTrace>\n\t\t<Settings level=\"Info\"/>\n\t</LogToTrace>\n\t<LogToTextFile>\n\t\t<Settings\n\t\t\tlevel=\"Debug\"\n\t\t\tpath=\"\\\\DebugLogs\"/>\n\t</LogToTextFile>\n\t<Engine>\n\t\t<Instance address=\"127.0.0.1\" port=\"10001\"/>\n\t\t<Loggers path=\"..\\Loggers\" />\n\t\t<Dictionaries path=\"..\\Dictionaries\" />\n\t\t<Storages path=\"..\\Storages\" />\n\t\t<Defaults numOfWP=\"4\" hrtBit=\"30\"/>\n\t\t<Settings acceptAll=\"false\" reconnectDelay=\"60\" sessions=\"sessions.xml\" />\n\t\t<EnabledLoggers>\n\t\t\t<item>Log to System Trace</item>\n\t\t\t<item>Log to Text File</item>\n\t\t</EnabledLoggers>\n\t\t<EnabledStorages>\n\t\t\t<item>XML Persistent Storage</item>\n\t\t</EnabledStorages>\n\t</Engine>\n</FixOne>";

		[Test ()]
		public void LoadSessionConfigurationTest ()
		{
			XElement configElement = XElement.Parse (sessionConfigurationElement);
			var config = new FixOne.Entities.Configuration.FixSessionConfiguration (configElement);
			var info = new FixOne.Entities.FixSessionInfo (config);


			Assert.AreEqual (config.Role, info.Role);
			Assert.AreEqual (config.Version, info.Version);
		}

		[Test()]
		public void CreateFixSessionByInfoTest()
		{
			var dictionaryName = "FIX44";

			var moqSettings = new Moq.Mock<IEngineSettings> ();
			FixOne.Engine.Managers.SettingsManager.Instance.ApplySettings (moqSettings.Object);

			var moqDitionary = new Moq.Mock<IProtocolDictionary> ();
			moqDitionary.Setup (x => x.Name).Returns (dictionaryName);
			moqDitionary.Setup (x => x.SupportedVersion).Returns (FixOne.Entities.FixVersion.FIX44);

			var added = FixOne.Engine.Managers.DictionariesManager.Instance.AddDictionary (moqDitionary.Object, false);
			Assert.IsTrue (added);

			XElement configElement = XElement.Parse (sessionConfigurationElement);
			var config = new FixOne.Entities.Configuration.FixSessionConfiguration (configElement);
			var info = new FixOne.Entities.FixSessionInfo (config);

			var session = new FixOne.Engine.FixSession (info);

			Assert.AreEqual (info.Name, session.Name);
			Assert.AreEqual (info.Role, session.Role);
			Assert.AreSame (session.dictionary, moqDitionary.Object);
		}
	}
}

