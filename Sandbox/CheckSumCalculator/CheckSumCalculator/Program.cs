using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckSumCalculator
{
	class Program
	{
		static void Main(string[] args)
		{
			string fullMessage = "8=FIX.4.49=5735=049=IntGBIorders56=Broker34=252=20100310-12:50:4510=244";
			string messageBody = fullMessage.Substring(0, fullMessage.Length - 8);
			byte[] body = System.Text.ASCIIEncoding.ASCII.GetBytes(messageBody);

			var sum = 1;
			for (int ii = 0; ii < body.Length; sum += body[ii++]) ;
			int checksumm = sum % 256;

			Console.WriteLine("Originl message is {0}", fullMessage);
			Console.WriteLine("Calculated checksum is {0}", checksumm);
			Console.ReadKey();
		}
	}
}
