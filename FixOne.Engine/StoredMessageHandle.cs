using System;
using System.Collections.Generic;
using FixOne.Entities;

namespace FixOne.Engine
{
	internal class StoredMessageHandle : IDisposable
	{
		internal Dictionary<string, long> StorageIdentifiers {
			get;
			private set;
		}

		internal FixMessage Message {
			get;
			private set;
		}

		internal bool IsSent {
			get;
			private set;
		}

		internal long SentSeqNum {
			get;
			private set;
		}

		internal string SentTimestamp {
			get;
			private set;
		}

		internal bool IsSentTracked {
			get;
			set;
		}

		internal bool IsStored {
			get;
			private set;
		}

		internal StoredMessageHandle (FixMessage message)
		{
			StorageIdentifiers = new Dictionary<string, long> ();
			Message = message;
		}

		internal void TrackStored(string storageName, long storageMessageId)
		{
			if (!StorageIdentifiers.ContainsKey (storageName))
				StorageIdentifiers.Add (storageName, storageMessageId);
		}

		internal void TrackIsStoredComplete()
		{
			IsStored = true;
		}

		internal void TrackAsSent(long sentSeqNum, string sentTimeStamp)
		{
			IsSent = true;
			SentSeqNum = sentSeqNum;
			SentTimestamp = sentTimeStamp;

			if (IsStored && !IsSentTracked) {
				Managers.PersistentStoragesManager.Instance.TrackMessageSent (this);
			}
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			IsSent = IsStored = false;
			StorageIdentifiers = null;
		}

		#endregion
	}
}

