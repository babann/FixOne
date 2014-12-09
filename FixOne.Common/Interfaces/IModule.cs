using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Common.Interfaces
{
	public interface IModule
	{
		string Name
		{
			get;
		}

		bool Enabled
		{
			get;
			set;
		}
	}
}
