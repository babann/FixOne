using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Entities.Exceptions
{
	public class FixSessionConfigurationException : Exception
	{
		public FixSessionConfigurationException(string message)
			: base(message)
		{
		}

		public FixSessionConfigurationException(string message, Exception baseException)
			: base(message, baseException)
		{
		}
	}
}
