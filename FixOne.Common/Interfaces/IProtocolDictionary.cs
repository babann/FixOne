using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FixOne.Entities;

namespace FixOne.Common.Interfaces
{
	public interface IProtocolDictionary : IModule
	{
		FixVersion SupportedVersion
		{
			get;
		}

		IEnumerable<FixMessageHeader> ParseHeader(byte[] rawData);

		IEnumerable<FixMessage> ParseAndValidate(byte[] rawData);

		IEnumerable<FixMessage> ParseDetails(params FixMessageHeader[] headers);

		IEnumerable<FixMessage> ParseDetails(params FixMessage[] messages);

	}
}
