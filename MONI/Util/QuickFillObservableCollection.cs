using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MONI.Util
{
  public class QuickFillObservableCollection<T> : ObservableCollection<T>
  {
    private bool suspendCollectionChanged;

    public QuickFillObservableCollection() {
    }

    public QuickFillObservableCollection(IEnumerable<T> collection)
      : this() {
      this.Fill(collection);
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
      if (!this.suspendCollectionChanged) {
        base.OnCollectionChanged(e);
      }
    }

    public void Fill(IEnumerable<T> sourceItems, bool clearCollection = false) {
      if (sourceItems == null) {
        return;
      }
      var enumerator = sourceItems.GetEnumerator();
      if (!enumerator.MoveNext()) {
        return;
      }
      this.suspendCollectionChanged = true;
      try {
        if (clearCollection) {
          this.Clear();
        }
        this.InsertItem(this.Count, enumerator.Current);
        while (enumerator.MoveNext()) {
          this.InsertItem(this.Count, enumerator.Current);
        }
      } finally {
        this.suspendCollectionChanged = false;
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }
    }
  }
}