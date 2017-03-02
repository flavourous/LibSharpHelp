using LibSharpHelp;
using NUnit.Framework;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    interface ipk { int id { get; } String nm { get; set; } }
    public class TestFrom : ipk
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public String nm { get; set; }
    }

    public class TestTo : ipk
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public String nm { get; set; }
    }
    class scon : SQLiteConnection
    {
        public readonly ReaderWriterLockSlim sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public scon(SQLitePlatformGeneric pg, string n) : base(pg, n) { }
    }
    static class TEX
    {
        static IDictionary<Type, int> mids = new ConcurrentDictionary<Type, int>(); // ok for tests
        public static SqliteKeyTo<F,T> K2Gen<F, T>(this scon conn, F from, int pidx)
            where F : class, ipk
            where T : class, ipk
        {
            Type tT = typeof(T), tF = typeof(F);
            if (!mids.ContainsKey(tT)) mids[tT] = mids.Count;
            if (!mids.ContainsKey(tF)) mids[tF] = mids.Count;
            return new SqliteKeyTo<F,T>(() => from.id, k => k.id, pidx, mids[tF], mids[tT], conn, conn.GetMapping<T>(), conn.GetMapping<F>(), conn.sync);
        }

        public static F GenFrom<F>(this scon conn, String init) where F : class, ipk, new()
        {
            var ret = new F { nm = init };
            conn.Insert(ret);
            return ret;
        }
        
        public static T[] GenTo<F,T>(this scon conn, F from, int pid, bool replace, params object[] fvals)
            where F : class, ipk
            where T : class, ipk, new()
        {
            var fv = fvals.Select(n => n is T ? n as T : new T { nm = (String)n }).ToArray();
            var k2 = conn.K2Gen<F, T>(from, pid);
            if (replace) k2.Replace(fv);
            else k2.Add(fv);
            k2.Commit();
            return fv;
        }

        public static void AssertHasFks<F,T>(this scon conn, F nmr, int pid, params T[] nms)
            where F : class, ipk
            where T : class, ipk
        {
            var k2 = conn.K2Gen<F, T>(nmr, pid);
            var troot = conn.Table<F>().Where(f => f.nm == nmr.nm).First();
            var fkv = k2.Get().ToArray();
            AssertIPK(nms, fkv);
        }
        public static void AssertIPK(this IEnumerable<ipk> @this, IEnumerable<ipk> other)
        {
            Assert.AreEqual(@this.Count(), other.Count());
            foreach (var n in @this)
                Assert.AreEqual(1, other.Where(f => f.nm == n.nm).Count());
        }
    }

    [TestFixture]
    class SqliteNetHelperTests
    {


        scon GetConn()
        {
            var fi = new FileInfo(Path.GetTempFileName());
            SQLitePlatformGeneric plat = new SQLitePlatformGeneric();
            var conn = new scon(plat, fi.FullName);
            conn.CreateTable<TestFrom>();
            conn.CreateTable<TestTo>();
            conn.CreateTable<OtherFrom>();
            conn.CreateTable<OtherTo>();
            return conn;
        }

        [Test]
        public void TestBasicKeyTo()
        {
            var conn = GetConn();

            // single root, add some fks and commit, do they come back?
            var root = conn.GenFrom<TestFrom>("First Root");
            var fks = conn.GenTo<TestFrom, TestTo>(root, 0, false, "FK 1", "FK 2", "FK 3");
            conn.AssertHasFks(root, 0, fks);
        }

        [Test]
        public void TestSharedKeyTo()
        {
            var conn = GetConn();

            // what happens if we add another root, and a few fks with a shared one?
            var root = conn.GenFrom<TestFrom>("First Root");
            var fks = conn.GenTo<TestFrom, TestTo>(root, 0, false, "FK 1", "FK 2", "FK 3");
            var root2 = conn.GenFrom<TestFrom>("Second Root");
            var afks = conn.GenTo<TestFrom, TestTo>(root2, 0, false, "aFK 1", fks[1], "aFK 2", "aFK 3");
            conn.AssertHasFks(root2, 0, afks);
            Assert.AreEqual(6, conn.Table<TestTo>().Count());
        }

        [Test]
        public void TestSimpleClear()
        {
            var conn = GetConn();

            var root = conn.GenFrom<TestFrom>("First Root");
            var fks = conn.GenTo<TestFrom, TestTo>(root, 0, false, "FK 1", "FK 2", "FK 3");
            conn.AssertHasFks(root, 0, fks);
            var k2 = conn.K2Gen<TestFrom, TestTo>(root, 0);
            k2.Clear();
            conn.AssertHasFks<TestFrom, TestTo>(root, 0);
        }

        [Test]
        public void TestClearWithShared()
        {
            var conn = GetConn();

            // what happens if we add another root, and a few fks with a shared one?
            var root = conn.GenFrom<TestFrom>("First Root");
            var fks = conn.GenTo<TestFrom, TestTo>(root, 0, false, "FK 1", "FK 2", "FK 3");
            var root2 = conn.GenFrom<TestFrom>("Second Root");
            var afks = conn.GenTo<TestFrom, TestTo>(root2, 0, false, "aFK 1", fks[1], "aFK 2", "aFK 3");
            var k2 = conn.K2Gen<TestFrom, TestTo>(root, 0);
            k2.Clear();

            // it should still have the other one
            Assert.AreEqual(1, conn.Table<TestTo>().Where(d => d.nm == "FK 2").Count());
            conn.AssertHasFks<TestFrom, TestTo>(root, 0);
            conn.AssertHasFks(root2, 0, afks);
        }

        [Test]
        public void TestBookkeeping()
        {
            // bookkeeping removes KeyToIndex refs where To or From are gone.
            var conn = GetConn();

            // basic setup
            var root = conn.GenFrom<TestFrom>("First Root");
            var fks = conn.GenTo<TestFrom, TestTo>(root, 0, false, "FK 1", "FK 1");
            var k2 = conn.K2Gen<TestFrom, TestTo>(root, 0);
            conn.Delete<TestTo>(fks[0].id);
            Assert.AreEqual(2, conn.Table<KeyToIndex>().Count());
            k2.Commit(); // does bookkeepy
            Assert.AreEqual(1, conn.Table<KeyToIndex>().Count());
            conn.Delete<TestFrom>(root.id);
            k2.Commit();
            Assert.AreEqual(0, conn.Table<KeyToIndex>().Count());
        }

        // many proprties, same/diff type
        [Test]
        public void TestProp()
        {
            var conn = GetConn();

            // what happens if we add another root, and a few fks with a shared one?
            var root = conn.GenFrom<TestFrom>("First Root");
            var fks = conn.GenTo<TestFrom, TestTo>(root, 0, false, "FK 1", "FK 2", "FK 3");
            var afks = conn.GenTo<TestFrom, TestTo>(root, 1, false, "aFK 1", fks[1], "aFK 2", "aFK 3");
            conn.AssertHasFks(root, 0, fks);
            conn.AssertHasFks(root, 1, afks);
            Assert.AreEqual(6, conn.Table<TestTo>().Count());

            // lets try a couple deleteys
            var k1 = conn.K2Gen<TestFrom, TestTo>(root, 0);
            var k2 = conn.K2Gen<TestFrom, TestTo>(root, 1);

            k2.Remove(new[] { fks[1] });
            k2.Commit();
            var afks_new = afks.Take(1).Concat(afks.Skip(2)).ToArray();
            conn.AssertHasFks(root, 0, fks);
            conn.AssertHasFks(root, 1, afks_new);
            Assert.AreEqual(6, conn.Table<TestTo>().Count());

            k1.Remove(new[] { fks[2] });
            k1.Commit();
            conn.AssertHasFks(root, 0, fks.Take(2).ToArray());
            conn.AssertHasFks(root, 1, afks_new);
            Assert.AreEqual(5, conn.Table<TestTo>().Count());
        }

        class OtherFrom : TestFrom { }
        class OtherTo : TestTo { }
        // many from classes, to classes
        [Test]
        public void TestClass()
        {
            var conn = GetConn();

            // what happens if we add another root, and a few fks with a shared one?
            var root1 = conn.GenFrom<TestFrom>("First Root");
            var afks1 = conn.GenTo<TestFrom, TestTo>(root1, 0, false, "FK 1", "FK 2", "FK 3");
            var root2 = conn.GenFrom<OtherFrom>("Second Root");
            var afks2 = conn.GenTo<OtherFrom, TestTo>(root2, 1, false, "aFK 1", afks1[1], "aFK 2", "aFK 3");
            var root3 = conn.GenFrom<OtherFrom>("third Root");
            var afks3 = conn.GenTo<OtherFrom, OtherTo>(root3, 2, false, "1aFK 1", "1aFK 2", "1aFK 3");
            var root4 = conn.GenFrom<TestFrom>("foth Root");
            var afks4 = conn.GenTo<TestFrom, OtherTo>(root4, 1, false, "2aFK 1", afks3[2], "2aFK 2", afks3[0]);
            var k1 = conn.K2Gen<TestFrom, TestTo>(root1, 0);
            var k2 = conn.K2Gen<OtherFrom, TestTo>(root2, 1);
            var k3 = conn.K2Gen<OtherFrom, OtherTo>(root3, 2);
            var k4 = conn.K2Gen<TestFrom, OtherTo>(root4, 1);

            // This is initial setup
            conn.AssertHasFks(root1, 0, afks1);
            conn.AssertHasFks(root2, 1, afks2);
            conn.AssertHasFks(root3, 2, afks3);
            conn.AssertHasFks(root4, 1, afks4);
            Assert.AreEqual(6, conn.Table<TestTo>().Count());
            Assert.AreEqual(5, conn.Table<OtherTo>().Count());

            // remove one
            k2.Remove(new[] { afks2[1] });
            k2.Commit();
            //T:T:1: "FK 1", "FK 2", "FK 3"  
            //O:T:2: "aFK 1", <<afks1[1]>>, "aFK 2", "aFK 3"
            //O:O:3: "1aFK 1", "1aFK 2", "1aFK 3"
            //T:O:4: "2aFK 1", afks3[2], "2aFK 2", afks3[0]
            var afks2_v1 = new[] { afks2[0], afks2[2], afks2[3] };
            conn.AssertHasFks(root1, 0, afks1);
            conn.AssertHasFks(root2, 1, afks2_v1);
            conn.AssertHasFks(root3, 2, afks3);
            conn.AssertHasFks(root4, 1, afks4);
            Assert.AreEqual(6, conn.Table<TestTo>().Count());
            Assert.AreEqual(5, conn.Table<OtherTo>().Count());

            // remove couple more
            k3.Remove(new[] { afks3[2] });
            k4.Remove(new[] { afks4[0], afks3[2] });
            k3.Commit(); k4.Commit();
            //T:T:1: "FK 1", "FK 2", "FK 3"  
            //O:T:2: "aFK 1", <<afks1[1]>>, "aFK 2", "aFK 3"
            //O:O:3: "1aFK 1", "1aFK 2", <<"1aFK 3">>
            //T:O:4: <<"2aFK 1">>, <<afks3[2]>>, "2aFK 2", afks3[0]
            var afks3_v1 = new[] { afks3[0], afks3[1] };
            var afks4_v1 = new[] { afks4[2], afks3[0] };
            conn.AssertHasFks(root1, 0, afks1);
            conn.AssertHasFks(root2, 1, afks2_v1);
            conn.AssertHasFks(root3, 2, afks3_v1);
            conn.AssertHasFks(root4, 1, afks4_v1);
            Assert.AreEqual(6, conn.Table<TestTo>().Count());
            Assert.AreEqual(3, conn.Table<OtherTo>().Count());

            // clear one
            k1.Clear();
            //T:T:1: <<"FK 1">>, <<"FK 2">>, <<"FK 3">> 
            //O:T:2: "aFK 1", <<afks1[1]>>, "aFK 2", "aFK 3"
            //O:O:3: "1aFK 1", "1aFK 2", <<"1aFK 3">>
            //T:O:4: <<"2aFK 1">>, <<afks3[2]>>, "2aFK 2", afks3[0]
            conn.AssertHasFks<TestFrom, TestTo>(root1, 0);
            conn.AssertHasFks(root2, 1, afks2_v1);
            conn.AssertHasFks(root3, 2, afks3_v1);
            conn.AssertHasFks(root4, 1, afks4_v1);
            Assert.AreEqual(3, conn.Table<TestTo>().Count());
            Assert.AreEqual(3, conn.Table<OtherTo>().Count());
        }

        // non-committed add/removal
        [Test]
        public void TestLimbo()
        {
            var conn = GetConn();

            var root = conn.GenFrom<TestFrom>("First Root");
            var k2 = conn.K2Gen<TestFrom, TestTo>(root, 0);
            var fks = new[] { new TestTo { nm = "T1" }, new TestTo { nm = "T3" }, new TestTo { nm = "T4" } };
            k2.Add(fks);
            Assert.AreEqual(0, conn.Table<TestTo>().Count());
            k2.Get().AssertIPK(fks);
            k2.Remove(fks.Take(1));
            Assert.AreEqual(0, conn.Table<TestTo>().Count());
            k2.Get().AssertIPK(fks.Skip(1).ToArray());
            k2.Clear();
            Assert.AreEqual(0, k2.Get().Count());
            Assert.AreEqual(0, conn.Table<TestTo>().Count());
        }

        // replace
        [Test]
        public void TestReplace()
        {
            var conn = GetConn();

            var root = conn.GenFrom<TestFrom>("First Root");
            var fks = conn.GenTo<TestFrom, TestTo>(root, 1, false, "G1", "G2", "G3", "G4");
            conn.AssertHasFks(root, 1, fks);
            var fks2 = conn.GenTo<TestFrom, TestTo>(root, 1, false, "N1", "N2", "N3", "N4");
            conn.AssertHasFks(root, 1, fks.Concat(fks2).ToArray());
            fks = conn.GenTo<TestFrom, TestTo>(root, 1, true, "G1", "G2", "G3", "G4");
            conn.AssertHasFks(root, 1, fks);
            fks2 = conn.GenTo<TestFrom, TestTo>(root, 1, true, "N1", "N2", "N3", "N4");
            conn.AssertHasFks(root, 1, fks2);
        }

        // pre exist, with other dudes going on
        [Test] 
        public void TestPreexist()
        {
            var conn = GetConn();

            // tt
            var fks = new[] { new TestTo { nm = "T1" }, new TestTo { nm = "T3" } };
            conn.InsertAll(fks);
            var efk = fks.Concat(new[] { new TestTo { nm = "was not pre" } }).ToArray();

            // ot
            var oks = new[] { new OtherTo { nm = "T1" }, new OtherTo { nm = "T3" } };
            conn.InsertAll(oks);
            var ofk = oks.Concat(new[] { new OtherTo { nm = "was not pre" } }).ToArray();

            Assert.AreEqual(2, conn.Table<TestTo>().Count());
            Assert.AreEqual(2, conn.Table<OtherTo>().Count());

            var root1 = conn.GenFrom<TestFrom>("First Root");
            var k1 = conn.K2Gen<TestFrom, TestTo>(root1, 0);
            k1.Add(efk); k1.Commit();

            var k11 = conn.K2Gen<TestFrom, TestTo>(root1, 2);
            k11.Add(efk); k11.Commit();

            var k12 = conn.K2Gen<TestFrom, OtherTo>(root1, 1);
            k12.Add(ofk); k12.Commit();

            var root2 = conn.GenFrom<TestFrom>("sec Root");
            var k2 = conn.K2Gen<TestFrom, TestTo>(root2, 3);
            k2.Add(efk); k2.Commit();

            var k21 = conn.K2Gen<TestFrom, OtherTo>(root2, 2);
            k21.Add(ofk); k21.Commit();

            var k22 = conn.K2Gen<TestFrom, TestTo>(root2, 1);
            k22.Add(efk); k22.Commit();

            Assert.AreEqual(3, conn.Table<TestTo>().Count());
            Assert.AreEqual(3, conn.Table<OtherTo>().Count());

            k1.Clear(); k11.Clear(); k12.Clear();
            k2.Clear(); k21.Clear(); k22.Clear();

            Assert.AreEqual(2, conn.Table<TestTo>().Count());
            Assert.AreEqual(2, conn.Table<OtherTo>().Count());
        }
    }
}
