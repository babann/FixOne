using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Entities.Exceptions
{
	public class FixEngineException : Exception
	{
		public FixEngineException(string message)
			: base(message)
		{
		}

		public FixEngineException(string message, Exception baseException)
			: base(message, baseException)
		{
		}
	}
}
