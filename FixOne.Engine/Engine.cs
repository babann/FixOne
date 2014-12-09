using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;
using FixOne.Engine.Managers;
using FixOne.Entities;
using FixOne.Entities.Logging;
using FixOne.Engine.Settings;

namespace FixOne.Engine
{
	/// <summary>
	/// Represents the FIX Engine and it's functions
	/// </summary>
	public class Engine : IDisposable
	{

		#region Private Fields

		bool engineStarted = false;
		bool exit = false;
		private string version;
		//bool disposed;
		Thread listenerThread;
		ConcurrentBag<NetworkSession> sessions = new ConcurrentBag<NetworkSession> ();
		ConcurrentDictionary<string, FixSession> fixSessions = new ConcurrentDictionary<string, FixSession> ();
		ManualResetEvent stopEvent = new ManualResetEvent (true);

		#endregion

		#region Events

		/// <summary>
		/// Occurs before the engine start.
		/// </summary>
		public event EventHandler<Events.EngineEventArgs> BeforeStart;

		/// <summary>
		/// Occurs when the engine started.
		/// </summary>
		public event EventHandler Started;

		/// <summary>
		/// Occurs when before the engine stop.
		/// </summary>
		public event EventHandler<Events.EngineEventArgs> BeforeStop;

		/// <summary>
		/// Occurs when the engine stopped.
		/// </summary>
		public event EventHandler Stopped;

		/// <summary>
		/// Occurs when a new session established.
		/// </summary>
		public event EventHandler<Events.SessionEstablishedEventArgs> SessionEstablished;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the current Engine version.
		/// </summary>
		public string Version {
			get {
				if (string.IsNullOrEmpty (version))
					version = getVersion ();

				return version;
			}
		}

		/// <summary>
		/// Gets the list of configured sessions.
		/// </summary>
		public ICollection<FixSession> Sessions {
			get {
				return fixSessions == null
					? null
					: fixSessions.Values;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this engine is running.
		/// </summary>
		/// <value><c>true</c> if this engine is running; otherwise, <c>false</c>.</value>
		public bool IsRunning {
			get {
				return engineStarted;
			}
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="FixOne.Engine.Engine"/> class.
		/// </summary>
		public Engine ()
		{
			if (SettingsManager.Instance.CurrentSettings.EnabledLoggers != null)
				EngineLogManager.Instance.EnableModules (SettingsManager.Instance.CurrentSettings.EnabledLoggers);

			if (SettingsManager.Instance.CurrentSettings.EnabledStorages != null)
				PersistentStoragesManager.Instance.EnableModules (SettingsManager.Instance.CurrentSettings.EnabledStorages);
		}

		/// <summary>
		/// Starts the engine at a specified IP address
		/// </summary>
		/// <param name="ipAddress">Local IP address</param>
		/// <param name="port">Port to listen for incoming initiators</param>
		/// <param name="numOfWorkersPerSession">Number of parsing/processing working threads per session</param>
		/// <param name="heartBeatInterval">Predefined hearbeat interval (needs t be removed because shuld be per session)</param>
		public void Start ()
		{
			if (engineStarted)
				return;

			if (BeforeStart != null) {
				Events.EngineEventArgs e = new Events.EngineEventArgs ();
				BeforeStart (this, e);
				if (e.Cancel)
					return;
			}

			EngineLogManager.Instance.Start ();
			PersistentStoragesManager.Instance.Start ();
			DictionariesManager.Instance.Start ();
			Statistics.Latency.Start ();

			if (!PersistentStoragesManager.Instance.EnabledModules.Any ())
				EngineLogManager.Instance.Warning ("There are no enabled storages. It would be impossible to restore session.");

			loadPredefinedSessions ();

			loadPrevoiusSessionsStats ();

			#region Init Listener Thread (Acceptor sessions case)

			EngineLogManager.Instance.Info ("Starting incoming connections listener.");

			listenerThread = new Thread (() => {
				int maxPendingConnections = 100;
				IPAddress hostIP = new IPAddress (SettingsManager.Instance.CurrentSettings.InstanceIP);
				IPEndPoint ep = new IPEndPoint (hostIP, SettingsManager.Instance.CurrentSettings.ListenPort);

				TcpListener listener = new TcpListener (ep);
				try {
					listener.Start (maxPendingConnections);
				} catch (Exception exc) {
					EngineLogManager.Instance.Error ("Engine can't start listener because of error", exc);
					return;
				}

				EngineLogManager.Instance.Info ("Listener started at {0}. Waiting for incoming connections.", ep.ToString ());

				while (!exit) {
					if (listener.Pending ()) {
						EngineLogManager.Instance.Info ("Connection pending. Instanciating new network session.");
						var session = new NetworkSession (listener.AcceptSocket ());
						session.MessageDiscovered += new EventHandler<Events.FixMessageEventArgs> (processFirstSessionMessage);

						sessions.Add (session);
						session.Start (SettingsManager.Instance.CurrentSettings.NumerOfWorkersPerSession, SettingsManager.Instance.CurrentSettings.DefaultHeartBeatInterval);
					}

					Thread.Sleep (1);
				}

				EngineLogManager.Instance.Info ("Closing listener...");
				listener.Stop ();
				EngineLogManager.Instance.Info ("Listener closed.");

				stopEvent.Reset ();
			});
			listenerThread.Name = "ACCLISTENER";
			listenerThread.IsBackground = true;
			listenerThread.Start ();

			#endregion

			#region Start Inititator sessions

			EngineLogManager.Instance.InfoIf (fixSessions.Values.Any (session => session.Role == FixSessionRole.Initiator), "Starting sockets for each Initiator sessions.");

			foreach (var sess in fixSessions.Where(s => s.Value.Role == FixSessionRole.Initiator))
				startInitiatorSession (sess);

			#endregion

			engineStarted = true;

			if (Started != null)
				Started (this, EventArgs.Empty);
		}

		/// <summary>
		/// Stop the engine.
		/// </summary>
		public void Stop ()
		{
			if (BeforeStop != null) {
				Events.EngineEventArgs e = new Events.EngineEventArgs ();
				BeforeStop (this, e);
				if (e.Cancel)
					return;
			}

			foreach (var session in fixSessions)
				if (session.Value.State == FixSessionState.Established)
					session.Value.Logout ("Officially stopping the enginge.");

			//need to wait untill all logouts will be sent
			while (fixSessions.Any (session => session.Value.State == FixSessionState.Established))
				Thread.Sleep (100);

			foreach (var session in sessions)
				session.Stop ();

			exit = true;

			stopEvent.WaitOne ();

			engineStarted = false;

			EngineLogManager.Instance.Stop ();
			PersistentStoragesManager.Instance.Stop ();
			Statistics.Latency.Stop ();

			if (Stopped != null)
				Stopped (this, EventArgs.Empty);
		}

		/// <summary>
		/// Creates the initiator FIX session.
		/// </summary>
		/// <returns>New initiator FIX session.</returns>
		/// <param name="version">Protocol version.</param>
		/// <param name="senderCompId">SenderCompId field.</param>
		/// <param name="targetCompId">TargetCompId field.</param>
		/// <param name="heartBeatInterval">Heart beat interval.</param>
		/// <param name="numberOfWorkingThreads">Number of working threads for current session.</param>
		/// <param name="acceptorIp">Acceptor IP address.</param>
		/// <param name="acceptorPort">Acceptor network port.</param>
		/// <param name="allowReconnect">If set to <c>true</c> automatic reconnects are allowed.</param>
		/// <param name="reconnectAttempts">Number of reconnect attempts.</param>
		public FixSession CreateInitiatorSession (FixVersion version,
		                                         string senderCompId,
		                                         string targetCompId,
		                                         int heartBeatInterval,
		                                         int numberOfWorkingThreads,
		                                         byte[] acceptorIp,
		                                         int acceptorPort,
		                                         bool allowReconnect,
		                                         int reconnectAttempts)
		{
			return createSession (version,
				FixSessionRole.Initiator,
				senderCompId,
				targetCompId,
				heartBeatInterval,
				numberOfWorkingThreads,
				null,
				acceptorIp,
				acceptorPort,
				allowReconnect,
				reconnectAttempts,
				true);
		}

		/// <summary>
		/// Creates the acceptor FIX session.
		/// </summary>
		/// <returns>New acceptor FIX session.</returns>
		/// <param name="version">Protocol version.</param>
		/// <param name="senderCompId">SenderCompId field.</param>
		/// <param name="targetCompId">TargetCompId field.</param>
		/// <param name="heartBeatInterval">Heart beat interval.</param>
		/// <param name="numberOfWorkingThreads">Number of working threads for current session.</param>
		public FixSession CreateAcceptorSession (FixVersion version, string senderCompId, string targetCompId, int heartBeatInterval, int numberOfWorkingThreads)
		{
			return createSession (version, FixSessionRole.Acceptor, senderCompId, targetCompId, heartBeatInterval, numberOfWorkingThreads, null, null, 0, false, 0, true);
		}

		/// <summary>
		/// Creates the session.
		/// </summary>
		/// <returns>The session.</returns>
		/// <param name="version">Protocol version.</param>
		/// <param name="role">Acceptor or initiator.</param>
		/// <param name="senderCompId">SenderCompId identifier.</param>
		/// <param name="targetCompId">TargetCompId identifier.</param>
		/// <param name="heartBeatInterval">Heart beat interval.</param>
		/// <param name="numberOfWorkingThreads">Number of working threads.</param>
		/// <param name="storages">Permanet storages for this session.</param>
		/// <param name="acceptorIp">Acceptor IP address (for initiator session only).</param>
		/// <param name="acceptorPort">Acceptor network port (for initiator session only).</param>
		/// <param name="allowReconnect">If set to <c>true</c> allow reconnect.</param>
		/// <param name="reconnectAttempts">Reconnect attempts.</param>
		/// <param name="failIfExists">If set to <c>true</c> fail if session with the same parameters already exists.</param>
		private FixSession createSession (FixVersion version,
		                                 FixSessionRole role,
		                                 string senderCompId,
		                                 string targetCompId,
		                                 int heartBeatInterval,
		                                 int numberOfWorkingThreads,
		                                 string[] storages,
		                                 byte[] acceptorIp,
		                                 int acceptorPort,
		                                 bool allowReconnect,
		                                 int reconnectAttempts,
		                                 bool failIfExists)
		{
			if (string.IsNullOrEmpty (senderCompId))
				throw new ArgumentException ("senderCompId can'b be empty");

			if (string.IsNullOrEmpty (targetCompId))
				throw new ArgumentException ("targetCompId can'b be empty");

			if (heartBeatInterval <= 0)
				heartBeatInterval = SettingsManager.Instance.CurrentSettings.DefaultHeartBeatInterval;

			if (numberOfWorkingThreads <= 0)
				numberOfWorkingThreads = SettingsManager.Instance.CurrentSettings.NumerOfWorkersPerSession;

			if (storages == null || !storages.Any ())
				storages = new string[] { SettingsManager.Instance.CurrentSettings.DefaultStorage };

			if (role == FixSessionRole.Initiator) {
				if (acceptorIp == null || acceptorIp.Length != 4)
					throw new ArgumentException ("Acceptor IP is empty or in wrong format.");

				if (acceptorPort <= 0)
					throw new ArgumentException ("Acceptor port should be greater than zero.");

			}

			var sessionInfo = new FixSessionInfo (
				                  version, 
				                  role, 
				                  senderCompId, 
				                  targetCompId, 
				                  heartBeatInterval, 
				                  numberOfWorkingThreads, 
				                  storages, 
				                  acceptorIp, 
				                  acceptorPort,
				                  allowReconnect,
				                  reconnectAttempts);

			if (fixSessions.ContainsKey (sessionInfo.Name) && failIfExists)
				throw new Entities.Exceptions.FixEngineException ("Session with the same SenderCompId and TargetCompId already exists.");

			var session = new FixSession (sessionInfo);

			fixSessions.AddOrUpdate (session.Name, session, (key, oldValue) => session);

			if (role == FixSessionRole.Initiator && engineStarted)
				startInitiatorSession (new KeyValuePair<string, FixSession> (session.Name, session));

			return session;
		}

		#region IDisposable Members

		/// <summary>
		/// Releases all resource used by the <see cref="FixOne.Engine.Engine"/> object.
		/// </summary>
		void IDisposable.Dispose ()
		{
		}

		#endregion

		/// <summary>
		/// Get version function.
		/// </summary>
		private Func<string> getVersion = () => {
			return System.Diagnostics.FileVersionInfo.GetVersionInfo (
				System.Reflection.Assembly.GetExecutingAssembly ().Location).FileVersion.ToString ();
		};

		private void startInitiatorSession (KeyValuePair<string, FixSession> sess)
		{
			new Thread ((object state) => {
				var session = (KeyValuePair<string, FixSession>)state;

				Thread.CurrentThread.Name = "INIT " + session.Key;
				Thread.CurrentThread.IsBackground = true;

				EngineLogManager.Instance.Info ("Starting socket for session {0}.", session.Value.Name);

				IPAddress remoteIP = new IPAddress (session.Value.AcceptorIP);
				IPEndPoint remoteEp = new IPEndPoint (remoteIP, session.Value.AcceptorPort);

				TcpClient client = new TcpClient ();
				int connectionRetry = 0;
				int allowedReconnects = session.Value.ReconnectAttempts == 0 ? int.MaxValue : session.Value.ReconnectAttempts;
				bool canreconnect = true;
				while (!client.Connected && canreconnect) {
					try {
						client.Connect (remoteEp);
						var nwSession = new NetworkSession (client.Client);
						sessions.Add (nwSession);
						nwSession.Start (session.Value.NumberOfWorkingThreads, session.Value.HeartBeatInterval);
						linkSessions (nwSession, session.Value);
						session.Value.Logon ();
						EngineLogManager.Instance.Info ("{0}: Connection to {1} established.", session.Value.Name, remoteEp.ToString ());
					} catch (Exception exc) {
						EngineLogManager.Instance.Info ("{0}: Can't connect to acceptor because of {1}.", session.Value.Name, exc.Message);
						connectionRetry++;

						canreconnect = session.Value.AllowReconnect && connectionRetry <= allowedReconnects;

						EngineLogManager.Instance.InfoIf (canreconnect, "{0}: AllowReconnect is enabled for session. Attempting to reconnect ({1} of {2}) in {3} sec.",
							session.Value.Name, connectionRetry, allowedReconnects, SettingsManager.Instance.CurrentSettings.InitiatorReconnectDelay);

						EngineLogManager.Instance.InfoIf (!session.Value.AllowReconnect, "{0}: AllowReconnect is disabled for session. Will not reconnect.", session.Value.Name);

						if(canreconnect)
							Thread.Sleep (SettingsManager.Instance.CurrentSettings.InitiatorReconnectDelay * 1000);
					}
				}
			}).Start (sess);
		}

		private void loadPredefinedSessions ()
		{
			EngineLogManager.Instance.Info ("Instanciating predefined sessions - Start.");

			foreach (var session in SettingsManager.Instance.ConfiguredSessions) {
				fixSessions.AddOrUpdate (session.Name, new FixSession (session), (key, oldValue) => new FixSession (session));
				if (session.Storages.Any ()) {
					foreach (var storage in session.Storages) {
						PersistentStoragesManager.Instance.RegisterSession (session.SenderCompId, session.TargetCompId, storage);
						EngineLogManager.Instance.Info ("Storage '{0}' set for session '{1}'.", storage, session.Name);
					}
				} else {
					//use default storage
					var defaultStorage = 
						string.IsNullOrEmpty (SettingsManager.Instance.CurrentSettings.DefaultStorage)
						? (PersistentStoragesManager.Instance.EnabledModules.Any () ? PersistentStoragesManager.Instance.EnabledModules.First ().Name : null)
						: SettingsManager.Instance.CurrentSettings.DefaultStorage;

					if (string.IsNullOrEmpty (defaultStorage)) {
						EngineLogManager.Instance.Warning ("Can's set storage for session '{0}' because there are no enabled storages.", session.Name);
					} else {
						PersistentStoragesManager.Instance.RegisterSession (session.SenderCompId, session.TargetCompId, defaultStorage);
						EngineLogManager.Instance.Info ("Default storage '{0}' set for session '{1}'.", defaultStorage, session.Name);
					}
				}
			}

			EngineLogManager.Instance.Info ("Instanciating predefined sessions - Complete.");
		}

		private void loadPrevoiusSessionsStats ()
		{
			EngineLogManager.Instance.Info ("Load previous sessions stats - Start.");

			foreach (var session in fixSessions) {
				var previousState = PersistentStoragesManager.Instance.GetLatestSessionState (session.Value.SenderCompId, session.Value.TargetCompId);
				if (previousState != FixSessionState.LoggedOut) {
					var inSeqNum = PersistentStoragesManager.Instance.GetRecentSequenceNumber (session.Value.SenderCompId, session.Value.TargetCompId, MessageDirection.Inbound);
					var outSeqNum = PersistentStoragesManager.Instance.GetRecentSequenceNumber (session.Value.SenderCompId, session.Value.TargetCompId, MessageDirection.Outbound);
					EngineLogManager.Instance.Info ("Previous session run was not logged out ({2}). Resoring sequence numbers to IN:{0}; OUT:{1}.", inSeqNum.ToString(), outSeqNum.ToString(), previousState.ToString());
					session.Value.InboundSequenceNumber = inSeqNum;
					session.Value.OutboundSequenceNumber = outSeqNum;
				}
				var unsentMessages = PersistentStoragesManager.Instance.GetUnsentMessages (session.Value.SenderCompId, session.Value.TargetCompId);
				if (unsentMessages != null) {
					EngineLogManager.Instance.Info ("Found {0} unsent messages from previous run of session '{1}'.", unsentMessages.Count ().ToString(), session.Value.Name);
					foreach (var message in unsentMessages)
						session.Value.Send (message);
				} else {
					EngineLogManager.Instance.Info ("No unsent messages found for session '{0}'.", session.Value.Name);
				}
			}

			EngineLogManager.Instance.Info ("Load previous sessions stats - Complete.");
		}

		private void linkSessions (NetworkSession nwSession, FixSession fSession)
		{
			fSession.SessionStateChanged += new EventHandler<Events.SessionStateChangedEventArgs> (processSessionStateChanged);
			fSession.LinkSession (nwSession);
		}

		private void processSessionStateChanged (object s, Events.SessionStateChangedEventArgs ea)
		{
			FixSession fSession = s as FixSession;
			if (s == null)
				return;

			switch (ea.NewState) {
			case FixSessionState.Established:
				{
					if (SessionEstablished != null)
						SessionEstablished (fSession, new Events.SessionEstablishedEventArgs (fSession.Name));
				}
				break;

			case FixSessionState.LoggedOut:
			case FixSessionState.Terminated:
				{
					fSession.SessionStateChanged -= new EventHandler<Events.SessionStateChangedEventArgs> (processSessionStateChanged);
				}
				break;
			}
		}

		private void processFirstSessionMessage (object s, Events.FixMessageEventArgs e)
		{
			var session = s as NetworkSession;
			if (s == null)
				return;

			//to link to existing session we need to reverse incoming session name
			var sessionName = string.Format ("{0}:{1}", e.Message.TargetCompId, e.Message.SenderCompId);
			EngineLogManager.Instance.Info ("[{0}][FIN] : {1}", sessionName, e.Message.ToString ());
			PersistentStoragesManager.Instance.Push (e.Message, MessageDirection.Inbound);

			bool acceptable = false;
			if (fixSessions.ContainsKey (sessionName)) {
				acceptable = true;
			} else {
				EngineLogManager.Instance.InfoIf (
					SettingsManager.Instance.CurrentSettings.AcceptAllInitiators, 
					"Session for incoming message does not exists ({0}) but will be created becase AcceptAllInitiators is ON.", sessionName);
				EngineLogManager.Instance.InfoIf (
					!SettingsManager.Instance.CurrentSettings.AcceptAllInitiators, 
					"Session for incoming message does not exists ({0}) and connection with initiator will be closed.", sessionName);
				if (SettingsManager.Instance.CurrentSettings.AcceptAllInitiators) {
					var fixSession = new FixSession (
						                 e.Message.Version, 
						                 e.Message.TargetCompId, 
						                 e.Message.SenderCompId, 
						                 FixSessionRole.Acceptor, 
						                 SettingsManager.Instance.CurrentSettings.NumerOfWorkersPerSession, 
						                 SettingsManager.Instance.CurrentSettings.DefaultHeartBeatInterval);

					fixSessions.AddOrUpdate (sessionName, fixSession, (key, oldValue) => {
						return oldValue;
					});

					acceptable = true;
				}
			}

			if (acceptable) {
				if (fixSessions [sessionName].State == FixSessionState.WaitingForPair
				    || fixSessions [sessionName].State == FixSessionState.LoggedOut
				    || fixSessions [sessionName].State == FixSessionState.Terminated)
					linkSessions (session, fixSessions [sessionName]);
				else {
					acceptable = false;
					EngineLogManager.Instance.Info ("Incoming session is not accepted because session {0} already established.", sessionName);
				}
			}

			if (acceptable) {
				string failure;
				acceptable = fixSessions [sessionName].ProcessFirstAcceptorMessage (e.Message, out failure);
				if (!acceptable)
					EngineLogManager.Instance.Info ("First session message is not valid because of {0}", failure);
			}

			if (!acceptable) {
				session.Stop ();
			}
			session.MessageDiscovered -= new EventHandler<Events.FixMessageEventArgs> (processFirstSessionMessage);
		}
	}
}
