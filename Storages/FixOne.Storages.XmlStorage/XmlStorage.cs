using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections.Concurrent;

using FixOne.Entities;
using System.Xml.Linq;
using System.Diagnostics;

namespace FixOne.Storages.XmlStorage
{
	public class XmlStorage : Common.Interfaces.IPersitentStorage
	{
		#region Constants

		private const string NAME = "XML Persistent Storage";
		private const string CONFIG_SECTION_NAME = "XmlStorage";
		private const string DEFAULT_STORAGE_PATH = "..\\XmlStorageData";

		#endregion

		#region Private Fields

		private Settings _settings;

		private string storageRootPath;
		private bool flushing = false;

		private ConcurrentDictionary<int, SessionStorage> storages = new ConcurrentDictionary<int, SessionStorage>();

		#endregion

		#region Constructor

		public XmlStorage()
		{
			_settings = new Settings(CONFIG_SECTION_NAME);
		}

		#endregion

		#region IPersitentStorage Members

		public string Name
		{
			get { return NAME; }
		}

		public bool Enabled
		{
			get;
			set;
		}

		public void TrackSessionStateChange(string side1, string side2, FixSessionState newState, string optionalMessage)
		{
			getStorage (side1, side2).TrackSessionStateChange (newState, optionalMessage);
		}

		public FixSessionState GetLatestState (string side1, string side2)
		{
			return getStorage (side1, side2).GetSessionState ();
		}

		public long StoreMessage(FixMessage message, MessageDirection direction)
		{
			return getStorage(message.SenderCompId, message.TargetCompId).Push(new MessageToStore(message), direction);
		}

		public void TrackAsSent (string senderCompId, string targetCompId, long messageIdentifier, long seqNum, string sentTimestamp)
		{
			getStorage(senderCompId, targetCompId).TrackAsSent(messageIdentifier, seqNum, sentTimestamp);
		}

		public IEnumerable<KeyValuePair<long, FixMessage>> GetUnsentMessages (string senderCompId, string targetCompId)
		{
			return getStorage(senderCompId, targetCompId).GetUnsentMessages ().Select (x => new KeyValuePair<long, FixMessage> (x.Id, x.Message));
		}

		public void Flush()
		{
			if (flushing)
				return;

			flushing = true;

			try
			{
				foreach (var session in storages)
					session.Value.Flush();
			}
			catch
			{
				throw;
			}
			finally
			{
				flushing = false;
			}
		}

		//public IEnumerable<FixMessage> GetInboundMessages(DateTime? from)
		//{
		//	//TODO: this needs to be done for parsers and replay. can wait
		//	throw new NotImplementedException();
		//}

		//public IEnumerable<FixMessage> GetOutboundMessages(DateTime? from)
		//{
		//	//TODO: this needs to be done for parsers and replay. can wait
		//	throw new NotImplementedException();
		//}

		public long GetLatestInboundSeqNum(string side1, string side2)
		{
			return getStorage (side1, side2).GetSeqNum (MessageDirection.Inbound);
		}

		public long GetLatestOutboundSeqNum(string side1, string side2)
		{
			return getStorage (side1, side2).GetSeqNum (MessageDirection.Outbound);
		}

		public IEnumerable<FixMessage> GetSentMessages(string side1, string side2, long seqNumFrom, long seqNumTo)
		{
			return getStorage (side1, side2).GetSentMessages (seqNumFrom, seqNumTo);
		}

		public void InitSettings(XDocument settingsDocument)
		{
			_settings.Load(settingsDocument);

			if (string.IsNullOrEmpty (_settings.StorageFilesPath))
				_settings.StorageFilesPath = DEFAULT_STORAGE_PATH;

			var assembly = Assembly.GetEntryAssembly();
			string binFileName = _settings.CreateSubFolder ? Path.GetFileNameWithoutExtension (assembly.Location) : string.Empty;
			storageRootPath = Common.PathHelper.MapPath(Path.Combine(_settings.StorageFilesPath, binFileName));

			Trace.WriteLine(string.Format("Storage '{0}' initialized with path:'{1}'.", Name, storageRootPath));
		}

		#endregion

		#region Private Methods

		private int getSessionHash(string side1, string side2)
		{
			return side1.GetHashCode() ^ side2.GetHashCode();
		}

		private SessionStorage getStorage(string side1, string side2)
		{
			var sessionHash = getSessionHash(side1, side2);
			var sessionStorageRootPath = Path.Combine(storageRootPath, sessionHash.ToString());
			return getStorage (sessionHash, sessionStorageRootPath);
		}

		private SessionStorage getStorage(int sessionHash, string sessionStoragePath)
		{
			if (storages == null)
				storages = new ConcurrentDictionary<int, SessionStorage> ();

			if (!storages.ContainsKey (sessionHash))
				storages.TryAdd (sessionHash, new SessionStorage (sessionStoragePath));

			return storages [sessionHash];
		}

		#endregion

	}
}
