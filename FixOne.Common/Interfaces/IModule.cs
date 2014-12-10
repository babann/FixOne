using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Common.Interfaces
{
	/// <summary>
	/// Engine Module.
	/// </summary>
	public interface IModule
	{
		/// <summary>
		/// Gets the name of the module.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="FixOne.Common.Interfaces.IModule"/> is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		bool Enabled
		{
			get;
			set;
		}
	}
}
