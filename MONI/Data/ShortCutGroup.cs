namespace MONI.Data
{
  public class ShortCutGroup
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

    public override string ToString()
    {
      return this.Key;
    }
  }
}