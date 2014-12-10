using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Engine
{
	/// <summary>
	/// Represents byte data array with length field.
	/// </summary>
	internal class ByteDataWithLength
	{
		/// <summary>
		/// Gets or sets the array of bytes.
		/// </summary>
		internal byte[] Data
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the length of data.
		/// </summary>
		internal int Length
		{
			get;
			set;
		}
	}
}
