using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace FixOne.Engine
{
	internal class QueuedStorage : IDisposable
	{
		long idxCounter = 0;
		object idxLocker = new object();
		ConcurrentQueue<ByteDataWithLength> dataQueue = new ConcurrentQueue<ByteDataWithLength>();
		SortedSet<long> gotIndicies = new SortedSet<long>();

		internal void Enqueue(byte[] data, int length)
		{
			lock (idxLocker) {
				if (gotIndicies == null)
					gotIndicies = new SortedSet<long> ();
			}

			Statistics.Latency.TrackPacketReceived (data.GetHashCode (), Entities.MessageDirection.Inbound);
			dataQueue.Enqueue (new ByteDataWithLength () { Data = data, Length = length });
		}

		internal byte[] Dequeue(out long idx)
		{
			ByteDataWithLength value;
			idx = -1;

			lock (idxLocker) {
				if (!dataQueue.TryDequeue (out value))
					return null;

					idx = idxCounter++;

				if(Managers.SettingsManager.Instance.CurrentSettings.DeepDebugMode)
					Managers.EngineLogManager.Instance.Debug("[{2}] Dequeued data {0} :: idx={1}", System.Text.ASCIIEncoding.ASCII.GetString(value.Data.Take(value.Length).ToArray()), idx, Thread.CurrentThread.Name);
				
					gotIndicies.Add (idx);
					return value.Data.Take (value.Length).ToArray ();
			}
		}

		internal bool IsValidOrder(long idx)
		{
			lock (idxLocker) {
				return idx == gotIndicies.Min;
			}
		}

		internal void GoToNextIndex()
		{
			lock (idxLocker)
			{
				gotIndicies.Remove(gotIndicies.Min);
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion
	}
}
