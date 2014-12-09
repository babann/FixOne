using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

using FixOne.Engine.Managers;
using FixOne.Entities;
using FixOne.Entities.SessionLevelMessages;
using FixOne.Common.Interfaces;

namespace FixOne.Engine
{
	public sealed class FixSession
	{
		#region Private Fields

		FixSessionInfo sessionInfo;
		NetworkSession innerSession;
		internal IProtocolDictionary dictionary;

		long? inSeqToStartWith;
		long? outSeqToStartWith;

		#endregion

		#region Properties

		public FixSessionRole Role
		{
			get { return sessionInfo.Role; }
		}

		public string SenderCompId
		{
			get { return sessionInfo.SenderCompId; }
		}

		public string TargetCompId
		{
			get { return sessionInfo.TargetCompId; }
		}

		public string Name
		{
			get { return sessionInfo.Name;}
		}

		public int HeartBeatInterval
		{
			get { return sessionInfo.HeartBeatInterval; }
		}

		public int NumberOfWorkingThreads
		{
			get { return sessionInfo.NumberOfWorkingThreads; }
		}

		internal byte[] AcceptorIP
		{
			get { return sessionInfo.AcceptorIP; }
		}

		internal int AcceptorPort
		{
			get { return sessionInfo.AcceptorPort; }
		}

		internal bool AllowReconnect
		{
			get { return sessionInfo.AllowReconnect; }
		}

		internal int ReconnectAttempts
		{
			get { return sessionInfo.ReceonnectAttempts; }
		}

		public FixSessionState State
		{
			get;
			private set;
		}

		public string Status
		{
			get;
			private set;
		}

		public long InboundSequenceNumber
		{
			get
			{
				return innerSession != null ? innerSession.InboundSequence
						: inSeqToStartWith.HasValue ? inSeqToStartWith.Value : 0L;
			}
			set
			{
				if (innerSession == null)
					inSeqToStartWith = value;
				else
					if (value > 0)
						innerSession.InboundSequence = value;
			}
		}

		public long OutboundSequenceNumber
		{
			get
			{
				return innerSession != null ? innerSession.OutboundSequence
						: outSeqToStartWith.HasValue ? outSeqToStartWith.Value : 0L;
			}
			set
			{
				if (innerSession == null)
					outSeqToStartWith = value;
				else
					if (value > 0)
						innerSession.OutboundSequence = value;
			}
		}

		#endregion

		#region Events

		public event EventHandler<Events.SessionStateChangedEventArgs> SessionStateChanged;

		#endregion

		internal ConcurrentQueue<StoredMessageHandle> writeQueue = new ConcurrentQueue<StoredMessageHandle>();

		public FixSession(FixSessionInfo info)
		{
			sessionInfo = info;
			dictionary = DictionariesManager.Instance.GetDictionaryByFixVersion(info.Version);

			if (dictionary == null)
			{
				var msg = string.Format("[{0}] No dictionary loaded for protocol version {1}", Name, info.Version);
				EngineLogManager.Instance.Error(msg);
				throw new FixOne.Entities.Exceptions.FixEngineException(msg);
			}

			changeSessionState(sessionInfo.Role == FixSessionRole.Acceptor ? FixSessionState.WaitingForPair : FixSessionState.Instanciated);
		}

		public FixSession(FixVersion version, string senderCompId, string targetCompId, FixSessionRole role, int numOfWorkingThreads, int hearBeatInterval)
			: this(new FixSessionInfo()
			{
				Version = version,
				SenderCompId = senderCompId,
				TargetCompId = targetCompId,
				Role = role,
				NumberOfWorkingThreads = numOfWorkingThreads,
				HeartBeatInterval = hearBeatInterval
			})
		{
		}

		public FixMessage NewMessage(string messageType)
		{
			return new FixMessage(sessionInfo, messageType);
		}

		public void Send(FixMessage message)
		{
			message.AssignSessionInfo(this.sessionInfo);
			var handle = PersistentStoragesManager.Instance.Push(message, MessageDirection.Outbound);
			EngineLogManager.Instance.Debug("[{1}] Adding to the out queue message {0}.", message.ToString(), Name);
			writeQueue.Enqueue(handle);
		}

		internal bool SendImmidiate(FixMessage message, bool setSeqNum)
		{
			message.AssignSessionInfo(this.sessionInfo);
			var handle = PersistentStoragesManager.Instance.Push(message, MessageDirection.Outbound);
			EngineLogManager.Instance.Debug("[{1}] URGENTLY SENDING message {0}.", message.ToString(), Name);
			Exception failure;
			var success = this.innerSession.SendAsap(message, setSeqNum, out failure);
			if (!success) {
				EngineLogManager.Instance.Error ("[{0}] Urgent sending failed because of {1}. Closing session.", Name, failure == null ? "UNSPECIFIED" : failure.Message);
				changeSessionState(FixSessionState.Terminated);
				innerSession.Stop ();
			} else {
				handle.TrackAsSent (message.SeqNum, message.GetTagValue<string> (HeaderTags.SendingTime));
			}

			return success;
		}

		#region Internal Methods

		/// <summary>
		/// Session will be linked only if no other session is linked
		/// </summary>
		/// <param name="netSession"></param>
		internal void LinkSession(NetworkSession netSession)
		{
			if ((State == FixSessionState.Terminated || State == FixSessionState.LoggedOut) && innerSession != null)
			{
				innerSession.Unlink();
				innerSession = null;
			}

			if (innerSession == null)
			{
				innerSession = netSession;
				innerSession.Link(this);

				if (inSeqToStartWith.HasValue)
				{
					innerSession.InboundSequence = inSeqToStartWith.Value;
					inSeqToStartWith = null;
				}

				if (outSeqToStartWith.HasValue)
				{
					innerSession.OutboundSequence = outSeqToStartWith.Value;
					outSeqToStartWith = null;
				}

				innerSession.MessageDiscovered += (object s, Events.FixMessageEventArgs e) =>
					{
						if(SettingsManager.Instance.CurrentSettings.DeepDebugMode)
							EngineLogManager.Instance.Debug("[FXS][{0}] SEQNUM: {1}", Thread.CurrentThread.Name, e.Message.SeqNum.ToString());

						EngineLogManager.Instance.Info("[{1}][IN] {0}", e.Message.ToString(), Name);
						PersistentStoragesManager.Instance.Push(e.Message, MessageDirection.Inbound);
						string failure;
						if (!ProcessMessage(e.Message, out failure))
						{
							EngineLogManager.Instance.Info("[{0}] Message is not valid because {1}.", Name, failure);
						}
					};
				innerSession.MessageSendingFailed += (object s, Events.FixMessageSendingFailed e) =>
					{
						if(this.Role == FixSessionRole.Initiator)
						{
							e.Reconnect = AllowReconnect;
							e.ReconnectAttempts = this.ReconnectAttempts;
							e.ReconnectInterval = SettingsManager.Instance.CurrentSettings.InitiatorReconnectDelay;
						}
					//else
					//	{
					//		changeSessionState(FixSessionState.Terminated);
					//		e.Reconnect = false;
					//		innerSession.Stop();
					//	}

						innerSession.OutboundSequence = e.Message.SeqNum - 1;
					};
				EngineLogManager.Instance.Info("FixSession {0} now linked with a Network Session.", this.Name);
				return;
			}

			EngineLogManager.Instance.Debug("FixSession {0} already linked with Network Session.", this.Name);
		}

		internal void Logon()
		{
			Send(NewMessage("A"));
			if (sessionInfo.Role == FixSessionRole.Initiator)
				changeSessionState(FixSessionState.WaitingForPair);
		}

		/// <summary>
		/// Sends logout message immidiately
		/// </summary>
		/// <param name="text">optional</param>
		internal void Logout(string text)
		{
			var msg = NewMessage("5");
			if (!string.IsNullOrEmpty(text))
				msg.SetTagValue(58, text);
			
			SendImmidiate(msg, true);
			changeSessionState(FixSessionState.LoggedOut);
		}

		internal void SendRejectMessage(string rejectMessage)
		{
			var msg = NewMessage("3");
			if (!string.IsNullOrEmpty(rejectMessage))
				msg.SetTagValue(58, rejectMessage);

			SendImmidiate(msg, true);
			changeSessionState(FixSessionState.Terminated);
		}

		internal void SendResendRequest(long seqFrom, long seqTo)
		{
			var msg = NewMessage("2");
			msg.SetTagValue(7, seqFrom.ToString());
			msg.SetTagValue(16, seqTo.ToString());
			SendImmidiate(msg, true);
		}

		internal bool ProcessMessage(FixMessage message, out string failureMessage)
		{
			failureMessage = string.Empty;

			if (message.SeqNum <= InboundSequenceNumber && ! message.PossDupFlag)
			{
				failureMessage = "Seq num too low.";
				SendRejectMessage(failureMessage);
				return false;
			}

			if (message.SeqNum > InboundSequenceNumber + 1)
			{
				SendResendRequest(InboundSequenceNumber, message.SeqNum);
				failureMessage = "Seq num too hign. Resend request sent.";
				return false;
			}

			switch (message.MessageType)
			{
				case "A":
					{
						if (State == FixSessionState.WaitingForPair)
						{
							//set the state at the initiator side
							if (!validateLogonMessage(message, out failureMessage))
								return false;

							changeSessionState(FixSessionState.Established);
						}
						else
							EngineLogManager.Instance.Warning("[{0}] Logon message in the middle of the session!", Name);
						//TODO: need to do smth in case of ELSE
					} break;
				case "1":
					{   //TEST REQUEST
						var reqId = message.GetTagValue<string>(112);
						Send(new HeartBeatMessage(reqId == null ? null : reqId));
					}break;
				case "2":
					{
						EngineLogManager.Instance.Info("[{0}] Resend Request (Fill Gap) message received.", Name);
						long sequenceFrom = long.Parse(message.GetTagValue<string> (7));
						long sequenceTo = long.Parse(message.GetTagValue<string> (16));
						var messages = PersistentStoragesManager.Instance.GetGapFill(message.SenderCompId, message.TargetCompId, sequenceFrom, sequenceTo);
						if (messages != null)
							foreach (var msg in messages)
							{
								msg.SetTagValue (HeaderTags.PossDupFlag, true);
								SendImmidiate(msg, false);
							}
					}break;
				case "3":
					{
						//EngineLogManager.Instance.Info("[{0}] Reject message received. Stopping session.", Name);
						//innerSession.Stop();

						EngineLogManager.Instance.Info("[{0}] Session Level Reject message received.", Name);
						changeSessionState(FixSessionState.Terminated);
					} break;
				case "5":
					{   //LOGOUT
						EngineLogManager.Instance.Info("[{0}] Logout message received. Stopping session.", Name);
						innerSession.Stop();
						changeSessionState(FixSessionState.LoggedOut);
					}break;
				default:
					{
						if (State == FixSessionState.WaitingForPair)
						{
							failureMessage = "First message is not Logon.";
							SendRejectMessage(failureMessage);
						}
					} break;
			}

			if(string.IsNullOrEmpty(failureMessage))
				innerSession.InboundSequence = message.SeqNum;

			return string.IsNullOrEmpty(failureMessage);
		}

		internal bool ProcessFirstAcceptorMessage(FixMessage message, out string failureMessage)
		{
			//For acceptor only!!!
			failureMessage = string.Empty;

			if (Role != FixSessionRole.Acceptor)
				return false;

			if (!validateLogonMessage(message, out failureMessage))
				return false;

			Logon();

			changeSessionState(FixSessionState.Established); //set the state at the acceptor side
			innerSession.InboundSequence = message.SeqNum;
			return true;
		}

		internal void changeSessionState(FixSessionState state)
		{
			var oldState = State;
			State = state;

			if(oldState == state)
				return;

			PersistentStoragesManager.Instance.TrackSessionState(SenderCompId, TargetCompId, state, Status);

			if (SessionStateChanged != null)
				SessionStateChanged(this, new Events.SessionStateChangedEventArgs(oldState, state));
		}

		#endregion

		private bool validateLogonMessage(FixMessage message, out string failureMessage)
		{
			failureMessage = string.Empty;

			//TODO: add more validation and rules skip configuration

			if (message.Version != sessionInfo.Version)
			{
				failureMessage = string.Format("Message protocol version is {0} - expected is {1}.", message.Version, sessionInfo.Version);
				SendRejectMessage(failureMessage);
				return false;
			}

			if (message.MessageType != "A")
			{
				failureMessage = "First message is not logon.";
				SendRejectMessage(failureMessage);
				return false;
			}

			if (message.SeqNum != InboundSequenceNumber + 1)
			{
				failureMessage = string.Format("Expected SeqNum: {0} - Received: {1}.", (InboundSequenceNumber + 1).ToString(), message.SeqNum.ToString());
				SendRejectMessage(failureMessage);
				return false;
			}

			return true;
		}
	}
}
