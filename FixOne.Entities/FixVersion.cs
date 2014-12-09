using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FixOne.Entities
{
	public enum FixVersion
	{
		[Description("FIX.4.0")]
		FIX40 = 0,
		[Description("FIX.4.1")]
		FIX41 = 1,
		[Description("FIX.4.2")]
		FIX42 = 2,
		[Description("FIX.4.3")]
		FIX43 = 3,
		[Description("FIX.4.4")]
		FIX44 = 4,
		[Description("FIXT.1.1")]
		FIX50 = 5,
		[Description("FIXT.1.1")]
		FIX501 = 6,
		[Description("FIXT.1.1")]
		FIX502 = 7,
		FIXML = 8,
		FAST = 9
	}
}
