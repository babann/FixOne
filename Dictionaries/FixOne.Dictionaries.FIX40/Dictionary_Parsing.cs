using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FixOne.Entities;
using FixOne.Common.Interfaces;

namespace FixOne.Dictionaries.FIX40
{
	public partial class Dictionary : IProtocolDictionary
	{
		#region Constants

		//private const string F = "F";
		//private const string MSG_START = "8=F";
		//private const char EQUALS = '=';
		//private static readonly char[] DELIMITERS = new char[] { Common.Constants.SOH, EQUALS };

		#endregion

		#region IModule Members

		public string Name
		{
			get { return "FIX 4.0 Dictionary"; }
		}

		public bool Enabled { get; set; }

		#endregion

		#region IProtocolDictionary Members

		public FixVersion SupportedVersion
		{
			get { return FixVersion.FIX40; }
		}

		public IEnumerable<FixMessageHeader> ParseHeader(byte[] rawData)
		{
			return FixOne.Parsers.Generic.FixMessageHeaderParser.ParseHeaders(rawData, null);
			//if (rawData == null || rawData.Length == 0)
			//{
			//    yield break;
			//}

			//var dataAsString = System.Text.ASCIIEncoding.ASCII.GetString(rawData);
			//if (!dataAsString.Contains(Common.Constants.SOH))
			//{
			//    yield break;
			//}

			//var splitMessages = dataAsString.Split(new string[] { MSG_START }, StringSplitOptions.RemoveEmptyEntries);
			//if (splitMessages == null || splitMessages.Length == 0)
			//{
			//    yield break;
			//}

			//int discoverTagsToPass = 5;
			//int startMsgIdx = 0;

			//for (int ii = 0; ii < splitMessages.Length; ii++)
			//{
			//    var msg = splitMessages[ii].Split(DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
			//    var version = F + msg[0];
			//    var fixVersion = FixOne.Parsers.Generic.FixMessageHeaderParser.ConvertStringToVersion(version);

			//    FixMessageHeader header = new FixMessageHeader();
			//    header.SeqNum = -1;
			//    if (fixVersion.HasValue)
			//        header.Version = fixVersion.Value;

			//    int discoveredTags = 0;
			//    int bodyLength = 0;

			//    for (int jj = 1; jj < msg.Length; jj += 2)
			//    {
			//        switch (msg[jj])
			//        {
			//            case Common.Constants.Tags.SENDER_COMP_ID:
			//                {
			//                    header.SenderCompId = msg[jj + 1];
			//                    discoveredTags++;
			//                } break;
			//            case Common.Constants.Tags.TARGET_COMP_ID:
			//                {
			//                    header.TargetCompId = msg[jj + 1];
			//                    discoveredTags++;
			//                } break;
			//            case Common.Constants.Tags.SEQ_NUM:
			//                {
			//                    header.SeqNum = long.Parse(msg[jj + 1]);
			//                    discoveredTags++;
			//                } break;
			//            case Common.Constants.Tags.MSG_TYPE:
			//                {
			//                    header.MessageType = msg[jj + 1];
			//                    discoveredTags++;
			//                } break;
			//            case Common.Constants.Tags.BODY_LENGTH:
			//                {
			//                    header.BodyLength = int.Parse(msg[jj + 1]);
			//                    discoveredTags++;
			//                } break;
			//        }

			//        if (discoveredTags == discoverTagsToPass)
			//        {
			//            //byte[] messageBytes = new byte[splitMessages[ii].Length];
			//            //rawData.CopyTo(messageBytes, 
			//            //header.SetInnerMessage(msg);
			//            //header.SetBody(null);
			//            break;
			//        }
			//    }

			//    if (header.IsValid)
			//        yield return header;
			//}
		}

		public IEnumerable<FixMessage> ParseAndValidate(byte[] rawData)
		{
			//TODO:
			return null;
		}

		public IEnumerable<FixMessage> ParseDetails(params FixMessageHeader[] headers)
		{
			return null;
			//TODO:
		}

		public IEnumerable<FixMessage> ParseDetails(params FixMessage[] messages)
		{
			return null;
			//TODO:
		}

		#endregion
	}
}
