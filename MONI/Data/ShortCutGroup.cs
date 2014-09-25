using System;

namespace MONI.Data
{
  public class ShortCutGroup : IComparable, IComparable<ShortCutGroup>
  {
    public ShortCutGroup()
    {
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