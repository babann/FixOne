using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Engine.Events
{
	public class SessionEstablishedEventArgs : EventArgs
	{
		public string SessionName
		{
			get;
			private set;
		}

		public SessionEstablishedEventArgs(string sessionName)
			: base()
		{
			SessionName = sessionName;
		}
	}
}
