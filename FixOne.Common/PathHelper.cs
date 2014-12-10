using System;
using System.Diagnostics;

namespace FixOne.Common
{
	public static class PathHelper
	{
		public static string MapPath(string path)
		{
			if (null == path)
				throw new ArgumentException ("PathHelper - Path can't be null.");

			var assembly = System.Reflection.Assembly.GetEntryAssembly ();
			var runningPath = assembly == null ? Environment.CurrentDirectory : System.IO.Path.GetDirectoryName(assembly.Location);

			if (string.IsNullOrWhiteSpace (path))
				return runningPath;

			if(path.StartsWith(@"\\"))
				{
				//network path
				return path;
				}

			if (path.Length > 2 && (path.Substring (1, 2) == @":\" || path.StartsWith("/"))) {
				//full path
				return path;
			}
				//relative path
			if (path.StartsWith(@"\"))
				path = path.Substring(1);
			return System.IO.Path.Combine(runningPath, path);
		}
	}
}

