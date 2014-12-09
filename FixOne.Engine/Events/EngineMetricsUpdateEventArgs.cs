using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Engine.Events
{
	public class EngineMetricsUpdateEventArgs : EventArgs
	{
		public IEnumerable<Entities.Metrics.MetricDisplayInfo> EngineMetrics
		{
			get;
			private set;
		}

		public IDictionary<string, IEnumerable<Entities.Metrics.MetricDisplayInfo>> SessionMetrics
		{
			get;
			private set;
		}

		internal EngineMetricsUpdateEventArgs(Entities.Metrics.EngineMetrics metrics)
		{
			EngineMetrics = (from metric in metrics.Values select new Entities.Metrics.MetricDisplayInfo(metric.Value)).ToArray();
			SessionMetrics = null;
			//TODO: fill properties using LINQ
		}
	}
}
