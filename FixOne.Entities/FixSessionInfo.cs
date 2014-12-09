using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FixOne.Entities
{
	public class FixSessionInfo
	{
		#region Fields

		private string name;
		private byte[] acceptorIp;
		private int acceptorPort;

		#endregion

		#region Properties

		public FixVersion Version
		{
			get;
			internal set;
		}

		public FixSessionRole Role
		{
			get;
			internal set;
		}

		public string SenderCompId
		{
			get;
			internal set;
		}

		public string TargetCompId
		{
			get;
			internal set;
		}

		public string Name
		{
			get
			{
				if (name == null)
					name = string.Format("{0}:{1}", SenderCompId, TargetCompId);

				return name;
			}
		}

		public int HeartBeatInterval
		{
			get;
			internal set;
		}

		public int NumberOfWorkingThreads
		{
			get;
			internal set;
		}

		public byte[] AcceptorIP
		{
			get
			{
				if (Role == FixSessionRole.Acceptor)
					return null;

				return acceptorIp;
			}
		}

		public int AcceptorPort
		{
			get
			{
				if (Role == FixSessionRole.Acceptor)
					return -1;

				return acceptorPort;
			}
		}

		public bool AllowReconnect
		{
			get;
			private set;
		}

		public int ReceonnectAttempts
		{
			get;
			private set;
		}

		public string [] Storages {
			get;
			private set;
		}

		#endregion

		public FixSessionInfo()
		{
		}

		internal FixSessionInfo(FixVersion version,
			FixSessionRole role,
			string senderCompId,
			string targetCompId,
			int heartBeatInterval,
			int numberOfWorkingThreads,
			string [] storages,
			byte [] acceptorIp,
			int acceptorPort,
			bool allowReconnect,
			int reconnectAttempts)
		{
			Version = version;
			Role = role;
			HeartBeatInterval = heartBeatInterval;
			NumberOfWorkingThreads = numberOfWorkingThreads;
			SenderCompId = senderCompId;
			TargetCompId = targetCompId;
			Storages = storages;
			if (Role == FixSessionRole.Initiator)
			{
				this.acceptorIp = acceptorIp;
				this.acceptorPort = acceptorPort;
				AllowReconnect = allowReconnect;
				ReceonnectAttempts = reconnectAttempts;
			}
		}

		internal FixSessionInfo(Configuration.FixSessionConfiguration configuration)
		{
			Version = configuration.Version;
			Role = configuration.Role;
			HeartBeatInterval = configuration.HeartBeatInterval;
			NumberOfWorkingThreads = configuration.NumberOfWorkingThreads;
			SenderCompId = configuration.SenderCompId;
			TargetCompId = configuration.TargetCompId;
			Storages = configuration.Storages.ToArray();
			if (Role == FixSessionRole.Initiator)
			{
				acceptorIp = configuration.Acceptor.Address;
				acceptorPort = configuration.Acceptor.Port;
				AllowReconnect = configuration.Acceptor.AllowReconnect;
				ReceonnectAttempts = configuration.Acceptor.ReconnectAttempts;
			}
		}

		internal FixSessionInfo(FixMessageHeader header)
		{
			SenderCompId = header.SenderCompId;
			TargetCompId = header.TargetCompId;
			Version = header.Version;
		}

		internal static IEnumerable<FixSessionInfo> GetFromXml(string sessionsDefinitionPath)
		{
			if (string.IsNullOrEmpty(sessionsDefinitionPath))
				throw new ArgumentException("Path to the sessions definition file is not specified.", "sessionsDefinitionPath");

			if (!System.IO.File.Exists(sessionsDefinitionPath))
				throw new System.IO.FileNotFoundException("Sessions definition file does not exists at " + sessionsDefinitionPath);

			XDocument doc = XDocument.Load(sessionsDefinitionPath);
			var sessionDefinitions = doc.Element("Sessions").Elements("Session");

			return from definition in sessionDefinitions
				select new FixSessionInfo(new Configuration.FixSessionConfiguration(definition));
		}

		
	}
}
