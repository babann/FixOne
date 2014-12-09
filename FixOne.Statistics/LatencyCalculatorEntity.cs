using System;

namespace FixOne.Statistics
{
	internal class LatencyCalculatorEntity
	{
		internal DateTime StartedAt{ get; private set;}
		internal long MessageIdx { get; private set;}
		internal string SenderCompId { get; private set;}
		internal string TargetCompId { get; private set;}
		internal long SeqNum{ get; set;}
		internal DateTime FinishedAt { get; private set;}
		internal Entities.MessageDirection Direction { get; private set;}

		internal LatencyCalculatorEntity (long messageIdx, Entities.MessageDirection direction, DateTime startedAt)
		{
			StartedAt = startedAt;
			MessageIdx = messageIdx;
			Direction = direction;
		}

		internal LatencyCalculatorEntity (string senderCompId, string targetCompId, long seqNum, long messageIdx)
		{
			SenderCompId = senderCompId;
			TargetCompId = targetCompId;
			SeqNum = seqNum;
			MessageIdx = messageIdx;
		}

		internal LatencyCalculatorEntity(string senderCompId, string targetCompId, long seqNum, DateTime finishedAt)
		{
			SenderCompId = senderCompId;
			TargetCompId = targetCompId;
			SeqNum = seqNum;
			FinishedAt = finishedAt;
		}

		internal void UpdateFromLevel2(LatencyCalculatorEntity entity)
		{
			SenderCompId = SenderCompId;
			TargetCompId = TargetCompId;
			SeqNum = SeqNum;
		}

		internal void UpdateFromLevel3(LatencyCalculatorEntity entity)
		{
			FinishedAt = entity.FinishedAt;
		}

		internal double CalculateLatency()
		{
			return (FinishedAt - StartedAt).TotalMilliseconds;
		}
	}
}

