#if HELP_SQLITENET

using SQLite.Net;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSharpHelp
{
    #region Helpers (move?)
    // Helpers for more complex models here
    static class GlobalForKeyTo
    {
        public static SQLiteConnection conn;
    }
    public delegate T Proxy<T>(T input);
    public class KeyToIndex 
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [Indexed]
        public int fk_t { get; set; }
        [Indexed]
        public int fk_id { get; set; }
        [Indexed]
        public int fk_mid { get; set; }
        [Indexed]
        public int fk_pid { get; set; }
    }
    public interface IPrimaryKey
    {
        int id { get; set; }
    }
    public interface IKeyTo
    {
        void Clear();
        void Commit();
    }
    public class KeyTo<T> : IKeyTo where T : class, IPrimaryKey
    {
        public delegate int gint();
        readonly int pid, mid;
        gint idd;
        int id { get { return idd(); } }
        readonly Proxy<T> updateCandidate;
        public KeyTo(gint id, int pid, int mid, Proxy<T> updateCandidate = null)
        {
            GlobalForKeyTo.conn.CreateTable<KeyToIndex>();
            this.mid = mid; // id of model (class)  
            this.pid = pid; // id of prop on model
            this.idd = id; // id of instance of model connecting to.
            this.updateCandidate = updateCandidate;
        }
        TableQuery<T> IGet()
        {
            var jt = from k in GlobalForKeyTo.conn.Table<KeyToIndex>()
                     where k.fk_id == id && k.fk_mid == mid && k.fk_pid == pid
                     select k.fk_t;
            return GlobalForKeyTo.conn.Table<T>().Where(t => jt.Contains(t.id));
        }
        public IEnumerable<T> Get()
        {
            var ret = IGet();
            if (ToRemove.Count > 0) ret = ret.Where(k => !ToRemove.ContainsKey(k.id));
            if (ToAdd.Count > 0) return ret.Concat(ToAdd);
            return ret;
        }
        Dictionary<int, T> ToRemove = new Dictionary<int, T>();
        public void Remove(IEnumerable<T> values)
        {
            foreach (var v in values)
                if (!ToAdd.Remove(v))
                    ToRemove[v.id] = v;
        }
        HashSet<T> ToAdd = new HashSet<T>();
        public void Add(IEnumerable<T> values)
        {
            foreach (var v in values)
                if (!ToRemove.Remove(v.id))
                    ToAdd.Add(v);
        }
        public void Replace(IEnumerable<T> values)
        {
            ToRemove.Clear();
            ToAdd.Clear();
            IGet().Act(k => ToRemove[k.id] = k);
            Add(values);
        }

        public void Clear()
        {
            IGet().Delete(k => true);
        }

        public void Commit()
        {
            // base query
            var idx = GlobalForKeyTo.conn.Table<KeyToIndex>()
                .Where(k => k.fk_id == id && k.fk_mid == mid && k.fk_pid == pid);

            foreach (var v in ToRemove)
            {
                idx.Delete(d => d.fk_t == v.Value.id);
                if(idx.Count() == 0)
                    GlobalForKeyTo.conn.Delete<T>(v.Key);
            }
            ToRemove.Clear();

            foreach (var v in ToAdd)
            {
                // Need to get proper pk first
                var uc = updateCandidate?.Invoke(v);
                if (uc == null) GlobalForKeyTo.conn.Insert(v);
                else GlobalForKeyTo.conn.Update(uc);
                int add_id = (uc ?? v).id;

                if (idx.Count(k => k.fk_t == add_id) == 0)
                    GlobalForKeyTo.conn.Insert(new KeyToIndex
                    {
                        fk_id = id,
                        fk_mid = mid,
                        fk_pid = pid,
                        fk_t = add_id
                    });
            }
            ToAdd.Clear();
        }
    }
    #endregion
}
#endif