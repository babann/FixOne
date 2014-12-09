using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Engine.Events
{
	public class EngineEventArgs : EventArgs
	{
		public bool Cancel
		{
			get;
			set;
		}
	}
}
