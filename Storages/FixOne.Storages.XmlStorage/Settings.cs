using System;
using FixOne.Engine.Settings;

namespace FixOne.Storages.XmlStorage
{
	internal class Settings : XDocSettings
	{
		[XSetting("Settings", "path", null)]
		public string StorageFilesPath
		{
			get;
			set;
		}

		[XSetting("Settings", "createSubFolder", typeof(FixOne.Engine.Settings.ValueParsers.BoolParser), true)]
		public bool CreateSubFolder
		{
			get;
			set;
		}

		internal Settings (string sectionName)
			:base(sectionName)
		{
		}
	}
}

