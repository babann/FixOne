using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FixOne.Entities;

namespace FixOne.Engine.Events
{
	internal class FixMessageSendingFailed : FixMessageEventArgs
	{
		internal bool Reconnect
		{
			get;
			set;
		}

		internal int ReconnectAttempts
		{
			get;
			set;
		}

		internal int ReconnectInterval
		{
			get;
			set;
		}

		internal Exception Exc
		{
			get;
			private set;
		}

		internal FixMessageSendingFailed(FixMessage message, Exception exc)
			: base(message)
		{
			Exc = exc;
		}
	}
}
