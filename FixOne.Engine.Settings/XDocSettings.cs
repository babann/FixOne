using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using System.IO;

namespace FixOne.Engine.Settings
{
	public abstract class XDocSettings
	{

		#region Constants

		private const string ROOT_NAME = "FixOne";
		private const string PATH_DELIM_STR = "\\";

		#endregion

		#region Protected Fields

		protected XDocument internalDocument;

		#endregion

		#region Protected Properties

		protected string RootElementName {
			get;
			private set;
		}

		#endregion

		#region Constructor

		protected XDocSettings (string root)
		{
			Contract.Requires (!string.IsNullOrEmpty (root));

			internalDocument = new XDocument (new XElement (ROOT_NAME));
			internalDocument.Root.Add (new XElement (root));
			RootElementName = root;

			//init with defaults
			foreach (var propertyDef in GetType().GetProperties()) {
				var attributes = propertyDef.GetCustomAttributes (typeof(XSettingAttribute), false);
				if (attributes == null || attributes.Length == 0)
					continue;

				foreach (var attrib in attributes) {
					XSettingAttribute xattribute = attrib as XSettingAttribute;
					propertyDef.SetValue (this, xattribute.Default, null);
				}
			}

		}

		#endregion

		#region Public Interface

		public void LoadFrom (string fileName)
		{
			Contract.Requires (!string.IsNullOrEmpty (fileName) && !string.IsNullOrWhiteSpace (fileName));

			if (!File.Exists (fileName))
				throw new Entities.Exceptions.FixSessionConfigurationException (string.Format ("Configuration file '{0}' not found.", fileName));

			try {
				XDocument doc = XDocument.Load (fileName);
				Load (doc);
			} catch (Exception exc) {
				throw new Entities.Exceptions.FixSessionConfigurationException (
					string.Format ("Configuration load from file {0} failed.", fileName),
					exc);
			}
		}

		public void AddOrReplaceSection (XElement element)
		{
			if (internalDocument.Root.Element (element.Name) == null)
				internalDocument.Root.Add (element);
			else
				internalDocument.Root.Element (element.Name).ReplaceWith (element);
		}

		public void Save (string filename)
		{
			var rootElement = internalDocument.Root.Element (RootElementName);
			if (rootElement == null) {
				rootElement = new XElement (RootElementName);
				internalDocument.Root.Add (rootElement);
			}

			foreach (var propertyDef in GetType().GetProperties()) {
				var attributes = propertyDef.GetCustomAttributes (typeof(XSettingAttribute), false);
				if (attributes == null || attributes.Length == 0)
					continue;

				foreach (var attrib in attributes) {
					XSettingAttribute xattribute = attrib as XSettingAttribute;
					var value = propertyDef.GetValue (this, null);

					var targetElement = findTargetElement (rootElement, xattribute.ElementPath, true);
					if (string.IsNullOrEmpty (xattribute.AttributeName))
						targetElement.SetValue (xattribute.Parser == null ? value : xattribute.Parser.Encode (value));
					else
						targetElement.SetAttributeValue (xattribute.AttributeName, xattribute.Parser == null ? value : xattribute.Parser.Encode (value));
				}
			}

			internalDocument.Save (filename);
		}

		public void Load (XDocument document)
		{
			if (document == null)
				throw new ArgumentException ("Document is null.");

			if (document.Root == null || document.Root.Name != ROOT_NAME)
				throw new ArgumentException ("Document root is null or document is in incorrect format.");

			internalDocument = document;
			var rootElement = internalDocument.Root.Element (RootElementName);

			foreach (var propertyDef in GetType().GetProperties()) {
				var attributes = propertyDef.GetCustomAttributes (typeof(XSettingAttribute), false);
				if (attributes == null || attributes.Length == 0)
					continue;

				foreach (var attrib in attributes) {
					XSettingAttribute xattribute = attrib as XSettingAttribute;
					var value = readOrDefault (rootElement, xattribute, propertyDef.PropertyType.IsArray);
					propertyDef.SetValue (this, value, null);
				}
			}
		}

		public void MergeSettings (IEnumerable<XElement> settings)
		{
			if (settings == null)
				return;

			if (internalDocument == null)
				throw new ApplicationException ("Settings document not initialized.");

			var rootElement = internalDocument.Root.Element (RootElementName);
			foreach (var newElement in settings) {
				var existingChilds = rootElement.Elements ().Where (x => (x.Name == newElement.Name)).ToList ();
				foreach (var child in existingChilds)
					child.Remove ();

				rootElement.Add (newElement);
			}
		}

		#endregion

		#region Private Methods

		private XElement findTargetElement (XElement parent, string path, bool create)
		{
			if (string.IsNullOrEmpty (path))
				return parent;

			var parts = path.Split (PATH_DELIM_STR.ToCharArray (), StringSplitOptions.RemoveEmptyEntries);
			if (parts == null || parts.Length == 0)
				return null;

			var elem = parent.Element (parts [0]);
			if (create && elem == null) {
				elem = new XElement (parts [0]);
				parent.Add (elem);
			}
			if (elem != null && parts.Length > 1)
				return findTargetElement (elem, string.Join (PATH_DELIM_STR, parts.Except (new string[] { parts [0] }).ToArray ()), create);

			return elem;
		}

		private object readOrDefault (XElement parentElement, XSettingAttribute definition, bool isArray)
		{
			if (parentElement == null)
				return definition.Default;

			var targetElement = findTargetElement (parentElement, definition.ElementPath, false);
			if (targetElement == null)
				return definition.Default;

			string targetValue = null;

			if (!string.IsNullOrEmpty (definition.AttributeName)) {
				var targetAttribute = targetElement.Attribute (definition.AttributeName);
				targetValue = targetAttribute == null ? null : targetAttribute.Value;
			} else {
				targetValue = isArray ? string.Join ("", targetElement.Elements ().Select (e => e.ToString ())) : targetElement.Value;
			}

			if (string.IsNullOrEmpty (targetValue))
				return definition.Default;

			if (!string.IsNullOrEmpty (targetValue))
				return definition.Parser == null ? targetValue : definition.Parser.Parse (targetValue);

			return definition.Default;
		}

		#endregion

	}
}
