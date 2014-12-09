using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Common
{
	public class Constants
	{
		public const char SOH = (char)1;
		
		public class Tags
		{
			public const string MSG_START = "8";
			public const string SEQ_NUM = "34";
			public const string MSG_TYPE = "35";
			public const string SENDER_COMP_ID = "49";
			public const string TARGET_COMP_ID = "56";
			public const string BODY_LENGTH = "9";
			public const string CHECKSUM = "10";
		}
	}
}
