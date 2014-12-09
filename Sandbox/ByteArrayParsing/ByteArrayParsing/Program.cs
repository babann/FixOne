using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ByteArrayParsing
{
	class Program
	{
		private static readonly byte[] MSG_START_BT = System.Text.ASCIIEncoding.ASCII.GetBytes("8=F");
		private static readonly byte SOH_BT = (byte)(char)1;

		private const string F = "F";
		private const string msg_start = "8=F";
		private const char soh = (char)1;
		private const char equals = '=';

		private static readonly char [] delimiters = new char[] {soh, equals };

		static void Main(string[] args)
		{
			Parse1(null);
			Parse2(null);

			var string2 = "8=FIX.4.49=5735=049=IntGBIorders56=Broker34=252=20100310-12:50:4510=244"
					+ "8=FIX.4.49=5735=049=IntGBIorders56=Broker34=352=20100310-12:51:1510=243";

			Stopwatch sw = new Stopwatch();

			sw.Start();
			var headers = Parse1(System.Text.ASCIIEncoding.ASCII.GetBytes(string2));
			sw.Stop();
			Console.WriteLine("Elapssed: {0}({1})", sw.ElapsedMilliseconds, sw.ElapsedTicks);

			foreach (var header in headers)
				Console.WriteLine(header.ToString());

			sw.Restart();
			headers = Parse2(System.Text.ASCIIEncoding.ASCII.GetBytes(string2));
			sw.Stop();
			Console.WriteLine("Elapssed: {0}({1})", sw.ElapsedMilliseconds, sw.ElapsedTicks);

			foreach (var header in headers)
				Console.WriteLine(header.ToString());

			Console.ReadLine();
		}

		#region Method1

		internal static IEnumerable<FixMessageHeader> Parse1(byte[] data)
		{
			if (data == null || data.Length == 0)
				return null;

			if (!data.Contains(SOH_BT))
				return null;

			List<FixMessageHeader> result = new List<FixMessageHeader>();
			int startMessagePos = data.FirstIndexOf(MSG_START_BT, 0);
			while (startMessagePos >= 0)
			{
				FixMessageHeader header = new FixMessageHeader();
				int msgLength = 0;

				List<byte> block = new List<byte>();
				int discoverTagsToPass = 3;
				int discoveredTags = 0;
				for (int ii = startMessagePos; ii < data.Length; ii++)
				{
					byte sym = data[ii];
					if (sym != SOH_BT)
						block.Add(sym);
					else
					{
						var str = System.Text.ASCIIEncoding.ASCII.GetString(block.ToArray());
						block.Clear();
						var parts = str.Split('=');
						if (parts == null || parts.Length != 2)
							continue;

						switch (parts[0])
						{
							case "8":
								{
									//TODO: detect version
									discoveredTags++;
								} break;
							case "9":
								{
									msgLength = int.Parse(parts[1]);
									//discoveredTags++;
								} break;
							case "35":
								{
									//discoveredTags++;
								} break;
							case "49":
								{
									header.SenderCompId = parts[1];
									discoveredTags++;
								} break;
							case "56":
								{
									header.TargetCompId = parts[1];
									discoveredTags++;
								} break;
							case "34":
								{
									header.SeqNum = long.Parse(parts[1]);
									discoveredTags++;
								} break;
						}
					}

					if (discoveredTags == discoverTagsToPass)
					{
						result.Add(header);
						break;
					}
				}

				if (msgLength == 0)
					msgLength = 1;

				startMessagePos = data.FirstIndexOf(MSG_START_BT, startMessagePos + msgLength);
			}

			return result;
		}

		#endregion

		#region Method2

		internal static IEnumerable<FixMessageHeader> Parse2(byte[] data)
		{
			if (data == null)
				yield break;

			var str = System.Text.ASCIIEncoding.ASCII.GetString(data);
			var strings = str.Split(new string[] { msg_start }, StringSplitOptions.RemoveEmptyEntries);
			int discoverTagsToPass = 4;

			for (int ii = 0; ii < strings.Length; ii ++)
			{
				var msg = strings[ii].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				
				FixMessageHeader header = new FixMessageHeader();
				header.Version = F + msg[0];
				header.SeqNum = -1;

				
				int discoveredTags = 0;

				for (int jj = 1; jj < msg.Length; jj+=2)
				{
					switch (msg[jj])
					{
						case "49":
							{
								header.SenderCompId = msg[jj + 1];
								discoveredTags++;
							}break;
						case "56":
							{
								header.TargetCompId = msg[jj + 1];
								discoveredTags++;
							}break;
						case "34":
							{
								header.SeqNum = long.Parse(msg[jj + 1]);
								discoveredTags++;
							}break;
						case "35":
							{
								header.MessageType = msg[jj + 1];
								discoveredTags++;
							} break;
					}

					if (discoveredTags == discoverTagsToPass)
					{
						header.SetInnerMessage(msg);
						break;
					}
				}

				if (header.IsValid)
					yield return header;
			}
		}

		#endregion

		#region Class

		internal class FixMessageHeader
		{
			private string[] innerMessage;

			internal string Version
			{
				get;
				set;
			}

			internal long SeqNum
			{
				get;
				set;
			}

			internal string SenderCompId
			{
				get;
				set;
			}

			internal string TargetCompId
			{
				get;
				set;
			}

			internal string MessageType
			{
				get;
				set;
			}

			internal byte[] Data
			{
				get;
				set;
			}

			internal string SessionName
			{
				get
				{
					return string.Format("{0}:{1}", SenderCompId, TargetCompId);
				}
			}

			public override string ToString()
			{
				return string.Format("Version={2};SeqNum={0};SessionName={1}", SeqNum, SessionName, Version);
			}

			internal bool IsValid
			{
				get
				{
					return !string.IsNullOrEmpty(SenderCompId)
						&& !string.IsNullOrEmpty(TargetCompId)
						&& SeqNum >= 0
						&& !string.IsNullOrEmpty(MessageType);
				}
			}

			internal void SetInnerMessage(string[] msg)
			{
				innerMessage = msg;
			}

		}

		#endregion
	}
}
