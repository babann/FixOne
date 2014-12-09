using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using FixOne.Engine.Managers;
using FixOne.Entities;
using FixOne.Entities.Logging;
using System.Collections.Concurrent;

namespace FixOne.Engine
{
	internal sealed class MessagesProcessor : IDisposable
	{
		#region Private Fields

		Thread [] processorThreads;
		Thread outputThread;
		bool exit = false;
		private NetworkSession parentSession;
		private Queue<FixMessageHeader> _processedHeaders = new Queue<FixMessageHeader>();
		
		#endregion

		#region Events

		internal EventHandler<Events.FixMessageEventArgs> MessageDiscovered;

		#endregion

		private FixMessageHeader dequeueProcessedHeader()
		{
			if (!exit) {
				lock (_processedHeaders) {
					if (!exit) {
						var header = _processedHeaders.Any () ? _processedHeaders.Dequeue () : null;
						if(header != null && SettingsManager.Instance.CurrentSettings.DeepDebugMode)
							EngineLogManager.Instance.Debug("[IN][{0}] OUT DEQUEUE: {1}", Thread.CurrentThread.Name, header.SeqNum.ToString());
						return header;
					}
				}
			}
			return null;
		}

		#region Public interface

		internal void Start(int numOfWorkerThreads, NetworkSession parent)
		{
			if (numOfWorkerThreads <= 0)
				numOfWorkerThreads = 1;

			parentSession = parent;

			outputThread = new Thread (() => {
				while(!exit)
				{
					long lastSeqNum = -1;
					FixMessageHeader header = null;
					while((header = dequeueProcessedHeader()) != null)
					{
						if(lastSeqNum < header.SeqNum)
						{
							lastSeqNum = header.SeqNum;
							var msg = new FixMessage(header);

							if(SettingsManager.Instance.CurrentSettings.DeepDebugMode)
								EngineLogManager.Instance.Debug("[IN][{0}] Message: {1}", Thread.CurrentThread.Name, msg.ToString());

							Statistics.Latency.TrackMessageParsed(msg.SenderCompId, msg.TargetCompId, msg.SeqNum);

							if (MessageDiscovered != null && !exit)
								MessageDiscovered(this, new Events.FixMessageEventArgs(msg));
						}
						else
							if(SettingsManager.Instance.CurrentSettings.DeepDebugMode)
								EngineLogManager.Instance.Debug("[IN][{0}] DUP SEQNUM: {1}", Thread.CurrentThread.Name, header.SeqNum.ToString());
					}
					Thread.Sleep(1);
				}
			});
			outputThread.Name = "MSGOUT";
			outputThread.IsBackground = true;
			outputThread.Start ();

			processorThreads = new Thread[numOfWorkerThreads];
			for (int ii = 0; ii < numOfWorkerThreads; ii++)
			{
				processorThreads[ii] = new Thread(() =>
					{
						while (!exit)
						{
							long idx = -1;
							byte[] data = parentSession.Storage.Dequeue(out idx);
							if (data != null)
							{
								if(SettingsManager.Instance.CurrentSettings.DeepDebugMode)
									EngineLogManager.Instance.Debug("[{0}] Dequeue block idx: {1}", Thread.CurrentThread.Name, idx.ToString());

								IEnumerable<FixMessageHeader> messageHeaders = null;

								if(SettingsManager.Instance.CurrentSettings.DeepDebugMode)
								{
									string threadNameFormatted = string.Format("[{0}][blk{1}] ", Thread.CurrentThread.Name, idx.ToString());
									messageHeaders = FixOne.Parsers.Generic.FixMessageHeaderParser.ParseHeaders(data, (LogMessage msg) =>
										{
											msg.PrependMessage(threadNameFormatted);
											EngineLogManager.Instance.LogEntry(msg);
											return true;
										}).ToList();
								}
								else
									messageHeaders = FixOne.Parsers.Generic.FixMessageHeaderParser.ParseHeaders(data, null);

								if (!messageHeaders.Any())
								{
									EngineLogManager.Instance.Warning("[{0}] No FIX messages found in a packet.", Thread.CurrentThread.Name);
									continue;
								}

								if(SettingsManager.Instance.CurrentSettings.DeepDebugMode)
									EngineLogManager.Instance.Debug("[{0}] {1} FIX message(s) found in a packet.", Thread.CurrentThread.Name, messageHeaders.Count());

								while (!parentSession.Storage.IsValidOrder(idx))
								{
									if(SettingsManager.Instance.CurrentSettings.DeepDebugMode)
										EngineLogManager.Instance.Debug("[{0}] Block {1} processed but waiting for previous block to be processed.", Thread.CurrentThread.Name, idx.ToString());
									Thread.Sleep(1);
								}

								if(SettingsManager.Instance.CurrentSettings.DeepDebugMode)
									EngineLogManager.Instance.Debug("[{0}] Processed block idx: {1}", Thread.CurrentThread.Name, idx.ToString());

								var hash = data.GetHashCode();

								lock(_processedHeaders)
								{
									foreach (var header in messageHeaders)
									{
										if(SettingsManager.Instance.CurrentSettings.DeepDebugMode)
											EngineLogManager.Instance.Debug("[{0}][blk{1}] Putting message to processed queue (SEQNUM: {2}).", Thread.CurrentThread.Name, idx.ToString(), header.SeqNum.ToString());
									
										Statistics.Latency.TrackHeaderParsed(header.SenderCompId, header.TargetCompId, header.SeqNum, hash);
										_processedHeaders.Enqueue(header);
									}
								}

								parentSession.Storage.GoToNextIndex();
							}
							
							Thread.Sleep(1);
						}
					});

				processorThreads[ii].Name = "WP_" + ii;
				processorThreads[ii].IsBackground = true;
				processorThreads[ii].Start();

				EngineLogManager.Instance.Info("Worker process {0} started.", processorThreads[ii].Name);
			}
		}

		internal void Stop()
		{
			exit = true;
			Thread.Sleep (100);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion

	}
}
