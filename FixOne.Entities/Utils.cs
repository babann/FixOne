using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Entities
{
	internal static class Utils
	{
		public static byte[] ParseIp(string address)
		{
			if (string.IsNullOrEmpty(address))
				return null;

			string[] segments = address.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			if (segments == null || segments.Length != 4)
				return null;

			byte[] ip = new byte[4];
			for (int ii = 0; ii < 4; ii++)
			{
				byte value;
				if (!byte.TryParse(segments[ii], out value))
					return null;

				ip[ii] = value;
			}

			return ip;
		}

		public static string SaveIp(byte[] address)
		{
			if (address == null || address.Length != 4)
				return null; //TODO: maybe throw exception??

			return string.Join(".", address);
		}
	}
}
