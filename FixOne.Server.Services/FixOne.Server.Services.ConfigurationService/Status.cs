using System;

namespace FixOne.Server.Services.ConfigurationService
{
	public struct Status
	{
		public string Message {
			get;
			set;
		}

		public bool Success {
			get;
			set;
		}

		public static readonly Status StatusOK = new Status() { Message = "OK", Success = true};
		public static readonly Status StatusError = new Status(){Message = "Error", Success = false};
	}
}

