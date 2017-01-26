using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace LibSharpHelp
{
	public static class EnumerableExtenstions
	{
        /// <summary>
        /// performs cross action.  returns false when unequal lengths.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="A"></typeparam>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="each"></param>
        /// <returns></returns>
        public static bool Both<T,A>(this IEnumerable<T> self, IEnumerable<A> other, Action<T,A> each)
        {
            using (var es = self.GetEnumerator())
            using (var eo = other.GetEnumerator())
            {
                bool ees, eeo;
                ees = es.MoveNext();
                eeo = eo.MoveNext();
                while (ees && eeo)
                {
                    each(es.Current, eo.Current);
                    ees = es.MoveNext();
                    eeo = eo.MoveNext();
                }
                return ees == eeo;
            }
        }
        public static void Act<T>(this IEnumerable<T> self, Action<T> each)
        {
            foreach (var t in self) each(t);
        }

        public static List<Out> MakeList<In,Out>(this IEnumerable<In> myself, Func<In, Out> creator)
		{
			return new List<Out>(from s in myself select creator(s));
		}
		public static BindingList<Out> MakeBindingList<In,Out>(this IEnumerable<In> myself, Func<In, Out> creator)
		{
			return new BindingList<Out>(new List<Out>(from s in myself select creator(s)));
		}
		public static TList MakeSomething<In,Out,TList>(this IEnumerable<In> myself, Func<In, Out> creator, Func<IEnumerable<Out>,TList> listCreator)
		{
			return listCreator(from s in myself select creator(s));
		}
        public static IEnumerable<KeyValuePair<T2k, T2v>> ToKeyValue<T1, T2v, T2k>(this IEnumerable<T1> @this, Func<T1, T2k> key, Func<T1, T2v> value)
        {
            return @this.Select(d => new KeyValuePair<T2k, T2v>(key(d), value(d)));
        }
		public static bool Contains<T>(this IEnumerable<T> items, T item) where T : class
		{
			foreach (T i in items)
				if (i.Equals (item))
					return true;
			return false;
		}
		public static void RemoveAll(this IList list, params Object[] items)
		{
			foreach (var t in items)
				list.Remove (t);
		}
		public static void AddAll<T>(this IList<T> list, IEnumerable<T> items)
		{
			foreach (var t in items)
				list.Add (t);
		}
		public static void AddAll<T>(this IList<T> list, params T[] items)
		{
			foreach (var t in items)
				list.Add (t);
		}
		public static void Add(this IList list, params Object[] items)
		{
			for (int i = 0; i < items.Length; i++)
				list.Add (items [i]);
		}
		public static void Ensure(this IList list, int index, params Object[] items)
		{
			int lindex = index;
			foreach (var t in items) 
			{
				if (list.Count <= lindex || !Object.Equals (list [lindex], t)) 
				{
					list.Remove (t);
					if (lindex < list.Count)
						list.Insert (lindex, t);
					else
						list.Add (t);
				}
				lindex++;
			}
		}
	}
	class ReadOnlyListConversionAdapter<In,Out> : IReadOnlyList<Out>
	{
		readonly IReadOnlyList<In> input;
		readonly Func<In,Out> convertDelegate;
		public ReadOnlyListConversionAdapter(IReadOnlyList<In> input, Func<In,Out> convertDelegate)
		{
			this.input = input;
			this.convertDelegate = convertDelegate;
		}

		#region IEnumerable implementation
		public IEnumerator<Out> GetEnumerator ()
		{
			return new ListConverterEnumerator<In,Out> (input, convertDelegate);
		}
		IEnumerator IEnumerable.GetEnumerator () { return GetEnumerator (); }
		#endregion

		#region IReadOnlyList implementation
		public Out this [int index] { get { return convertDelegate (input [index]); } }
		#endregion
		#region IReadOnlyCollection implementation
		public int Count { get { return input.Count; } }
		#endregion
	}
	public class ListEnumerator<T> : IEnumerator<T>
	{
		readonly IList<T> dd;
		int st = -1;
		public ListEnumerator(IList<T> dd)
		{
			this.dd = dd;
		}
		#region IEnumerator implementation
		public bool MoveNext ()
		{
			st++;
			return st < dd.Count;
		}
		public void Reset ()
		{
			st = 0; 
		}
		protected virtual T current { get { return dd [st]; } }
		object IEnumerator.Current { get { return current; } }
		public T Current { get { return current; } }
		#endregion
		public void Dispose () { }
	}
	public class ListConverterEnumerator<TIn, TOut> : IEnumerator<TOut>
	{
		readonly IReadOnlyList<TIn> dd;
		readonly Func<TIn,TOut> cdel;
		int st = -1;
		public ListConverterEnumerator(IReadOnlyList<TIn> dd, Func<TIn,TOut> cdel)
		{
			this.dd = dd;
			this.cdel = cdel;
		}
		#region IEnumerator implementation
		public bool MoveNext ()
		{
			st++;
			return st < dd.Count;
		}
		public void Reset ()
		{
			st = 0; 
		}
		TOut current { get { return cdel(dd [st]); } }
		object IEnumerator.Current { get { return current; } }
		public virtual TOut Current { get { return current; } }
		#endregion
		public void Dispose () { }
	}
}

