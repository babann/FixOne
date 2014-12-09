using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Entities
{
	public enum FixSessionState
	{
		Undefined = 0,
		Instanciated = 1,
		WaitingForPair = 2,
		Established = 3,
		Terminated = 4,
		LoggedOut = 5,
		//Error = 200
	}
}
