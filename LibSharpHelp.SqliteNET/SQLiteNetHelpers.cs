using SQLite.Net;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Collections;

namespace LibSharpHelp
{
    public class LockedEnumerable<T> : IEnumerable<T>
    {
        readonly ReaderWriterLockSlim Sync;
        readonly IEnumerable<T> from;
        public LockedEnumerable(IEnumerable<T> from, ReaderWriterLockSlim Sync)
        {
            this.from = from;
            this.Sync = Sync;
        }

        class LockedEnumerator : IEnumerator<T>
        {
            readonly ReaderWriterLockSlim Sync;
            readonly IEnumerator<T> from;
            public LockedEnumerator(IEnumerator<T> from, ReaderWriterLockSlim Sync)
            {
                this.from = from;
                this.Sync = Sync;
                Sync.EnterWriteLock();
            }
            public void Dispose()
            {
                from.Dispose();
                Sync.ExitWriteLock(); // feels risky, works with using and foreach, though.
            }
            public T Current { get { return from.Current; } }
            object IEnumerator.Current { get { return from.Current; } }
            public bool MoveNext() => from.MoveNext();
            public void Reset() => from.Reset();
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public IEnumerator<T> GetEnumerator()
        {
            return new LockedEnumerator(from.GetEnumerator(), Sync);
        }
    }
    public static class RLX
    {
        class dd : IDisposable
        {
            readonly Action d;
            public dd(Action d) { this.d = d; }
            public void Dispose() { d(); }
        }
        public static IDisposable ReadLock(this ReaderWriterLockSlim sync)
        {
            sync.EnterReadLock();
            return new dd(sync.ExitReadLock);
        }
        public static IDisposable WriteLock(this ReaderWriterLockSlim sync)
        {
            sync.EnterWriteLock();
            return new dd(sync.ExitWriteLock);
        }
    }

    [Table("KeyToIndex")]
    public class KeyToIndex 
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int id { get; set; } // our id
        [Indexed, Column("fk_t")]
        public int fk_t { get; set; } // id keying TO
        [Indexed, Column("fk_id")]
        public int fk_id { get; set; } // id keying FROM
        [Indexed, Column("fk_mid")]
        public int fk_mid { get; set; } // ident of FROM table
        [Indexed, Column("fk_tid")]
        public int fk_tid { get; set; } // ident of TO table
        [Indexed, Column("fk_pid")]
        public int fk_pid { get; set; } // ident of property on FROM table
        [Column("fk_pre_existing")]
        public bool fk_pre_existing { get; set; } // set if element already existed on TO table on first keying
    }
    
    public class SqliteKeyTo<F,T> where T : class where F : class
    {
        readonly ReaderWriterLockSlim Sync;
        readonly SQLiteConnection cconn;
        readonly Func<int> from_pk;
        readonly int from_pid, from_mid, to_mid;
        readonly Func<T, int> to_pk;
        readonly TableMapping ti, ki, fi;
        public SqliteKeyTo(
            Func<int> from_pk,
            Func<T, int> to_pk,
            int from_pid,
            int from_mid, 
            int to_mid,
            SQLiteConnection conn,
            TableMapping t_map,
            TableMapping f_map,
            ReaderWriterLockSlim Sync) 
        {
            if (Sync.RecursionPolicy != LockRecursionPolicy.SupportsRecursion)
                throw new ArgumentException("Sync must support recursion");
            this.Sync = Sync;
            this.cconn = conn;
            using(Sync.WriteLock())
                conn.CreateTable<KeyToIndex>();
            this.from_mid = from_mid;
            this.from_pid = from_pid;
            this.from_pk = from_pk;
            this.to_pk = to_pk;
            this.to_mid = to_mid;

            ti = t_map;
            ki = conn.GetMapping<KeyToIndex>();
            fi = f_map;
        }
        String IsKeyTo()
        {
            return String.Format(
                "{0} = KeyToIndex.fk_id and {1} = KeyToIndex.fk_mid and "+
                "{2} = KeyToIndex.fk_pid and {3} = KeyToIndex.fk_tid",
                from_pk(), from_mid, from_pid, to_mid
                );
        }
        void Execute(String q)
        {
            using(Sync.ReadLock())
                cconn.Execute(q);
        }
        X Scalar<X>(String q)
        {
            using (Sync.ReadLock())
                return cconn.ExecuteScalar<X>(q);
        }
        IEnumerable<X> Command<X>(String c, TableMapping map = null, params object[] args)
        {
            var cc = cconn.CreateCommand(c, args);
            var ret = map == null ? cc.ExecuteQuery<X>() : cc.ExecuteQuery<X>(map);
            return new LockedEnumerable<X>(ret, Sync);
        }
        void Insert<X>(X i, TableMapping m=null)
        {
            using (Sync.ReadLock())
            {
                if (m == null) cconn.Insert(i);
                else cconn.Insert(i, m);
            }
        }
        void DoBookkeeping()
        {
            var c1 = String.Format(
                "delete from KeyToIndex " +
                "where "+IsKeyTo()+" and KeyToIndex.fk_t not in (SELECT {0}.{1} from {0})",
            ti.TableName, ti.PK.Name);
            Execute(c1);
            var c2 = String.Format(
                "delete from KeyToIndex " +
                "where " + IsKeyTo() + " and KeyToIndex.fk_id not in (SELECT {0}.{1} from {0})",
            fi.TableName, fi.PK.Name);
            Execute(c2);
        }
        public IEnumerable<T> Get()
        {
            var c1 = String.Format(
                "select {0}.* from {0} join KeyToIndex on " +
                "KeyToIndex.fk_t = {0}.{1} where " + IsKeyTo(),
            ti.TableName, ti.PK.Name);

            var ret = Command<T>(c1, ti);
            // Patch non-committed stuff with linq
            if (ToRemove.Count > 0) ret = ret.Where(k => !ToRemove.ContainsKey(to_pk(k)));
            if (ToAdd.Count > 0) return ret.Concat(ToAdd);
            return ret;

        }
        Dictionary<int, T> ToRemove = new Dictionary<int, T>();
        public void Remove(IEnumerable<T> values)
        {
            foreach (var v in values)
                if (!ToAdd.Remove(v))
                    ToRemove[to_pk(v)] = v;
        }
        HashSet<T> ToAdd = new HashSet<T>();
        public void Add(IEnumerable<T> values)
        {
            foreach (var v in values)
                if (!ToRemove.Remove(to_pk(v)))
                    ToAdd.Add(v);
        }
        public void Replace(IEnumerable<T> values)
        {
            ToRemove.Clear();
            ToAdd.Clear();
            Get().Act(k => ToRemove[to_pk(k)] = k);
            Add(values);
        }

        public void Clear()
        {
            ToRemove.Clear();
            ToAdd.Clear();
            foreach (var ex in Command<KeyToIndex>("select * from KeyToIndex where " + IsKeyTo()))
                Remove(ex);
        }

        void Remove(KeyToIndex k)
        {
            var nperem = Scalar<int>(
                "select count(id) from KeyToIndex where fk_t = "
                + k.fk_t + " and not fk_pre_existing and fk_tid = " + to_mid);
            if (nperem == 1) Execute(
                String.Format("delete from {0} where {1}={2}",
                ti.TableName, ti.PK.Name, k.fk_t)); // this ishax, think about it.
            Execute("delete from KeyToIndex where id="+k.id);
        }

        public void Commit()
        {
            DoBookkeeping();

            foreach (var kv in ToRemove)
                Remove(Command<KeyToIndex>("select * from KeyToIndex where " + IsKeyTo() + " and fk_t = " + kv.Key + " limit 1").First());

            foreach (var v in ToAdd)
            {
                // if it doesnt exist, 
                bool pre_existing = Scalar<int>(String.Format("select count(id) from {0} where {1}={2}", ti.TableName, ti.PK.Name, to_pk(v))) > 0;
                bool rec_pre_exist = Scalar<int>("select count(id) from KeyToIndex where fk_t = " + to_pk(v)) > 0;
                bool any_rec_pre_exist = Scalar<int>("select count(id) from KeyToIndex where fk_t = " + to_pk(v) + " and fk_pre_existing" ) > 0;
                // Need to get proper pk first
                if (!pre_existing) cconn.Insert(v, ti);
                int add_id = to_pk(v);
                // Always add
                Insert(new KeyToIndex
                {
                    fk_id = from_pk(),
                    fk_mid = from_mid,
                    fk_pid = from_pid,
                    fk_tid = to_mid,
                    fk_t = add_id,
                    fk_pre_existing = any_rec_pre_exist || (pre_existing && !rec_pre_exist)
                });
            }
            ToAdd.Clear();
        }
    }
}
