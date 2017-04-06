using System;

namespace LibSharpHelp
{
	static class ConvertHelp
	{
		public static int ToInt(Object value)
		{
			return (int)Convert.ChangeType (value, typeof(int));
		}
	}
}

