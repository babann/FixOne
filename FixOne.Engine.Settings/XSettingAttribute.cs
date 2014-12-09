using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace FixOne.Engine.Settings
{
	[System.AttributeUsage (System.AttributeTargets.Property)]
	public class XSettingAttribute : Attribute
	{
		internal object Default {
			get;
			set;
		}

		internal string ElementPath {
			get;
			set;
		}

		internal string AttributeName {
			get;
			set;
		}

		internal ValueParsers.IValueParser Parser {
			get;
			private set;
		}

		public XSettingAttribute (string elementPath, string attributeName, object defaultValue)
		{
			ElementPath = elementPath;
			AttributeName = attributeName;
			Default = defaultValue;
		}

		public XSettingAttribute (string elementPath, string attributeName, Type parserType, object defaultValue)
			: this (elementPath, attributeName, defaultValue)
		{
			ValueParsers.IValueParser parser = null;

			if (parserType != null) {
				parser = Activator.CreateInstance (parserType) as ValueParsers.IValueParser;

				if (parser != null && defaultValue != null && parser.ResultType != defaultValue.GetType ())
					throw new ApplicationException ("Type mismatch");
			}
			
			Parser = parser;
		}
	}
}
