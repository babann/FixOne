using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FixOne.Engine
{
	internal class EngineMetricsManager
	{
		//TODO:

		static bool exit = false; //TODO: make a global EXIT flag and use it instead of local private fields
		Entities.Metrics.EngineMetrics metrics = new Entities.Metrics.EngineMetrics();
		
		internal event EventHandler<Events.EngineMetricsUpdateEventArgs> MetricsUpdate;

		internal void Start()
		{
			exit = false;
			new Thread((object state) =>
				{
					while (!exit)
					{
						Thread.Sleep(10 * 60 * 1000); //Sleep 10 sec
						if (MetricsUpdate != null)
							MetricsUpdate(this, new Events.EngineMetricsUpdateEventArgs(metrics));
					}
				}).Start(null);
		}

		internal void Stop()
		{
			exit = true;
		}

		internal void RegisterMetric(int metricCode, object metricValue, string sessionName)
		{
		}
	}
}
