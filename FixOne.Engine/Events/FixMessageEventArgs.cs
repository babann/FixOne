using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FixOne.Entities;

namespace FixOne.Engine.Events
{
	public class FixMessageEventArgs : EventArgs
	{
		public FixMessage Message
		{
			get;
			private set;
		}

		public FixMessageEventArgs(FixMessage message)
		{
			Message = message;
		}
	}
}
