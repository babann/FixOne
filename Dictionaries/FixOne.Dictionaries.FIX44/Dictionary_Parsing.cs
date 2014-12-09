using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FixOne.Entities;
using FixOne.Common.Interfaces;

namespace FixOne.Dictionaries.FIX44
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
			get { return "FIX 4.4 Dictionary"; }
		}


		public bool Enabled { get; set; }

		#endregion

		#region IProtocolDictionary Members

		public FixVersion SupportedVersion
		{
			get { return FixVersion.FIX44; }
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

			//int msgStartIdx = 0;
			//var splitMessages = dataAsString.Split(new string[] { MSG_START }, StringSplitOptions.RemoveEmptyEntries);
			//if (splitMessages == null || splitMessages.Length == 0)
			//{
			//    yield break;
			//}

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
			//    int checksum = -1;
			//    int headerLength = version.Length + 1;
			//    bool headerComplete = false;
			//    msgStartIdx = dataAsString.IndexOf(MSG_START, msgStartIdx);

			//    for (int jj = 1; jj < msg.Length; jj += 2)
			//    {
			//        switch (msg[jj])
			//        {
			//            case Common.Constants.Tags.SENDER_COMP_ID:
			//                {
			//                    header.SenderCompId = msg[jj + 1];
			//                    headerLength += (msg[jj + 1].Length + msg[jj].Length);
			//                    discoveredTags++;
			//                } break;
			//            case Common.Constants.Tags.TARGET_COMP_ID:
			//                {
			//                    header.TargetCompId = msg[jj + 1];
			//                    headerLength += (msg[jj + 1].Length + msg[jj].Length);
			//                    discoveredTags++;
			//                } break;
			//            case Common.Constants.Tags.SEQ_NUM:
			//                {
			//                    header.SeqNum = long.Parse(msg[jj + 1]);
			//                    headerLength += (msg[jj + 1].Length + msg[jj].Length);
			//                    discoveredTags++;
			//                } break;
			//            case Common.Constants.Tags.MSG_TYPE:
			//                {
			//                    header.MessageType = msg[jj + 1];
			//                    headerLength += (msg[jj + 1].Length + msg[jj].Length);
			//                    discoveredTags++;
			//                } break;
			//            case Common.Constants.Tags.BODY_LENGTH:
			//                {
			//                    header.BodyLength = int.Parse(msg[jj + 1]);
			//                    headerLength += (msg[jj + 1].Length + msg[jj].Length);
			//                    discoveredTags++;
			//                } break;
			//            case "115":
			//            case "128":
			//            case "90":
			//            case "91":
			//            case "50":
			//            case "142":
			//            case "57":
			//            case "143":
			//            case "116":
			//            case "144":
			//            case "129":
			//            case "145":
			//            case "43":
			//            case "97":
			//            case "52":
			//            case "122":
			//            case "212":
			//            case "213":
			//            case "347":
			//            case "369":
			//            case "627":
			//            case "628":
			//            case "629":
			//            case "630":
			//                {
			//                    headerLength += (msg[jj + 1].Length + msg[jj].Length);
			//                }break;
			//            default:
			//                {
			//                    headerComplete = true;
			//                    //header is complete and body started
			//                }break;
			//        }

			//        if(headerComplete)
			//        {
			//            //trying to get checksum
			//            if (msg[msg.Length - 2] == Common.Constants.Tags.CHECKSUM)
			//            {
			//                if (int.TryParse(msg[msg.Length - 1], out checksum))
			//                {
			//                    int reminder = -1;
			//                    bool isValidChecksum = Math.DivRem(header.BodyLength, 256, out reminder) == checksum;
			//                }
			//            }

			//            byte[] bodyBytes = new byte[header.BodyLength];
			//            Array.Copy(rawData, msgStartIdx + headerLength, bodyBytes, 0, header.BodyLength);
			//            //header.Data = bodyBytes;
			//            //header.SetInnerMessage(msg);
			//            header.SetBody(bodyBytes);
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
