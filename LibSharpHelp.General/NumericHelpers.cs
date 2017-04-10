using System;
using System.Collections.Generic;

namespace LibSharpHelp
{
	static class ConvertHelp
	{
        public static IEnumerable<uint> SplitAsFlags(this uint value)
        {
            uint current = value, compare = 1;
            while (current != 0)
            {
                if ((current & compare) != 0)
                {
                    yield return compare;
                    current -= compare;
                }
                compare *= 2; // next flag...
            }
        }

        // check the number of bits present in the flag...
        public static IEnumerable<T> SplitFlags<T>(this uint value) 
        {
            // BitMasks 101: 0 | 0 == 0, we cant pick up on that "flag" it's the implicit none. 
            uint current = (uint)value, compare = 1;
            while (current != 0)
            {
                if ((current & compare) != 0)
                {
                    yield return (T)Enum.ToObject(typeof(T), compare);
                    current -= compare;
                }
                compare *= 2; // next flag...
            }
        }

        public static int ToInt(Object value)
		{
			return (int)Convert.ChangeType (value, typeof(int));
		}
	}
}

