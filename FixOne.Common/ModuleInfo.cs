using System;

namespace FixOne.Common
{
	/// <summary>
	/// Represents module information stored in GenericManager.
	/// </summary>
	public class ModuleInfo<T>
	{
		/// <summary>
		/// Gets the instane of the module.
		/// </summary>
		public T Module {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this module instance is enabled.
		/// </summary>
		public bool IsEnabled
		{
			get;
			set;
		}

		public ModuleInfo (T module)
		{
			Module = module;
		}
	}
}

