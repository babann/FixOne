using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Entities
{
	public class FixMessageHeader
	{
		private bool isParsed = false;

		public FixVersion Version
		{
			get;
			set;
		}

		public long SeqNum
		{
			get;
			set;
		}

		public string SenderCompId
		{
			get;
			set;
		}

		public string MessageType
		{
			get;
			set;
		}

		public string TargetCompId
		{
			get;
			set;
		}

		public int BodyLength
		{
			get;
			set;
		}

		public string SessionName
		{
			get
			{
				return string.Format("{0}:{1}", SenderCompId, TargetCompId);
			}
		}

		internal byte[] Data
		{
			get;
			set;
		}

		internal bool IsValid
		{
			get
			{
				return !string.IsNullOrEmpty(SenderCompId)
					&& !string.IsNullOrEmpty(TargetCompId) 
					&& !string.IsNullOrEmpty(MessageType)
					&& (!isParsed || (isParsed && SeqNum >= 0))
					&& (!isParsed || (isParsed && BodyLength > 0))
					&& (!isParsed || (isParsed && Data != null));
			}
		}

		public FixMessageHeader()
		{
		}

		internal FixMessageHeader(bool isparsed)
		{
			isParsed = isparsed;
		}
	}
}
