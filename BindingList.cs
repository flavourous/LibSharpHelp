using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LibSharpHelp
{
    public enum ListChangedType
    {
        Reset = 0, ItemAdded = 1, ItemDeleted = 2, ItemMoved = 3, ItemChanged = 4,
    }
    public class ListChangedEventArgs : EventArgs
    {
        public ListChangedEventArgs(ListChangedType listChangedType, int newIndex, int oldIndex = -1)
        {
            ListChangedType = listChangedType;
            NewIndex = newIndex;
            OldIndex = oldIndex;
        }
        public ListChangedType ListChangedType { get; private set; }
        public int NewIndex { get; }
        public int OldIndex { get; }
    }
    public delegate void ListChangedEventHandler(object sender, ListChangedEventArgs e);

    public interface IBindingList : IList
    {
        event ListChangedEventHandler ListChanged;
    }
    public interface IBindingList<T> : IList<T>, IBindingList { }

    public class BindingList<T> : IBindingList<T>
    {
        readonly List<T> backing;
        public BindingList() { backing = new List<T>(); }
        public BindingList(IList<T> list) {backing = new List<T>(list); }

        // IBindingList<T>
        public event ListChangedEventHandler ListChanged = delegate { };
        void olc(int index, ListChangedType lct)
        {
            int ni = lct == ListChangedType.ItemDeleted ? -1 : index;
            int oi = lct != ListChangedType.ItemDeleted ? -1 : index;
            ListChanged(this, new ListChangedEventArgs(lct, ni, oi));
        }

        // IList<T>
        IList<T> b { get { return backing as IList<T>; } }
        public T this[int index] { get { return b[index]; } set { b[index] = value; olc(index, ListChangedType.ItemChanged); } }
        public int Count { get { return b.Count; } }
        public bool IsReadOnly { get { return b.IsReadOnly; } }
        public void Add(T item) { b.Add(item); olc(b.Count - 1, ListChangedType.ItemAdded); }
        public void Clear() { b.Clear(); olc(-1, ListChangedType.Reset); }
        public bool Contains(T item) { return b.Contains(item); }
        public void CopyTo(T[] array, int arrayIndex) { b.CopyTo(array, arrayIndex); }
        public IEnumerator<T> GetEnumerator() { return b.GetEnumerator(); }
        public int IndexOf(T item) { return b.IndexOf(item); }
        public void Insert(int index, T item) { b.Insert(index, item); olc(index, ListChangedType.ItemAdded);  }
        public bool Remove(T item)
        {
            int ii = IndexOf(item);
            bool bv = b.Remove(item);
            if (ii > -1) olc(ii, ListChangedType.ItemDeleted);
            return bv;
        }
        public void RemoveAt(int index) { b.RemoveAt(index); olc(index, ListChangedType.ItemDeleted); }
        IEnumerator IEnumerable.GetEnumerator() { return (backing as IEnumerable).GetEnumerator(); }

        // Ilist
        IList l { get { return backing as IList; } }
        public bool IsFixedSize { get { return l.IsFixedSize; } }
        public bool IsSynchronized { get { return l.IsSynchronized; } }
        public object SyncRoot { get { return l.SyncRoot; } }
        object IList.this[int index] { get { return this[index]; } set { this[index] = (T)value; } }
        public int Add(object value) { int rv = l.Add(value); olc(l.Count - 1, ListChangedType.ItemAdded); return rv; }
        public bool Contains(object value) { return l.Contains(value); }
        public int IndexOf(object value) { return l.IndexOf(value); }
        void IList.Insert(int index, object value) { this.Insert(index, (T)value); }
        void IList.Remove(object value) { this.Remove((T)value); }
        public void CopyTo(Array array, int index) { l.CopyTo(array, index); }
    }
}
