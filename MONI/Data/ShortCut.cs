using System;
using MONI.Util;
using Newtonsoft.Json;

namespace MONI.Data
{
  public class ShortCut : ViewModelBase//, IComparable, IComparable<ShortCut>
  {
    public ShortCut() {
      this.ID = Guid.NewGuid().ToString();
      this.ValidFrom = DateTime.MinValue;
      this.Expansion = string.Empty;
    }

    public ShortCut(string key, string expansion)
      : this() {
      this.Key = key;
      this.Expansion = expansion;
      }

    public ShortCut(string key, string expansion, DateTime validFrom)
      : this(key, expansion) {
      this.ValidFrom = validFrom;
    }

    [JsonIgnore]
    public string ID { get; set; }
    public string Key { get; set; }
    public string Expansion { get; set; }
    public bool WholeDayExpansion { get; set; }
    public DateTime ValidFrom { get; set; }

    private string group = string.Empty;

    public string Group {
      get { return this.group; }
      set {
        if (Equals(this.group, value)) {
          return;
        }
        this.group = value;
        this.OnPropertyChanged(() => this.Group);
      }
    }

    public override bool Equals(object obj) {
      if (obj is ShortCut) {
        var other = (ShortCut)obj;
        return this.ID == other.ID;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return this.ID.GetHashCode();
    }

    public void GetData(ShortCut sc) {
      this.Key = sc.Key;
      this.Expansion = sc.Expansion;
      this.WholeDayExpansion = sc.WholeDayExpansion;
      this.ValidFrom = sc.ValidFrom;
      this.Group = sc.Group;
    }

    public override string ToString() {
      return string.Format("{0}, {1}, {2}, {3}", this.Key, this.Expansion, this.ValidFrom, this.WholeDayExpansion);
    }
  }
}