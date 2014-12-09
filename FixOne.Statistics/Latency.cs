using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;

namespace FixOne.Statistics
{
	public static class Latency
	{
		public static double IncomingLatency {
			get;
			private set;
		}

		public static double OutgoingLatency {
			get;
			private set;
		}

		private static bool _started;
		private static bool _exit;
		private static Thread _calculatorThread;
		private static ConcurrentQueue<LatencyCalculatorEntity> _level1List = new ConcurrentQueue<LatencyCalculatorEntity> ();
		private static List<LatencyCalculatorEntity> _level2List = new List<LatencyCalculatorEntity> ();
		private static List<LatencyCalculatorEntity> _level3List = new List<LatencyCalculatorEntity> ();

		static Latency ()
		{
			IncomingLatency = OutgoingLatency = 0;
		}

		public static void Start ()
		{
			if (_started)
				return;

			_started = true;
			_exit = false;

			_calculatorThread = new Thread (() => {
				while (!_exit) {
					LatencyCalculatorEntity entity = null;
					bool matched = false;
					if (_level1List.TryDequeue (out entity) && entity != null) {
						List<LatencyCalculatorEntity> level2all = null;
						lock (_level2List) {
							level2all = _level2List.Where (x => x != null && x.MessageIdx == entity.MessageIdx).ToList ();
						}
						foreach (var level2 in level2all) {
							List<LatencyCalculatorEntity> level3all = null;
							lock (_level3List) {
								level3all = _level3List.Where (x => x != null && x.SenderCompId == level2.SenderCompId && x.TargetCompId == level2.TargetCompId && x.SeqNum == level2.SeqNum).ToList ();
							}
							foreach (var level3 in level3all) {
								matched = true;
								entity.UpdateFromLevel2 (level2);
								entity.UpdateFromLevel3 (level3);
								_level2List.Remove (level2);
								_level3List.Remove (level3);
								switch (entity.Direction) {
								case  Entities.MessageDirection.Inbound:
									IncomingLatency = (IncomingLatency + entity.CalculateLatency ()) / 2;
									break;
								case Entities.MessageDirection.Outbound:
									OutgoingLatency = (OutgoingLatency + entity.CalculateLatency ()) / 2;
									break;	
								}
							}
						}
					}
					if (!matched && entity != null)
						_level1List.Enqueue (entity);
				}
			});

			_calculatorThread.IsBackground = true;
			_calculatorThread.Name = "LATCALC";
			_calculatorThread.Start ();
		}

		public static void Stop ()
		{
			_exit = true;
			_started = false;
			Thread.Sleep (100);
			if (_calculatorThread != null && _calculatorThread.IsAlive) {
				_calculatorThread.Abort ();
				_calculatorThread.Join (100);
			}
		}

		public static void TrackPacketReceived (long packetIdx, Entities.MessageDirection direction)
		{
			return;
			_level1List.Enqueue (new LatencyCalculatorEntity (packetIdx, direction, DateTime.Now));
		}

		public static void TrackHeaderParsed (string senderCompId, string targetCompId, long seqNum, long packetIdx)
		{
			return;
			lock (_level2List) {
				_level2List.Add (new LatencyCalculatorEntity (senderCompId, targetCompId, seqNum, packetIdx));
			}
		}

		public static void TrackMessageParsed (string senderCompId, string targetCompId, long seqNum)
		{
			return;
			var now = DateTime.Now;
			lock (_level3List) {
				_level3List.Add (new LatencyCalculatorEntity (senderCompId, targetCompId, seqNum, now));
			}
		}
	}
}

