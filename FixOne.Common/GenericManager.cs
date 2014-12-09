using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace FixOne.Common
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T">Modules type</typeparam>
	/// <typeparam name="V">Singleton type</typeparam>
	public class GenericManager<T, V> 
		where  T:Common.Interfaces.IModule
		where V:class
	{
		#region Protected Fields

		protected object writeLocker = new object();

		#endregion

		#region Public Properties

		public IEnumerable<T> AvailableModules
		{
			get;
			private set;
		}

		public IEnumerable<T> EnabledModules
		{
			get;
			private set;
		}

		public static V Instance
		{
			get
			{
				return Common.Singleton<V>.Instance;
			}
		}

		#endregion

		public GenericManager(string path)
		{
			var dlls = System.IO.Directory.GetFiles(PathHelper.MapPath(path), "*.dll");

			List<T> list = new List<T>();

			foreach (var dllName in dlls)
			{
				var dllPath = System.IO.Path.GetFullPath(dllName);
				Assembly assembly = Assembly.LoadFile(dllPath);
				var types = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(T)));

				foreach (var type in types)
					try
					{
						list.Add((T)Activator.CreateInstance(type));
					}
					catch (Exception exc)
					{
						Trace.WriteLine (string.Format ("Can't create instance of '{0}'", type.FullName), "Error");
						Trace.WriteLine (exc);
					}
			}

			AvailableModules = list.ToArray();
		}

		public void EnableModules(params string[] moduleNames)
		{
			setEnabledModules(true, moduleNames);
		}

		public void DisableModules(params string[] moduleNames)
		{
			setEnabledModules(false, moduleNames);
		}

		#region Private Methods

		private void setEnabledModules(bool value, params string[] moduleNames)
		{
			if (AvailableModules == null || !AvailableModules.Any())
				return;

			if (moduleNames == null)
				return; //TODO: maybe throw exception???

			lock (writeLocker)
			{
				foreach (var logger in AvailableModules.Where(module => moduleNames.Contains(module.Name)))
					logger.Enabled = value;
			}

			EnabledModules = AvailableModules.Where(module => module.Enabled).ToArray();
		}

		#endregion
	}
}
