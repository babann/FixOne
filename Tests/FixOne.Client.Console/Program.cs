using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace FixOne.Client.Console
{
	class Program
	{
		private static FixOne.Engine.Engine engine;

		static void Main(string[] args)
		{
#if DEBUG
			ConsoleTraceListener consoleTracer = new ConsoleTraceListener();
			Trace.Listeners.Add(consoleTracer);
#endif
			engine = new FixOne.Engine.Engine();

			engine.BeforeStart += (object s, FixOne.Engine.Events.EngineEventArgs e) =>
			{
				System.Console.WriteLine("Starting engine v.{0}", engine.Version);
			};

			engine.Started += (object s, EventArgs e) =>
			{
				System.Console.WriteLine("Engine v.{0} started.", engine.Version);
				System.Console.WriteLine("Press CTRL+C for exit.");
				System.Console.CancelKeyPress += (object o, ConsoleCancelEventArgs ea) => { ExitProgram(); };
			};

			engine.BeforeStop += (object s, FixOne.Engine.Events.EngineEventArgs e) =>
			{
				System.Console.WriteLine("Exit signal received.");
			};

			engine.Stopped += (object s, EventArgs e) =>
			{
				System.Console.WriteLine("Engine stopped.");
			};

			engine.SessionEstablished += (object s, FixOne.Engine.Events.SessionEstablishedEventArgs e) =>
			{
				FixOne.Engine.FixSession session = s as FixOne.Engine.FixSession;
				if (session == null)
					return;

				for(int ii=0; ii<10; ii++)
					session.Send(session.NewMessage("D"));
			};

			engine.Start();

			System.Console.ReadLine();

			Environment.Exit(0);
		}

		static void ExitProgram()
		{
			engine.Stop();
			System.Console.WriteLine("Stop signal sent. Waiting to close the socket.");
			Thread.Sleep(500);
		}
	}
}
