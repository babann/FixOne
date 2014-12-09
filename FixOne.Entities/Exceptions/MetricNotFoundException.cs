using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FixOne.Entities.Exceptions
{
	public class MetricNotFoundException : Exception
	{
		public MetricNotFoundException(int code)
			: base(string.Format("Metric with code '{0}' is not found.", code))
		{

		}
	}
}
