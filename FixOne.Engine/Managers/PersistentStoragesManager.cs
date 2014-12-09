using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using FixOne.Entities;

namespace FixOne.Engine.Managers
{
	internal class PersistentStoragesManager : Common.GenericManager<Common.Interfaces.IPersitentStorage, PersistentStoragesManager>
	{
		private const int flushTimeSeconds = 10;

		#region Private Fields

		ConcurrentQueue<Tuple<StoredMessageHandle, MessageDirection>> messages = new ConcurrentQueue<Tuple<StoredMessageHandle, MessageDirection>> ();
		Thread storageWriterThread;
		bool exit = false;
		bool started = false;
		ConcurrentDictionary<SessionIdentifier, ConcurrentBag<string>> sessionsToStorages = new ConcurrentDictionary<SessionIdentifier, ConcurrentBag<string>> ();
		DateTime recentFlushTime;

		#endregion

		#region Constructor

		private PersistentStoragesManager ()
			: base (SettingsManager.Instance.CurrentSettings.PersistentStoragesPath)
		{
			
		}

		#endregion

		#region Public Methods

		internal void Start ()
		{
			if (started)
				return;

			List<string> initializationErrors = new List<string> ();
			var modules = AvailableModules.ToList ();

			foreach (var module in modules) {
				try {
					module.InitSettings (SettingsManager.Instance.CurrentSettings.CurrentSettingsDocument);
				} catch (Exception exc) {
					initializationErrors.Add (string.Format ("Failed to initialize storage '{0}'. Error: {1}. Module will be disabled.", module.Name, exc.Message));
					DisableModules (module.Name);
				}
			}

			if (initializationErrors.Any ())
				initializationErrors.ForEach (msg => EngineLogManager.Instance.Error (msg));

			started = true;

			exit = false;
			storageWriterThread = new Thread (() => {
				while (!exit) {
					if (EnabledModules != null) {
						int bufferSize = 1000;
						List<Tuple<StoredMessageHandle, MessageDirection>> buffer = new List<Tuple<StoredMessageHandle, MessageDirection>> (bufferSize);
						int cnt = 0;
						Tuple<StoredMessageHandle, MessageDirection> msg;

						lock (writeLocker) {
							while (messages.TryDequeue (out msg) && cnt++ < bufferSize)
								buffer.Add (msg);
						}

						if (buffer.Any ()) {
							for (int ii = 0; ii < buffer.Count; ii++) {
								msg = buffer [ii];
								SessionIdentifier ident = new SessionIdentifier (msg.Item1.Message.SenderCompId, msg.Item1.Message.TargetCompId);
								var bag = sessionsToStorages.ContainsKey (ident) ? sessionsToStorages [ident] : null;
								if (bag != null && bag.Any ()) {
									foreach (var storage in EnabledModules.Where(module => bag.Contains(module.Name))) {
										try {
											long storedMessageId = storage.StoreMessage (msg.Item1.Message, msg.Item2);
											msg.Item1.TrackStored (storage.Name, storedMessageId);
											if (msg.Item1.IsSent) {
												storage.TrackAsSent (msg.Item1.Message.SenderCompId,
													msg.Item1.Message.TargetCompId,
													storedMessageId,
													msg.Item1.SentSeqNum,
													msg.Item1.SentTimestamp);
												msg.Item1.IsSentTracked = true;
											}
										} catch (Exception exc) {
											EngineLogManager.Instance.Error (
												string.Format ("Failed to store message {0} in an storage {1}. If error will occur again, please disable the storage. Error: {2}.",
													msg.Item1.Message.ToString (),
													storage.Name),
												exc);
										}
									}

									msg.Item1.TrackIsStoredComplete ();
									if (msg.Item1.IsSent && !msg.Item1.IsSentTracked)
										TrackMessageSent (msg.Item1);
								}
							}
						}

						if ((DateTime.Now - recentFlushTime).TotalSeconds > flushTimeSeconds)
							doflush ();
					}

					Thread.Sleep (1);
				}
			});
			storageWriterThread.IsBackground = true;
			storageWriterThread.Name = "STORWRT";
			storageWriterThread.Start ();
		}

		public void Stop ()
		{
			doflush ();
			started = false;
			exit = true;
		}

		public void RegisterSession (string side1, string side2, string storageName)
		{
			SessionIdentifier ident = new SessionIdentifier (side1, side2);
			var storages = new ConcurrentBag<string> ();
			storages.Add (storageName);
			if (!sessionsToStorages.ContainsKey (ident))
				sessionsToStorages.AddOrUpdate (ident, storages, (key, oldValue) => {
					oldValue.Add (storageName);
					return oldValue;
				});
			else {
				var bag = sessionsToStorages [ident];
				if (!bag.Contains (storageName))
					bag.Add (storageName);
			}
		}

		internal StoredMessageHandle Push (FixMessage message, MessageDirection direction)
		{
			var handle = new StoredMessageHandle (message);
			messages.Enqueue (Tuple.Create (handle, direction));
			return handle;
		}

		internal void TrackMessageSent (StoredMessageHandle handle)
		{
			foreach (var pairSessionIdentifier in handle.StorageIdentifiers) {
				var storage = EnabledModules.FirstOrDefault (x => x.Name == pairSessionIdentifier.Key);
				if (storage == null)
					continue;

				storage.TrackAsSent (handle.Message.SenderCompId,
					handle.Message.TargetCompId,
					pairSessionIdentifier.Value,
					handle.SentSeqNum,
					handle.SentTimestamp);

				handle.IsSentTracked = true;
			}

			handle.Dispose ();
		}

		internal void TrackSessionState (string side1, string side2, FixSessionState state, string comment)
		{
			var ident = new SessionIdentifier (side1, side2);

			var bag = sessionsToStorages.ContainsKey (ident) ? sessionsToStorages [ident] : null;
			if (bag == null)
				return;

			var storages = EnabledModules.Where (module => bag.Contains (module.Name)).ToArray();
			foreach (var storage in storages) {
				storage.TrackSessionStateChange (side1, side2, state, comment);
			}
		}

		internal FixSessionState GetLatestSessionState(string side1, string side2)
		{
			var ident = new SessionIdentifier (side1, side2);
			if (!sessionsToStorages.ContainsKey (ident) || !sessionsToStorages [ident].Any ())
				return FixSessionState.Undefined;

			var storage = EnabledModules.FirstOrDefault (module => module.Name == sessionsToStorages [ident].ElementAt (0));
			if (storage == null)
				return FixSessionState.Undefined;;

			return storage.GetLatestState (side1, side2);
		}

		internal long GetRecentSequenceNumber (string side1, string side2, MessageDirection direction)
		{
			var ident = new SessionIdentifier (side1, side2);
			if (!sessionsToStorages.ContainsKey (ident) || !sessionsToStorages [ident].Any ())
				return 0;

			var storage = EnabledModules.FirstOrDefault (module => module.Name == sessionsToStorages [ident].ElementAt (0));
			if (storage == null)
				return 0;

			switch (direction) {
			case MessageDirection.Inbound:
				return storage.GetLatestInboundSeqNum (side1, side2);
			case MessageDirection.Outbound:
				return storage.GetLatestOutboundSeqNum (side1, side2);
			}

			return 0;
		}

		public IEnumerable<FixMessage> GetGapFill (string side1, string side2, long seqFrom, long seqTo)
		{
			doflush ();

			var ident = new SessionIdentifier (side1, side2);

			if (!sessionsToStorages.ContainsKey (ident))
				return null;

			var storageNames = sessionsToStorages [ident];
			var storages = EnabledModules.Where (module => storageNames.Contains (module.Name));
			foreach (var storage in storages) {
				var messages = storage.GetSentMessages (side1, side2, seqFrom, seqTo);
				if (messages != null && messages.Any ())
					return messages.ToList();
			}
			return null;
		}

		public IEnumerable<FixMessage> GetUnsentMessages (string side1, string side2)
		{
			var ident = new SessionIdentifier (side1, side2);

			if (!sessionsToStorages.ContainsKey (ident))
				return null;

			var storageNames = sessionsToStorages [ident];
			var storages = EnabledModules.Where (module => storageNames.Contains (module.Name));
			var storage = storages.FirstOrDefault ();
			//TODO: merge messages from all storages
			if (storage == null)
				return null;

			return storage.GetUnsentMessages (side1, side2).Select (x => x.Value).ToList();
		}

		#endregion

		#region Private Methods

		private void doflush ()
		{
			try {
				foreach (var storage in EnabledModules)
					storage.Flush ();

				recentFlushTime = DateTime.Now;
			} catch (Exception exc) {
				EngineLogManager.Instance.Error ("[PersistentStoragesManager] Can't do flush because of error.", exc);
			}
		}

		#endregion

	}
}
