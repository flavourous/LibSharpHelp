using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace LibSharpHelp.Test
{
	interface ITest 
	{
		void RunTest();
	}
	class MainClass
	{
       
		public static void Main (string[] args)
		{
			Console.WriteLine ("Running tests!  #Assert");
			foreach (ITest test in from s in Assembly.GetCallingAssembly ().GetTypes ()
					where s.GetInterfaces().Contains(typeof(ITest))
				select (ITest)Activator.CreateInstance(s)) TestRunner(test);
			Console.WriteLine ("Got to end of code I suppose tests are #GREEEEENNN");
		}
		static void TestRunner(ITest test)
		{
			test.RunTest ();
		}
	}
	interface IAssert { void Assert(); }

	class EnsureListTests : ITest
	{
		class EnsureTest<T> : IAssert where T : IEquatable<T> {
			public readonly T[] expected;
			public readonly T[] initial;
			public readonly int first;
			public readonly T[] ensure;
			public EnsureTest(T[] initial, int first, T[] ensure, T[] expected)
			{
				this.expected=expected;
				this.initial=initial;
				this.first=first;
				this.ensure = ensure;
			}
			public void Assert () {
				var test = new List<T> (initial);
				test.Ensure (first, new List<Object> (from e in ensure select e as Object).ToArray ());
				Debug.Assert (test.Count == expected.Length);
				for (int i = 0; i < test.Count; i++)
					Debug.Assert (test[i].Equals(expected [i]));	
			}
		}

		readonly IReadOnlyList<IAssert> tests = new List<IAssert> {
			new EnsureTest<int> (new[]{ 1, 3, 2, 4, 5 }, 1, new[] { 2, 3 }, new[] { 1, 2, 3, 4, 5 }),
			new EnsureTest<int> (new[]{ 1, 9, 8, 4, 5 }, 1, new[] { 2, 3 }, new[] { 1, 2, 3, 9, 8, 4, 5 }),
			new EnsureTest<int> (new[]{ 1, 2, 3, 4, 5 }, 1, new[] { 2, 3 }, new[] { 1, 2, 3, 4, 5 }),
		};

		#region ITest implementation
		public void RunTest ()
		{
			foreach (var t in tests)
				t.Assert ();
		}
		#endregion
	}
}
