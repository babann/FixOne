using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Entities.SessionLevelMessages
{
	public class HeartBeatMessage : FixMessage
	{
		public HeartBeatMessage(string testRequestID)
			: base (new FixSessionInfo(), "0")
		{
			if (!string.IsNullOrEmpty(testRequestID))
				SetTagValue(112, testRequestID);
		}
	}
}
