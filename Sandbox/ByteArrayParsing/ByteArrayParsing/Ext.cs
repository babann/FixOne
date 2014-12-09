using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteArrayParsing
{
	public static class Ext
	{
		public static int FirstIndexOf(this byte[] source, byte[] pattern, int startIndex)
		{
			if (source == null || pattern == null || source.Length == 0 || pattern.Length == 0 || source.Length < startIndex)
				return -1;

			for (int ii = startIndex; ii < source.Length; ii++)
			{
				if (source[ii] == pattern[0])
				{
					int matches = 1;
					for (int jj = 1; jj < pattern.Length; jj++)
					{
						if (source[ii + jj] != pattern[jj])
							break;
						matches++;
					}

					if (matches == pattern.Length)
						return ii;
				}
			}

			return -1;
		}
	}
}
