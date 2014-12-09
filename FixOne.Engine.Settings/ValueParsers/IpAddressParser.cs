using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace FixOne.Engine.Settings.ValueParsers
{
	public class IpAddressParser : IValueParser
	{
		#region IValueParser Members

		public Type ResultType
		{
			get { return typeof(byte[]); }
		}

		public object Parse(string address)
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

		public string Encode(object value)
		{
			Contract.Requires(value != null);
			Contract.Requires(value.GetType() == typeof(byte[]));

			var address = value as byte[];

			if (address == null || address.Length != 4)
				return null; //TODO: maybe throw exception??

			return string.Join(".", address);
		}

		public bool AreEquals(object value1, object value2)
		{
			return value1 is byte[] && value2 is byte[] && value1.Equals(value2);
		}

		#endregion
	}
}
