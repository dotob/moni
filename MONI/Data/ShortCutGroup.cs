using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MONI.Util;
using Newtonsoft.Json;

namespace MONI.Data
{
  public class ShortCutGroup : ViewModelBase, IComparable, IComparable<ShortCutGroup>
  {
    public ShortCutGroup()
    {
      this.ShortCuts = new ObservableCollection<ShortCut>();
    }

    public void AddShortCut(ShortCut shortCut)
    {
      if (shortCut == null || this.ShortCuts.Contains(shortCut)) {
        return;
      }
      this.ShortCuts.Add(shortCut);
    }

    public void AddShortCuts(IEnumerable<ShortCut> shortCutList)
    {
      if (shortCutList == null) {
        return;
      }
      foreach (var shortCut in shortCutList) {
        this.ShortCuts.Add(shortCut);
      }
    }

    public void RemoveShortCut(ShortCut shortCut)
    {
      if (shortCut == null) {
        return;
      }
      this.ShortCuts.Remove(shortCut);
    }

    private ObservableCollection<ShortCut> shortCuts;

    [JsonIgnore]
    public ObservableCollection<ShortCut> ShortCuts
    {
      get { return this.shortCuts; }
      protected set
      {
        if (!Equals(value, this.ShortCuts)) {
          this.shortCuts = value;
          this.OnPropertyChanged(() => this.ShortCuts);
        }
      }
    }

    public string Key { get; set; }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      if (obj.GetType() != this.GetType()) {
        return false;
      }
      return this.Equals((ShortCutGroup)obj);
    }

    protected bool Equals(ShortCutGroup other)
    {
      return string.Equals(this.Key, other.Key);
    }

    public override int GetHashCode()
    {
      return (this.Key != null ? this.Key.GetHashCode() : 0);
    }

    public int CompareTo(object obj)
    {
      return this.CompareTo(obj as ShortCutGroup);
    }

    public int CompareTo(ShortCutGroup other)
    {
      return String.Compare(this.Key, other.Key, StringComparison.InvariantCulture);
    }

    public override string ToString()
    {
      return this.Key;
    }
  }
}