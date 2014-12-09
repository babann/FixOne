using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Engine.Events
{
	internal class FixMessageInternalEventArgs : FixMessageEventArgs
	{
		//TODO: need to add flags is messge valid and functions (delegates)
		internal bool IsAccepted
		{
			get;
			set;
		}

		internal FixMessageInternalEventArgs(FixOne.Entities.FixMessage message)
			: base(message)
		{
			IsAccepted = true;
		}
	}
}
