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
			ts => new ARet(ts.Days % 7 == 0 && ts.Days > 0, ts.TotalDays / 7, "Weeks"),
			ts => new ARet(ts.Days != 0, ts.TotalDays, "Days"),
			ts => new ARet(ts.Hours != 0, ts.TotalHours, "Hours"),
			ts => new ARet(ts.Minutes != 0, ts.TotalMinutes, "Minutes"),
			ts => new ARet(true, 0, "Zero"),
		};
		public static String WithSuffix(this TimeSpan self)
		{
			ARet ar = null;
			foreach (var aa in amounters)
				if ((ar = aa (self)).success)
					break;
			return String.Format ("{0} {1}", ar.amount.ToString ("F0"), ar.units);
		}
		public static String WithSuffix(this int num)
		{
			int fd = num % 10;
			String suf = "th";
			if (fd == 1) suf = "st";
			else if (fd == 2) suf = "nd";
			else if (fd == 3) suf = "rd";
			return num + suf;
		}
	}
}

