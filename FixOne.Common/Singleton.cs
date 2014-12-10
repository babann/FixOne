using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using FixOne.Entities.Exceptions;

namespace FixOne.Common
{
	/// <summary>
	/// Implementation of Singleton pattern.
	/// </summary>
	internal static class Singleton<T>
	   where T : class
	{
		#region Private Fields

		/// <summary>
		/// Current isntance
		/// </summary>
		private static T _instance;

		/// <summary>
		/// Threads sync object
		/// </summary>
		private static object _lock = new object();

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current instance.
		/// </summary>
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
								throw new FixEngineException(string.Format("A constructor is missing for '{0}'.", typeof(T).Name));

							try
							{
								_instance = (T)constructor.Invoke(null);
							} catch(Exception exc) {
								throw new FixEngineException ("Singleton initialization failed", exc);
							}

						}
					}
				}

				return _instance;
			}
		}

		#endregion
	}
}
