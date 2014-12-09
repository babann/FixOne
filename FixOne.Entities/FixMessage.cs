using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace FixOne.Entities
{
	public class FixMessage
	{
		private const string MESSAGE_DATE_FORMAT = "yyyyMMdd-hh:mm:ss";
		private const string HEAD_FORMAT_STRING = "8=FIX.4.49={0}";
		private const string CHECKSUM_FORMAT_STRING = "10={0}";
		private const string PAIR_FORMAT = "{0}={1}";

		#region Private Fields

		private FixSessionInfo sessionInfo;
		private byte[] body;
		private Dictionary<int, object> tags;
		private bool isChanged;
		private int? checksum;

		#endregion

		#region Public Properties

		public FixVersion Version {
			get {
				return sessionInfo == null ? default(FixVersion) : sessionInfo.Version;
			}
		}

		public string SenderCompId {
			get {
				return readTag (HeaderTags.SenderCompId, null) as string;
				//return sessionInfo == null ? null : sessionInfo.SenderCompId;
			}
		}

		public string TargetCompId {
			get {
				return readTag (HeaderTags.TargetCompId, null) as string;
				//return sessionInfo == null ? null : sessionInfo.TargetCompId;
			}
		}

		public string MessageType {
			get {
				return readTag (HeaderTags.MessageType, null) as string;
			}
			private set {
				fillTag (HeaderTags.MessageType, value);
			}
		}

		public long SeqNum {
			get {
				var val = readTag (HeaderTags.SeqNum, 0L).ToString ();
				return long.Parse (val);
			}
			set {
				fillTag (HeaderTags.SeqNum, value.ToString ());
			}
		}

		public bool PossDupFlag {
			get {
				var val = readTag (HeaderTags.PossDupFlag, false).ToString ();
				return bool.Parse (val);
			}
			//set {
			//	if (value)
			//		fillTag (HeaderTags.PossDupFlag, bool.TrueString);
			//	else
			//		fillTag (HeaderTags.PossDupFlag, null);
			//}
		}

		public int ChechSum {
			get {
				string cs = readTag (10, null) as string;
				if (cs != null)
					return int.Parse (cs);

				if (!checksum.HasValue) {
					if (body == null)
						return 0;

					if (checksum.HasValue)
						return checksum.Value;

					var sum = 1;
					for (int ii = 0; ii < body.Length; sum += body [ii++])
						;

					var headStr = string.Format (HEAD_FORMAT_STRING, body.Length);
					var head = System.Text.ASCIIEncoding.ASCII.GetBytes (headStr);
					for (int ii = 0; ii < head.Length; sum += head [ii++])
						;

					checksum = sum % 256;
				}

				return checksum.Value;
			}
		}

		public string SessionName {
			get {
				return sessionInfo == null ? null : sessionInfo.Name;
			}
		}

		#endregion

		#region Constructor

		private FixMessage ()
		{
			tags = new Dictionary<int, object> ();
		}

		internal FixMessage (FixSessionInfo sessionInfo, string msgType)
			: this ()
		{
			this.sessionInfo = sessionInfo;
			MessageType = msgType;

			fillTag (HeaderTags.SenderCompId, sessionInfo.SenderCompId);
			fillTag (HeaderTags.TargetCompId, sessionInfo.TargetCompId);
			fillTag (HeaderTags.MessageType, msgType);
		}

		/// <summary>
		/// Construct message from header data loaded from session or storage
		/// </summary>
		/// <param name="header"></param>
		public FixMessage (FixMessageHeader header)
			: this ()
		{
			Contract.Requires (header != null);

			sessionInfo = new FixSessionInfo (header);
			SeqNum = header.SeqNum;
			MessageType = header.MessageType;

			fillTag (HeaderTags.SenderCompId, sessionInfo.SenderCompId);
			fillTag (HeaderTags.TargetCompId, sessionInfo.TargetCompId);
			fillTag (HeaderTags.MessageType, header.MessageType);

			body = header.Data;

			parseTagsFromBody ();
		}

		#endregion

		#region Internal (Engine) Methods

		internal void AssignSessionInfo (FixSessionInfo sessionInfo)
		{
			this.sessionInfo = sessionInfo;
			fillTag (HeaderTags.SenderCompId, sessionInfo.SenderCompId);
			fillTag (HeaderTags.TargetCompId, sessionInfo.TargetCompId);
		}

		internal T GetTagValue<T> (int tag)
		{
			return (T)readTag (tag, default(T));
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Set value for specified tag.
		/// <b>If value is null - tag will be removed.</b>
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="value"></param>
		public void SetTagValue (int tag, object value)
		{
			if (value == null) {
				//remove tag from the dictionary
				if (tags.ContainsKey (tag))
					tags.Remove (tag);
				return;
			}

			fillTag (tag, value);
		}

		public override string ToString ()
		{
			if (body == null)
				prepareBodyForSending ();

			return string.Format ("{0}{1}{2}", 
				string.Format (HEAD_FORMAT_STRING, calculateLength ()), 
				System.Text.ASCIIEncoding.ASCII.GetString (body),
				tags.ContainsKey (10) ? null : string.Format (CHECKSUM_FORMAT_STRING, ChechSum));

			//return string.Format("8=FIX.4.49={5}35={3}49={1}56={2}34={0}52=20100310-12:50:4510={4}", SeqNum, SenderCompId, TargetCompId, MessageType, calculateChecksum(), calculateLength());
		}

		/// <summary>
		/// Serialize the message into bytes array
		/// <b>Call before sending the message</b>
		/// </summary>
		/// <returns></returns>
		public byte[] Serialize ()
		{
			prepareBodyForSending ();
			return System.Text.ASCIIEncoding.ASCII.GetBytes (ToString ());
		}

		#endregion

		#region Private Methods

		private int calculateLength ()
		{
			lock (tags) {
				if (body == null)
					return 0;
				return body.Length;
			}
		}

		private void parseTagsFromBody ()
		{
			lock (tags) {
				if (body == null)
					return;

				var dataAsString = System.Text.ASCIIEncoding.ASCII.GetString (body);
				char[] DELIMITERS = new char[] { (char)1, '=' };
				var msg = dataAsString.Split (DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
				for (int jj = 0; jj < msg.Length; jj += 2) {
					int tag = -1;
					if (int.TryParse (msg [jj], out tag) && jj + 1 < msg.Length) {
						if (tag == HeaderTags.SeqNum)
							fillTag (tag, long.Parse (msg [jj + 1]));
						else
							fillTag (tag, msg [jj + 1]);
					}
				}
			}
		}

		private void fillTag (int tag, object value)
		{
			lock (tags) {
				if (tags.ContainsKey (tag))
					tags [tag] = value;
				else
					tags.Add (tag, value);

				isChanged = true;
				checksum = null;
			}
		}

		private object readTag (int tag, object defaultValue)
		{
			lock (tags) {
				if (tags.ContainsKey (tag))
					return tags [tag];
				else
					return defaultValue;
			}
		}

		private void prepareBodyForSending ()
		{
			if (!isChanged)
				return;

			//TODO: optimize performance
			fillTag (HeaderTags.SendingTime, DateTime.UtcNow.ToString (MESSAGE_DATE_FORMAT));
			List<byte> bytes = new List<byte> (1024);
			lock (tags) {
				foreach (var tag in tags) {
					string pair = string.Format (PAIR_FORMAT, tag.Key, tag.Value.ToString ());
					bytes.AddRange (System.Text.ASCIIEncoding.ASCII.GetBytes (pair));
				}
				body = bytes.ToArray ();

				checksum = null;
				isChanged = false;
			}
		}

		#endregion

	}
}
