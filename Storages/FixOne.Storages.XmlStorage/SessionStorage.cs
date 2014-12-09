using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

using FixOne.Entities;

namespace FixOne.Storages.XmlStorage
{
	internal class SessionStorage
	{
		#region Private Fields

		string sessionRootPath;
		string inMessagesPath;
		string outMessagesPath;
		XDocument incomingMessages;
		XDocument outgoingMessages;

		#endregion

		#region Constructor

		internal SessionStorage(string root)
		{
			sessionRootPath = root;

			long dateCode = DateTime.Now.ToBinary();
			inMessagesPath = Path.Combine(sessionRootPath, string.Format("i{0}.xml", dateCode));
			outMessagesPath = Path.Combine(sessionRootPath, string.Format("o{0}.xml", dateCode));

			if (!Directory.Exists(sessionRootPath))
				Directory.CreateDirectory(sessionRootPath);

			if (File.Exists(inMessagesPath))
				using (var stream = File.OpenRead(inMessagesPath))
					incomingMessages = XDocument.Load(stream);
			else
			{
				incomingMessages = new XDocument();
				incomingMessages.Add(new XElement("Messages"));
			}

			if (File.Exists(outMessagesPath))
				using (var stream = File.OpenRead(outMessagesPath))
					outgoingMessages = XDocument.Load(stream);
			else
			{
				outgoingMessages = new XDocument();
				outgoingMessages.Add(new XElement("Messages"));
			}
		}

		#endregion

		internal long Push(MessageToStore message, MessageDirection direction)
		{
			XDocument doc = null;
			switch (direction)
			{
				case MessageDirection.Inbound:
					doc = incomingMessages;
					break;
				case MessageDirection.Outbound:
					doc = outgoingMessages;
					break;
			}

			if (doc != null)
				doc.Root.Add(message.GetElement());

			return message.Id;
		}

		internal void TrackAsSent (long messageIdentifier, long seqNum, string sentTimestamp)
		{
			var element = outgoingMessages.Root.Elements ("Msg")
				.Where (x => (long)x.Attribute ("Id") == messageIdentifier && (bool)x.Attribute ("IsSent") == false).FirstOrDefault ();

			if (element != null) {
				//var msg = new FixMessage (Parsers.Generic.FixMessageHeaderParser.ParseHeaders (Convert.FromBase64String (element.Value), null).First ());

				//msg.SetTagValue (34, seqNum);
				//msg.SetTagValue (56, sentTimestamp);

				element.SetAttributeValue ("IsSent", bool.TrueString);
				element.SetAttributeValue ("Seq", seqNum);
			}

			outgoingMessages.Root.SetAttributeValue("Seq", seqNum);
		}

		internal void Flush()
		{
			incomingMessages.Save(inMessagesPath);
			outgoingMessages.Save(outMessagesPath);
		}

		internal FixSessionState GetSessionState()
		{
			XDocument doc = findLatestSession(MessageDirection.Inbound);

			if (doc != null && doc.Root.Attributes ("State").Any ())
				return (FixSessionState)Enum.Parse (typeof(FixSessionState), doc.Root.Attribute ("State").Value);

			return FixSessionState.Undefined;
		}

		internal long GetSeqNum(MessageDirection direction)
		{
			XDocument doc = findLatestSession(direction);

			switch (direction) {
			case MessageDirection.Inbound:
				{
					if (doc != null)
						return doc.Root.Elements("Msg").Any() ? (long)doc.Root.Elements ("Msg").Last ().Attribute ("Seq") : 0L;
				}
				break;
			case MessageDirection.Outbound:
				{
					if (doc != null)
						return doc.Root.Attributes("Seq").Any() ? (long)doc.Root.Attribute("Seq") : 0L;
				}
				break;
			}


			return 0L;
		}

		internal IEnumerable<MessageToStore> GetUnsentMessages()
		{
			var doc = findLatestSession(MessageDirection.Outbound);
			if (doc == null)
				return new MessageToStore[0];

			return from x in doc.Descendants ("Msg")
					where !(bool)x.Attribute ("IsSent")
				select MessageToStore.Restore(x);
		}

		internal IEnumerable<FixMessage> GetSentMessages(long seqNumFrom, long seqNumTo)
		{
			var doc = findLatestSession (MessageDirection.Outbound);
			if (doc == null)
				yield break;// return new FixMessage[0];

			//TODO: save session status and validate it. if completed then return nothing

			var messages = from x in doc.Descendants ("Msg")
			               where (long)x.Attribute ("Seq") >= seqNumFrom && (long)x.Attribute ("Seq") <= seqNumTo && (bool)x.Attribute ("IsSent")
			               select x;

			if (messages.Any ()) {
				foreach (var msg in messages)
					foreach (var header in Parsers.Generic.FixMessageHeaderParser.ParseHeaders (Convert.FromBase64String (msg.Value), null))
						yield return new FixMessage (header);
			} else {
				yield break;// return new FixMessage[0];
			}
		}

		internal void TrackSessionStateChange(FixSessionState newState, string optionalMessage)
		{
			if (!incomingMessages.Root.Attributes ("State").Any ())
				incomingMessages.Root.Add (new XAttribute ("State", newState.ToString ()));
			else
				incomingMessages.Root.Attribute ("State").SetValue (newState.ToString ());

			if (!incomingMessages.Root.Elements ("History").Any ())
				incomingMessages.Root.Add (new XElement ("History"));

			var stateElement = new XElement ("State", newState.ToString ());
			if (!string.IsNullOrEmpty (optionalMessage))
				stateElement.SetAttributeValue ("text", optionalMessage);

			incomingMessages.Root.Element ("History").Add (stateElement);
				
			if (!outgoingMessages.Root.Attributes ("State").Any ())
				outgoingMessages.Root.Add (new XAttribute ("State", newState.ToString ()));
			else
				outgoingMessages.Root.Attribute ("State").SetValue (newState.ToString ());

			if (!outgoingMessages.Root.Elements ("History").Any ())
				outgoingMessages.Root.Add (new XElement ("History"));

			outgoingMessages.Root.Element ("History").Add (stateElement);
		}

		private XDocument findLatestSession(MessageDirection direction)
		{
			string pattern = "*.xml";
			string filePrefix = null;
			switch (direction)
			{
				case MessageDirection.Inbound:
					pattern = "i" + pattern;
					filePrefix = "i";
					break;
				case MessageDirection.Outbound:
					pattern = "o" + pattern;
					filePrefix = "o";
					break;
			}

			string[] files = Directory.GetFiles(sessionRootPath, pattern);
			if (files != null && files.Length > 0)
			{
				DateTime[] dates = new DateTime[files.Length];
				for (int ii = 0; ii < files.Length; ii++)
				{
					long value = -1;
					if (!long.TryParse(Path.GetFileNameWithoutExtension(files[ii]).Substring(1), out value))
						continue;

					dates[ii] = DateTime.FromBinary(value);
				}

			var path = Path.Combine(sessionRootPath, string.Format("{0}{1}.xml", filePrefix, dates.Max().ToBinary().ToString()));
				if (File.Exists(path))
					return XDocument.Load(path);
			}
			return null;
		}
	}
}
