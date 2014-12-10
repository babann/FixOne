using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace FixOne.Common
{
	/// <summary>
	/// Generic manages. Used as a base class for all modules namagers.
	/// </summary>
	/// <typeparam name="T">Modules type</typeparam>
	/// <typeparam name="V">Singleton type</typeparam>
	public class GenericManager<T, V> 
		where  T:Common.Interfaces.IModule
		where V:class
	{
		#region Private fields

		private string _modulesPath;
		private List<T> _availableModules;

		#endregion

		#region Protected Fields

		protected object writeLocker = new object();

		#endregion

		#region Public Properties

		public IEnumerable<T> AvailableModules
		{
			get {
				return _availableModules;
			}
		}

		public IEnumerable<T> EnabledModules
		{
			get{
				return _availableModules.Where (x => x.Enabled);
			}
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
			_modulesPath = path;
			_availableModules = new List<T> ();
		}

		public void EnableModules(params string[] moduleNames)
		{
			setEnabledModules(true, moduleNames);
		}

		public void DisableModules(params string[] moduleNames)
		{
			setEnabledModules(false, moduleNames);
		}

		public virtual void Init ()
		{
			var dlls = System.IO.Directory.GetFiles(PathHelper.MapPath(_modulesPath), "*.dll");

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

			_availableModules = list;
		}

		#region Private Methods

		private void setEnabledModules(bool value, params string[] moduleNames)
		{
			if (_availableModules == null || !_availableModules.Any())
				return;

			if (moduleNames == null)
				return; //TODO: maybe throw exception???

			lock (writeLocker)
			{
				foreach (var logger in _availableModules.Where(module => moduleNames.Contains(module.Name)))
					logger.Enabled = value;
			}
		}

		#endregion

		#region Proteted Methods

		protected bool AddModule(T module, bool replace)
		{
			var modules = _availableModules;
			var idx = -1;
			if(modules.Any(x => x.Name.Equals(module.Name)))
				idx = modules.FindIndex(x => x.Name.Equals(module.Name));

			if (idx >= 0) {
				if (replace)
					modules.RemoveAt (idx);
				else
					return false;
			}

			modules.Add(module);

			lock(writeLocker)
			{
				_availableModules = modules;
			}

			return true;
		}
		#endregion
	}
}
