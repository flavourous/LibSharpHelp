using System;
using System.Collections.Generic;

namespace LibSharpHelp
{
	public static class FormattingExtensions
	{
		class ARet {
			public readonly bool success; public readonly double amount; public readonly String units;  
			public ARet(bool success, double amount, String units) { this.success=success; this.amount=amount; this.units=units; }
		}
		delegate ARet AmtGetter(TimeSpan input);
		static readonly List<AmtGetter> amounters = new List<AmtGetter> {
			ts => new ARet(ts.Minutes != 0, ts.TotalMinutes, "Minutes"),
			ts => new ARet(ts.Hours != 0, ts.TotalHours, "Hours"),
			ts => new ARet(ts.Days != 0, ts.TotalDays, "Days"),
		};
		public static String WithSuffix(this TimeSpan self)
		{
			double amount;
			String units;
			return "To fix. FIXME.";
		}
	}
}

