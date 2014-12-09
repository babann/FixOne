using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FixOne.Common
{
	internal static class Singleton<T>
	   where T : class
	{
		private static T _instance;
		private static object _lock = new object();

		internal static T Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lock)
					{
						if (_instance == null)
						{
							ConstructorInfo constructor = null;

							try
							{
								constructor = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.Instance,
									Type.DefaultBinder, new Type[0], new ParameterModifier[0]);
							}
							catch
							{
								throw;
							}

							if (constructor == null || constructor.IsAssembly)
								// Also exclude internal constructors.
								throw new Exception(string.Format("A constructor is missing for '{0}'.", typeof(T).Name));

							_instance = (T)constructor.Invoke(null);
						}
					}
				}

				return _instance;
			}
		}
	}
}
