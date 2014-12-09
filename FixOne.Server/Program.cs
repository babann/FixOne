using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace FixOne.Server
{
	class Program
	{
		private static FixOne.Engine.Engine engine;

		static void Main(string[] args)
		{
			//Entities.Settings.EngineSettings settings = new Entities.Settings.EngineSettings();

#if DEBUG
			ConsoleTraceListener consoleTracer = new ConsoleTraceListener();
			Trace.Listeners.Add(consoleTracer);

			//settings.InstanceIP = new byte[] { 127, 0, 0, 1 };
			//settings.ListenPort = 10001;
			//settings.NumerOfWorkersPerSession = 4;
			//settings.DefaultHeartBeatInetrval = 30;
			//settings.InitiatorReconnectDelay = 30;
			//settings.AcceptAllInitiators = true;
			//settings.SessionsDefinitionFileName = "sessions.xml";

#endif
			engine = new FixOne.Engine.Engine();//settings);

			engine.BeforeStart += (object s, FixOne.Engine.Events.EngineEventArgs e) =>
			{
				Console.WriteLine("Starting engine v.{0}", engine.Version);
			};

			engine.Started += (object s, EventArgs e) =>
			{
				Console.WriteLine("Engine v.{0} started.", engine.Version);
				Console.WriteLine("Press CTRL+C for exit.");
				Console.CancelKeyPress += (object o, ConsoleCancelEventArgs ea) => { ExitProgram(); };
			};

			engine.BeforeStop += (object s, FixOne.Engine.Events.EngineEventArgs e) =>
			{
				Console.WriteLine("Exit signal received.");
			};

			engine.Stopped += (object s, EventArgs e) =>
			{
				Console.WriteLine("Engine stopped.");
			};

			engine.Start();

			Console.ReadLine();

			ExitProgram();
			Environment.Exit(0);
		}

		static void ExitProgram()
		{
			engine.Stop();
			Console.WriteLine("Stop signal sent. Waiting to close the socket.");
			Thread.Sleep(500);
		}
	}
}
