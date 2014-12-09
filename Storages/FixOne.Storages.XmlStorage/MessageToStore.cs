using System;
using System.Collections.Generic;
using FixOne.Entities;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace FixOne.Storages.XmlStorage
{
	internal class MessageToStore
	{
		internal long Id { get; private set; }

		internal bool IsSent { get; set; }

		internal FixMessage Message { get; private set; }

		internal MessageToStore (FixMessage message)
		{
			Id = DateTime.UtcNow.Ticks;
			IsSent = false;
			Message = message;
		}

		internal XElement GetElement()
		{
			var msgElement = new XElement("Msg", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Message.ToString())));
			msgElement.SetAttributeValue("Seq", Message.SeqNum);
			msgElement.SetAttributeValue("Id", Id);
			msgElement.SetAttributeValue("IsSent", IsSent);
			return msgElement;
		}

		internal static MessageToStore Restore(XElement element)
		{
			if (element == null)
				return null;

			return new MessageToStore (
				new FixMessage (Parsers.Generic.FixMessageHeaderParser.ParseHeaders (Convert.FromBase64String (element.Value), null).First())) {
				IsSent = false,
				Id = (long)element.Attribute ("Id")
			};
		}
	}
}

