using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LibSharpHelp
{
    public static class ExpressionHelpers
    {
        public static void CastAct<T>(this Object o, Action<T> act, Action fail = null)
        {
            if (o is T) act((T)o);
            else fail?.Invoke();
        }
    }
}
