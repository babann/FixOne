using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FixOne.Entities;
using FixOne.Entities.Logging;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace FixOne.Parsers.Generic
{
	public static class FixMessageHeaderParser
	{
		#region Constants

		private const string F = "F";
		private const string MSG_START = "8=F";
		private const char EQUALS = '=';
		private static readonly char[] DELIMITERS = new char[] { Common.Constants.SOH, EQUALS };

		#endregion

		private static ConcurrentDictionary<string, List<FixVersion>> descriptions;
		private static ConcurrentDictionary<FixVersion, string> values;

		public static FixVersion? ConvertStringToVersion(string versionString)
		{
			var enumValues = Enum.GetValues(typeof(FixVersion));

			if (descriptions == null)
			{
				descriptions = new ConcurrentDictionary<string, List<FixVersion>>();
				foreach (var value in enumValues)
				{
					FieldInfo fi = value.GetType().GetField(value.ToString());
					DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

					if (attributes != null && attributes.Length > 0)
					{
						if (descriptions.ContainsKey(attributes[0].Description))
							descriptions[attributes[0].Description].Add((FixVersion)value);
						else
							descriptions.AddOrUpdate(attributes[0].Description, new List<FixVersion>(new FixVersion[] { (FixVersion)value }), (k, v) => {v.Add((FixVersion)value); return v;});
					}
				}
			}

			if (descriptions.ContainsKey(versionString))
				return descriptions[versionString].First();

			return null;
		}

		public static string ConvertVersionToString(FixVersion version)
		{
			var enumValues = Enum.GetValues(typeof(FixVersion));
			if (values == null)
			{
				values = new ConcurrentDictionary<FixVersion, string>();
				foreach (var value in enumValues)
				{
					FieldInfo fi = value.GetType().GetField(value.ToString());
					DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

					if (attributes != null && attributes.Length > 0)
					{
						if (!values.ContainsKey((FixVersion)value))
							values.AddOrUpdate((FixVersion)value, attributes[0].Description, (k, v) => {return v;});
					}
				}
			}

			if (values.ContainsKey(version))
				return values[version];

			return null;
		}

		public static string ParseFixVersion(byte[] data, Func<LogMessage, bool> notity)
		{
			if (data == null || data.Length == 0)
			{
				if(notity != null)
					DoNotyfy(LogMessageLevel.Warning, "Received data is empty. Parsing cancelled.", notity);
				return null;
			}

			var dataAsString = System.Text.ASCIIEncoding.ASCII.GetString(data);
			if (!dataAsString.Contains(Common.Constants.SOH))
			{
				if(notity != null)
					DoNotyfy(LogMessageLevel.Warning, "Received data does not contains SOH. Parsing cancelled.", notity);
				return null;
			}

			var splitMessages = dataAsString.Split(new string[] { MSG_START }, StringSplitOptions.RemoveEmptyEntries);
			if (splitMessages == null || splitMessages.Length == 0)
			{
				if(notity != null)
					DoNotyfy(LogMessageLevel.Warning, "No messages can be discovered because splitted data is empty. Parsing cancelled.", notity);
				return null;
			}

			if(notity != null)
				DoNotyfy (LogMessageLevel.Debug, string.Format ("Discovered splitted blocks count {0}.", splitMessages.Length.ToString()), notity);

			List<string> detectedVersions = new List<string>();

			for (int ii = 0; ii < splitMessages.Length; ii++)
			{
				var msg = splitMessages[ii].Split(DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
				var version = F + msg[0];
				detectedVersions.Add(version);
				if(notity != null)
					DoNotyfy (LogMessageLevel.Debug, string.Format ("Protocol version discovered '{0}'.", version), notity);
			}

			if (detectedVersions == null || !detectedVersions.Any ()) {
				if(notity != null)
					DoNotyfy (LogMessageLevel.Warning, "No messages can be discovered because splitted data does not contains message start stags.", notity);
				return null;
			}

			if (detectedVersions.Any (version => version != detectedVersions.First ())) {
				if(notity != null)
					DoNotyfy (LogMessageLevel.Warning, "Parsing of current block cancelled because multiple protocol versions detected in one packet.", notity);
				return null;
			}

			return detectedVersions.First();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="notity">It should return bool to PROBABLY cancel message parsing by any condition</param>
		/// <returns></returns>
		public static IEnumerable<FixMessageHeader> ParseHeaders(byte[] data, Func<LogMessage, bool> notity)
		{
			if (data == null || data.Length == 0)
			{
				if(notity != null)
					DoNotyfy(LogMessageLevel.Warning, "Received data is empty. Parsing cancelled.", notity);
				yield break;
			}

			//TODO: add detection of XML and if so then parse as FIXML

			var dataAsString = System.Text.ASCIIEncoding.ASCII.GetString(data);
			if (!dataAsString.Contains(Common.Constants.SOH))
			{
				if(notity != null)
					DoNotyfy(LogMessageLevel.Warning, "Received data does not contains SOH. Parsing cancelled.", notity);
				yield break;
			}

			int msgStartIdx = -1;
			var splitMessages = dataAsString.Split(new string[] { MSG_START }, StringSplitOptions.RemoveEmptyEntries);
			if (splitMessages == null || splitMessages.Length == 0)
			{
				if(notity != null)
					DoNotyfy(LogMessageLevel.Warning, "No messages can be discovered because splitted data is empty. Parsing cancelled.", notity);
				yield break;
			}

			int discoverTagsToPass = 5;

			for (int ii = 0; ii < splitMessages.Length; ii++)
			{
				msgStartIdx = dataAsString.IndexOf(MSG_START, msgStartIdx + 1);

				var msg = splitMessages[ii].Split(DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
				var version = F + msg[0];
				var fixVersion = FixOne.Parsers.Generic.FixMessageHeaderParser.ConvertStringToVersion(version);

				if (!fixVersion.HasValue) {
					if(notity != null)
						DoNotyfy (LogMessageLevel.Warning, string.Format ("No version match found for string '{0}'. Parsing of current splitted block is cancelled.", version), notity);
					break;
				}

				if(notity != null)
					DoNotyfy (LogMessageLevel.Debug, string.Format ("Version match found {0} for string '{1}'.", fixVersion.Value, version), notity);

				FixMessageHeader header = new FixMessageHeader(true);
				header.SeqNum = -1;

				//HasValue validation already made
				header.Version = fixVersion.Value;

				int discoveredTags = 0;
				int headLength = MSG_START.Length + version.Length;

				for (int jj = 1; jj < msg.Length; jj += 2)
				{
					switch (msg[jj])
					{
						case Common.Constants.Tags.SENDER_COMP_ID:
							{
								header.SenderCompId = msg[jj + 1];
								discoveredTags++;
								if(notity != null)
									DoNotyfy (LogMessageLevel.Debug, string.Format ("Discovered SENDER_COMP_ID={0}", header.SenderCompId), notity);
							} break;
						case Common.Constants.Tags.TARGET_COMP_ID:
							{
								header.TargetCompId = msg[jj + 1];
								discoveredTags++;
								if(notity != null)
									DoNotyfy (LogMessageLevel.Debug, string.Format ("Discovered TARGET_COMP_ID={0}", header.TargetCompId), notity);
							} break;
						case Common.Constants.Tags.SEQ_NUM:
							{
								header.SeqNum = long.Parse(msg[jj + 1]);
								discoveredTags++;
								if(notity != null)
									DoNotyfy (LogMessageLevel.Debug, string.Format ("Discovered SEQ_NUM={0}", header.SeqNum.ToString()), notity);
							} break;
						case Common.Constants.Tags.MSG_TYPE:
							{
								header.MessageType = msg[jj + 1];
								discoveredTags++;
								if(notity != null)
									DoNotyfy (LogMessageLevel.Debug, string.Format ("Discovered MSG_TYPE={0}", header.MessageType), notity);
							} break;
						case Common.Constants.Tags.BODY_LENGTH:
							{
								header.BodyLength = int.Parse(msg[jj + 1]);
								discoveredTags++;
								headLength += msg[jj].Length + msg[jj + 1].Length + 1; //1 = length of Common.Constants.SOH
								if(notity != null)
									DoNotyfy (LogMessageLevel.Debug, string.Format ("Discovered BODY_LENGTH={0} (head length={1})", header.BodyLength.ToString(), headLength.ToString()), notity);
							} break;
					}

					if (discoveredTags == discoverTagsToPass)
					{
						int bytesToCopy = header.BodyLength + Common.Constants.Tags.CHECKSUM.Length + msg[msg.Length - 1].Length + 1 + 1;
						//adding checksum, SOH and EQUALS length

						byte[] bodyBytes = new byte[bytesToCopy];
						int sourceIndex = msgStartIdx + headLength + 1;

						if(notity != null)
							DoNotyfy (LogMessageLevel.Debug, string.Format ("Calculated bytes to copy={0}. Data lenght={1}. Copy data from={2}.", bytesToCopy, data.Length.ToString(), sourceIndex.ToString()), notity);

						if (data.Length < sourceIndex + bodyBytes.Length) {
							if(notity != null)
								DoNotyfy (LogMessageLevel.Error, "There is something wrong with received data. Data length is less then discovered message length.", notity);
							break;
						}

						Array.Copy(data, sourceIndex, bodyBytes, 0, bytesToCopy);

						//if (msg[msg.Length - 2] == Common.Constants.Tags.CHECKSUM)
						//{
						//    //trying to get checksum
						//    int checksum;
						//    if (int.TryParse(msg[msg.Length - 1], out checksum))
						//    {
						//        int sum = 0;
						//        for (int kk = 0; kk < bodyBytes.Length; sum += bodyBytes[kk++]) ;
						//        bool isValidChecksum = sum % 256 == checksum;
						//    }
						//}

						header.Data = bodyBytes;
						break;
					}
				}

				if (header.IsValid) {
					if(notity != null)
						DoNotyfy (LogMessageLevel.Debug, "Parsed header is valid.", notity);
					yield return header;
				}
				else
					if(notity != null)
						DoNotyfy(LogMessageLevel.Warning, "Message discovered only partially and will not be processed.\n" + splitMessages[ii], notity);
			}
		}

		#region Private Functions

		private static bool DoNotyfy(LogMessageLevel level, string msg, Func<LogMessage, bool> notify)
		{
			if (notify == null)
				return true;

			return notify(new LogMessage(level, msg));
		}

		#endregion
	}
}
