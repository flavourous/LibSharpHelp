using System;
using System.Collections.Generic;
using System.Text;

namespace LibSharpHelp
{
	public static class FormattingExtensions
	{
        public static IEnumerable<String> QuotedSplit(this String s, char split, params char[] quote)
        {
            HashSet<char> qq = new HashSet<char>(quote);
            StringBuilder rv = new StringBuilder();
            bool inquote = false;
            char currentq = (char)0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == split && !inquote)
                {
                    yield return rv.ToString();
                    rv.Clear();
                }
                else if (qq.Contains(c) && (!inquote || c == currentq))
                {
                    currentq = c;
                    inquote = !inquote;
                }
                else rv.Append(c);
            }
            yield return rv.ToString();
        }

        public static String ToCapital(this string s)
        {
            if (s.Length == 0) return s;
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }

        public static String ToNiceAscii(this string friendlyname)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < friendlyname.Length; i++)
            {
                var c = friendlyname[i];
                sb.Append((c < 17 || c > 128) ? '_' : c);
            }
            return sb.ToString();
        }

        class ARet {
			public readonly bool success; public readonly double? amount; public readonly String units;  
			public ARet(bool success, double? amount, String units) { this.success=success; this.amount=amount; this.units=units; }
		}
		delegate ARet AmtGetter(TimeSpan input);
		static readonly List<AmtGetter> amounters = new List<AmtGetter> {
			ts => new ARet(ts.Days % 7 == 0 && ts.Days > 0, ts.TotalDays / 7, "weeks"),
			ts => new ARet(ts.Days != 0, ts.TotalDays, "days"),
			ts => new ARet(ts.Hours != 0, ts.TotalHours, "hours"),
			ts => new ARet(ts.Minutes != 0, ts.TotalMinutes, "minutes"),
			ts => new ARet(true, ts.TotalSeconds, "seconds"),
		};
        static readonly List<AmtGetter> preamounters = new List<AmtGetter> {
            ts => new ARet(ts.Days % 7 == 0 && ts.Days > 0, ts.TotalDays / 7, "weekly"),
            ts => new ARet(ts.Days != 0, ts.TotalDays, "daily"),
            ts => new ARet(ts.Hours != 0, ts.TotalHours, "hourly"),
            ts => new ARet(ts.Minutes != 0, ts.TotalMinutes, "minutely"),
            ts => new ARet(true, ts.TotalSeconds, "secondly"),
        };
        public static String WithSuffix(this TimeSpan self, bool numeric_singular = false)
        {
            ARet ar = null;
            foreach (var aa in amounters)
                if ((ar = aa(self)).success)
                    break;
            bool o = ar.amount == 1.0;
            return String.Format("{0}{1}",
                o && !numeric_singular ? "" : ar.amount.HasValue ? (ar.amount.Value.ToString("F0") + " ") : "",
                o ? ar.units.Substring(0, ar.units.Length - 1) : ar.units
                );
        }
        public static String WithPrefix(this TimeSpan self, bool numeric_singular = false)
        {
            ARet ar = null;
            foreach (var aa in preamounters)
                if ((ar = aa(self)).success)
                    break;
            bool o = ar.amount == 1.0;
            return String.Format("{0}{1}",
                o && !numeric_singular ? "" : ar.amount.HasValue ? (ar.amount.Value.ToString("F0") + " ") : "",
                ar.units
                );
        }
        
        public static string Suffix(this int num)
        {
            int fd = num % 10;
            String suf = "th";
            if (fd == 1) suf = "st";
            else if (fd == 2) suf = "nd";
            else if (fd == 3) suf = "rd";
            return suf;
        }
		public static String WithSuffix(this int num)
		{
			return num + num.Suffix();
		}
	}
}

