using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Common.Interfaces
{
	public interface IFixDictionary
	{
		//TODO:

		Entities.FixVersion SupportedVersion
		{
			get;
		}

		bool IsValidMessage(Entities.FixMessage message);

		Entities.FixMessage CretaeMessage(string messageType);
	}
}
