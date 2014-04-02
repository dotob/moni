using System;
using Newtonsoft.Json;

namespace MONI.Data
{
  public class ShortCut
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
    }

    public override string ToString() {
      return string.Format("{0}, {1}, {2}, {3}", this.Key, this.Expansion, this.ValidFrom, this.WholeDayExpansion);
    }
  }
}