using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FixOne.Entities;
using System.Xml.Linq;

namespace FixOne.Common.Interfaces
{
	public interface IPersitentStorage : IModule
	{
		/// <summary>
		/// Put he message in a storage. 
		/// Outgoing messages will be marked as NOT_SENT.
		/// Returns the internal identifier of a message (Timestamp)
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="direction">Direction.</param>
		long StoreMessage(FixMessage message, MessageDirection direction);

		void TrackAsSent (string senderCompId, string targetCompId, long messageIdentifier, long seqNum, string SentTimestamp);

		IEnumerable<KeyValuePair<long, FixMessage>> GetUnsentMessages (string senderCompId, string targetCompId);

		void Flush();

		//IEnumerable<FixMessage> GetInboundMessages(DateTime? from);

		//IEnumerable<FixMessage> GetOutboundMessages(DateTime? from);

		long GetLatestInboundSeqNum(string side1, string side2);

		long GetLatestOutboundSeqNum(string side1, string side2);

		IEnumerable<FixMessage> GetSentMessages(string side1, string side2, long seqNumFrom, long seqNumTo);

		void InitSettings (XDocument settingsDocument);

		void TrackSessionStateChange(string side1, string side2, FixSessionState newState, string optionalMessage);

		FixSessionState GetLatestState (string side1, string side2);
	}
}
