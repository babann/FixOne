using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FixOne.Entities;

namespace FixOne.Engine.Events
{
	public class SessionStateChangedEventArgs : EventArgs
	{
		public FixSessionState OldState
		{
			get;
			private set;
		}

		public FixSessionState NewState
		{
			get;
			private set;
		}

		internal SessionStateChangedEventArgs(FixSessionState oldState, FixSessionState newState)
		{
			OldState = oldState;
			NewState = newState;
		}
	}
}
