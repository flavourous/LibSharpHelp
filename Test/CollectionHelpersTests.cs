using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSharpHelp.Test
{
    [TestFixture]
    public class CollectionHelpersTests
    {
        [Test]
        public void DispatchedObservableCollection()
        {
            // Observers
            bool dispatched = false;
            Action<Action> disp = a => { dispatched = true; a(); dispatched = false; };
            NotifyCollectionChangedEventHandler cch = (e, c) => Assert.IsTrue(dispatched);
            PropertyChangedEventHandler pch = (e, c) => Assert.IsTrue(dispatched);

            // Setup
            var doc = new DispatchedObservableCollection<String>();
            doc.Dispatcher = disp;
            (doc as INotifyCollectionChanged).CollectionChanged += cch;
            (doc as INotifyPropertyChanged).PropertyChanged += pch;
            (doc as ObservableCollection<String>).CollectionChanged += cch;
            doc.CollectionChanged += cch;

            // Test
            doc.Add("Test");
            doc.RemoveAt(0);

        }
    }
}
