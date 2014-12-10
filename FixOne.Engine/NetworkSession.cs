using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using FixOne.Entities;
using FixOne.Engine.Managers;

namespace FixOne.Engine
{
	/// <summary>
	/// Represents the Network Session - base communication layer.
	/// </summary>
	internal sealed class NetworkSession
	{

		#region Fields

		bool exit = false;
		bool initiated = false;
		bool suspended = false;
		Socket underlayingSocket;
		Thread readerThread;
		Thread writerThread;
		DateTime lastRecivedData = DateTime.Now;
		DateTime lastSentData = DateTime.Now;
		FixSession linkedSession;
		MessagesProcessor processor = new MessagesProcessor ();

		#region lockers

		object writeLocker = new object ();
		//used in writerThread and heartbeatThread
		object sequenceLocker = new object ();
		//to generate sequence consequently

		#endregion

		internal long InboundSequence = 0;
		internal long OutboundSequence = 0;

		#endregion

		internal QueuedStorage Storage {
			get;
			private set;
		}

		internal FixOne.Common.Interfaces.IProtocolDictionary Dictionary {
			get {
				return linkedSession == null ? null : linkedSession.dictionary;
			}
			set {
				if (linkedSession != null && value != null)
					linkedSession.dictionary = value;
			}
		}

		#region Events

		internal event EventHandler<Events.FixMessageEventArgs> MessageDiscovered;
		internal event EventHandler<Events.FixMessageSendingFailed> MessageSendingFailed;

		#endregion

		#region Public Interface

		internal NetworkSession (Socket socket)
		{
			underlayingSocket = socket;
		}

		internal void Start (int numOfWorkerThreads, int heartbeatInterval)
		{
			if (initiated)
				return;

			initiated = true;

			Storage = new QueuedStorage ();

			if (numOfWorkerThreads <= 0)
				numOfWorkerThreads = 1;

			processor.MessageDiscovered += (object s, Events.FixMessageEventArgs e) => {
				if(SettingsManager.Instance.CurrentSettings.DeepDebugMode)
					EngineLogManager.Instance.Debug("[NWS][{0}] SEQNUM: {1}", Thread.CurrentThread.Name, e.Message.SeqNum.ToString());
				if (MessageDiscovered != null)
					MessageDiscovered (this, e);
			};

			processor.Start (numOfWorkerThreads, this);

			#region Init Reader Thread

			readerThread = new Thread (() => {
				byte[] buffer = new byte[underlayingSocket.ReceiveBufferSize];

				while (!exit) {
					if (!suspended && underlayingSocket.Connected) {
						int bytesRead = -1;
						try {
							if (exit)
								break;

							bytesRead = underlayingSocket.Receive (buffer);
						} catch (SocketException exc) {
							if (!exit)
								EngineLogManager.Instance.Error (string.Format ("[{0}] Error while trying to read data from connection.", linkedSession == null ? "UNKNOWN" : linkedSession.Name), exc);
							else
								EngineLogManager.Instance.Info ("[{0}] Reading failed because session exited.", linkedSession == null ? "UNKNOWN" : linkedSession.Name);
						}

						if (bytesRead > 0) {
							lastRecivedData = DateTime.Now;
							Storage.Enqueue ((byte[])buffer.Clone (), bytesRead);
						}
						
						int heartBtInt = linkedSession == null ? heartbeatInterval : linkedSession.HeartBeatInterval;
						if ((DateTime.Now - lastRecivedData).TotalSeconds > heartBtInt) {
							EngineLogManager.Instance.Warning ("[{0}] Session is interrupted because last data was received more than {1} sec ago. Stopping session.", 
								linkedSession == null ? "UNKNOWN" : linkedSession.Name,
								heartbeatInterval);

							if (linkedSession != null) {
								linkedSession.changeSessionState (FixSessionState.Terminated);
								Stop ();
							}
						}
					}

					Thread.Sleep (1);
				}

				EngineLogManager.Instance.Info ("[{0}] Closing connection.", linkedSession == null ? "UNKNOWN" : linkedSession.Name);
				if (underlayingSocket != null && underlayingSocket.Connected)
					underlayingSocket.Close ();

				EngineLogManager.Instance.Info ("[{0}] Connection closed.", linkedSession == null ? "UNKNOWN" : linkedSession.Name);
				Unlink ();
			});
			readerThread.Name = "SOCKREADER";
			readerThread.IsBackground = true;
			readerThread.Start ();

			#endregion

			#region Init Write Thread

			writerThread = new Thread (() => {
				while (!exit) {
					if (underlayingSocket == null || !underlayingSocket.Connected || linkedSession == null || suspended) {
						Thread.Sleep (100);
						continue;
					}

					StoredMessageHandle handle;

					if (!linkedSession.writeQueue.TryDequeue (out handle)) {
						if (heartbeatInterval > 0 && (DateTime.Now - lastSentData).TotalSeconds >= heartbeatInterval)
							handle = PersistentStoragesManager.Instance.Push (linkedSession.NewMessage ("0"), MessageDirection.Outbound); //TODO: move session level message types to constants (or to dictionary???)
					}

					if (handle != null) {
						Exception failure = null;

						lock (writeLocker) {
							var sent = doSend (handle.Message, true, out failure);
							if (sent)
								handle.TrackAsSent (handle.Message.SeqNum, handle.Message.GetTagValue<string> (HeaderTags.SendingTime));
							else {
								EngineLogManager.Instance.Error ("[{0}] Sending failed because of {1}. Closing session.", linkedSession.Name, failure == null ? "UNSPECIFIED" : failure.Message);
								linkedSession.changeSessionState (FixSessionState.Terminated);
								Stop ();
							}
						}
					}

					Thread.Sleep (1);
				}
			});
			writerThread.Name = "SOCKWRT";
			writerThread.IsBackground = true;
			writerThread.Start ();

			#endregion
		}

		internal void Stop ()
		{
			exit = true;

			if (underlayingSocket != null) {
				underlayingSocket.Close (100);
				underlayingSocket.Dispose ();
				underlayingSocket = null;
			}
		}

		internal bool SendAsap (FixMessage message, bool setSeqNum, out Exception failure)
		{
			bool sent = false;
			failure = null;

			if (message != null) {
				lock (writeLocker) {
					sent = doSend (message, setSeqNum, out failure);
				}
			}

			return sent;
		}

		internal void Link (FixSession fSession)
		{
			lastSentData = DateTime.Now;
			linkedSession = fSession;
		}

		internal void Unlink ()
		{
			linkedSession = null;
		}

		#endregion

		#region Private Methods

		private bool doSend (FixMessage message, bool setSeqNum, out Exception failure)
		{
			failure = null;

			if (message == null)
				return true;

			bool canExit = false;
			bool msgSent = false;

			if (setSeqNum)
				message.SeqNum = getNextOutboundSequence ();

			byte[] data = message.Serialize ();

			while (!canExit) {
				try {
					if(underlayingSocket != null) {
						underlayingSocket.Send (data);
						lastSentData = DateTime.Now;
						EngineLogManager.Instance.Info ("[{1}][OUT] {0}", message.ToString (), linkedSession.Name);
						canExit = msgSent = true;
					}
				} catch (SocketException sendExc) {
					EngineLogManager.Instance.Error ("[{0}] Error while trying to send the message into session.", linkedSession.Name);
					if (!operateSendingFailure (message, sendExc)) {
						canExit = true;
						failure = sendExc;
					} else
						EngineLogManager.Instance.Info ("[{0}] Attempting to resend.", linkedSession.Name);
				}
			}

			return msgSent;
		}

		private long getNextOutboundSequence ()
		{
			lock (sequenceLocker) {
				long seq = OutboundSequence + 1;
				Interlocked.Exchange (ref OutboundSequence, seq);
				return seq;
			}
		}

		private bool operateSendingFailure (FixMessage message, Exception exc)
		{
			if (MessageSendingFailed != null) {
				suspended = true;

				var args = new Events.FixMessageSendingFailed (message, exc);
				MessageSendingFailed (this, args);

				if (args.Reconnect && args.ReconnectAttempts > 0) {
					int reconnectAmt = 0;

					while (!underlayingSocket.Connected && reconnectAmt <= args.ReconnectAttempts) {
						try {
							EngineLogManager.Instance.Info ("[{0}] Attemting to reconnect ({1}).", linkedSession.Name, reconnectAmt.ToString());
							underlayingSocket.Connect (underlayingSocket.RemoteEndPoint);
						} catch (Exception excConnect) {
							EngineLogManager.Instance.Info ("[{0}] Reconnect attemp failed ({1}).", linkedSession.Name, excConnect.Message);
							reconnectAmt++;
							Thread.Sleep (1000 * args.ReconnectInterval);
						}
					}
				} else {
					EngineLogManager.Instance.Info ("[{0}] AllowReconnect is not set for the session and session will be terminated.", linkedSession.Name);
					exit = true;
				}
			} else
				exit = true;

			suspended = false;

			return (underlayingSocket.Connected && !exit);
		}

		#endregion

	}
}
