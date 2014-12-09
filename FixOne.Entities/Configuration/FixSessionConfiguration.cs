using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FixOne.Entities.Configuration
{
	internal class FixSessionConfiguration : XElement
	{
		#region Constants

		private const string ATTRIBUTE_NAME_ROLE = "Role";
		private const string ATTRIBUTE_NAME_VERSION = "Version";
		private const string ATTRIBUTE_NAME_HEARTBEAT = "HeartBeat";
		private const string ATTRIBUTE_NAME_THREADS = "Threads";

		private const string ELEMENT_NAME_SESSION = "Session";
		private const string ELEMENT_NAME_SENDER = "SenderCompId";
		private const string ELEMENT_NAME_TARGET = "TargetCompId";
		private const string ELEMENT_NAME_ACCEPTOR = "Acceptor";

		private const string ELEMENT_NAME_STORAGES = "Storages";

		//For Acceptor element
		private const string ATTRIBUTE_NAME_ADDRESS = "Address";
		private const string ATTRIBUTE_NAME_PORT = "Port";
		private const string ATTRIBUTE_NAME_RECONNECTS = "ReconnectAttemts";

		#endregion

		internal FixSessionConfiguration(XElement other)
			: base(other)
		{
		}

		#region Default values

		//TODO: move to engine configuration
		//TODO: throw warning if default value is used (or INFO???)
		int defaultHRTBTInterval = 30;
		int defaultWorkingThreads = 1;

		#endregion

		#region Properties

		internal FixSessionRole Role
		{
			get
			{
				if (Attributes(ATTRIBUTE_NAME_ROLE).Any())
					return (FixSessionRole)Enum.Parse(typeof(FixSessionRole), (string)Attribute(ATTRIBUTE_NAME_ROLE));
				else
					throw new Exceptions.FixSessionConfigurationException(ATTRIBUTE_NAME_ROLE + " attribute is not specified.");
			}
		}

		internal FixVersion Version
		{
			get
			{
				if (Attributes(ATTRIBUTE_NAME_VERSION).Any())
					return (FixVersion)Enum.Parse(typeof(FixVersion), (string)Attribute(ATTRIBUTE_NAME_VERSION));
				else
					throw new Exceptions.FixSessionConfigurationException(ATTRIBUTE_NAME_VERSION + " attribute is not specified.");
			}
		}

		internal int HeartBeatInterval
		{
			get
			{
				return Attributes(ATTRIBUTE_NAME_HEARTBEAT).Any() ? (int)Attribute(ATTRIBUTE_NAME_HEARTBEAT) : defaultHRTBTInterval;
			}
		}

		//TODO: known issue - this configuration does not work for acceptor because at the moment of first
		//incoming message we don't know yet what is the session
		internal int NumberOfWorkingThreads
		{
			get
			{
				return Attributes(ATTRIBUTE_NAME_THREADS).Any() ? (int)Attribute(ATTRIBUTE_NAME_THREADS) : defaultWorkingThreads;
			}
		}

		internal string SenderCompId
		{
			get
			{
				if(Elements(ELEMENT_NAME_SENDER).Any())
					return (string)Element(ELEMENT_NAME_SENDER);
				else
					throw new Exceptions.FixSessionConfigurationException(ELEMENT_NAME_SENDER + " element is not specified.");
			}
		}

		internal string TargetCompId
		{
			get
			{
				if(Elements(ELEMENT_NAME_TARGET).Any())
					return (string)Element(ELEMENT_NAME_TARGET);
				else
					throw new Exceptions.FixSessionConfigurationException(ELEMENT_NAME_TARGET + " element is not specified.");
			}
		}

		internal AcceptorConfiguration Acceptor
		{
			get
			{
				if (Role == FixSessionRole.Acceptor)
					return null;

				if (!Elements(ELEMENT_NAME_ACCEPTOR).Any())
					throw new Exceptions.FixSessionConfigurationException(ELEMENT_NAME_ACCEPTOR + " element is not specified.");

				return new AcceptorConfiguration(Element(ELEMENT_NAME_ACCEPTOR));
			}
		}

		internal IEnumerable<string> Storages {
			get {
				if(!Elements(ELEMENT_NAME_STORAGES).Any())
					return new string[0];

				return Elements(ELEMENT_NAME_STORAGES).Elements("Name").Select(x => x.Value);
			}
		}

		#endregion

		#region Acceptor block

		internal class AcceptorConfiguration : XElement
		{
			internal AcceptorConfiguration(XElement other)
				: base(other)
			{
			}

			internal byte[] Address
			{
				get
				{
					if (Attributes(ATTRIBUTE_NAME_ADDRESS).Any())
					{
						var val = Utils.ParseIp((string)Attribute(ATTRIBUTE_NAME_ADDRESS));
						if (val == null)
							throw new Exceptions.FixSessionConfigurationException(ATTRIBUTE_NAME_ADDRESS + " attribute can't be parsed for session.");
						else
							return val;
					}
					else
						throw new Exceptions.FixSessionConfigurationException(ATTRIBUTE_NAME_ADDRESS + " attribute is not specified.");
				}
			}

			internal int Port
			{
				get
				{
					if(!Attributes(ATTRIBUTE_NAME_PORT).Any())
						throw new Exceptions.FixSessionConfigurationException(ATTRIBUTE_NAME_PORT + " attribute is not specified.");

					int port;

					if(!int.TryParse((string)Attribute(ATTRIBUTE_NAME_PORT), out port))
						throw new Exceptions.FixSessionConfigurationException(ATTRIBUTE_NAME_PORT + " attribute can't be parsed for session.");

					return port;
				}
			}

			internal bool AllowReconnect
			{
				get
				{
					return Attributes(ATTRIBUTE_NAME_RECONNECTS).Any();
				}
			}

			internal int ReconnectAttempts
			{
				get
				{
					if (!AllowReconnect)
						return -1;

					int numOfAttemts;
					if (!int.TryParse((string)Attribute(ATTRIBUTE_NAME_RECONNECTS), out numOfAttemts))
						throw new Exceptions.FixSessionConfigurationException(ATTRIBUTE_NAME_RECONNECTS + " attribute can't be parsed for session.");

					if (numOfAttemts < 0)
						throw new Exceptions.FixSessionConfigurationException(ATTRIBUTE_NAME_RECONNECTS + " value must be greater or equeal to zero.");

					return numOfAttemts;
				}
			}
		}
		#endregion
	}

	
}
