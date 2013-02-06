using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Linq;

namespace MONI.Data
{
  public class MoniSettings
  {
    public WorkDayParserSettings ParserSettings { get; set; }
    public MainSettings MainSettings { get; set; }
    public static MoniSettings GetEmptySettings() {
      var ms = new MainSettings { SpecialDates = new List<SpecialDate>(), DataDirectory = AppDomain.CurrentDomain.BaseDirectory };
      return new MoniSettings { ParserSettings = new WorkDayParserSettings(), MainSettings = ms };
    }
  }

  public class MainSettings
  {
    public List<SpecialDate> SpecialDates { get; set; }
    public string DataDirectory { get; set; }
    public int HitListLookBackInWeeks { get; set; }
  }

  public class SpecialDate
  {
    public string Name { get; set; }
    public DateTime Date { get; set; }
  }

  public class WorkDayParserSettings
  {
    public WorkDayParserSettings() {
      this.ShortCuts = new List<ShortCut>();
    }

    [JsonIgnore]
    public List<ShortCut> AllCurrentShortcuts {
      get { return this.GetValidShortCuts(DateTime.Now); }
    }

    public List<ShortCut> GetValidShortCuts(DateTime from) {
      var allShortcuts = this.ShortCuts;
      return ValidShortCuts(allShortcuts, from);
    }

    public static List<ShortCut> ValidShortCuts(IEnumerable<ShortCut> allShortcuts, DateTime testDate) {
      var ret = new List<ShortCut>();
      var groupedByKey = allShortcuts.GroupBy(sc => sc.Key);
      foreach (var keyedShortcut in groupedByKey) {
        if (keyedShortcut.Count() > 1) {
          foreach (var groupedByExpansionType in keyedShortcut.GroupBy(ks => ks.WholeDayExpansion)) {
            var lastOrDefault = groupedByExpansionType.OrderBy(sc => sc.ValidFrom).LastOrDefault(sc => sc.ValidFrom <= testDate);
            if (lastOrDefault != null) {
              ret.Add(lastOrDefault);
            }
          }
        } else {
          ret.Add(keyedShortcut.Single());
        }
      }
      return ret;
    }

    public bool InsertDayBreak { get; set; }
    public TimeItem DayBreakTime { get; set; }
    public int DayBreakDurationInMinutes { get; set; }
    public List<ShortCut> ShortCuts { get; set; }
  }

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
      ValidFrom = validFrom;
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

    public void GetData(ShortCut sc) {
      this.Key = sc.Key;
      this.Expansion = sc.Expansion;
      this.WholeDayExpansion = sc.WholeDayExpansion;
      this.ValidFrom = sc.ValidFrom;
    }

    public override string ToString() {
      return string.Format("{0}, {1}, {2}, {3}", Key, Expansion, ValidFrom, WholeDayExpansion);
    }
  }

  public class ShortCutStatistic : ShortCut, INotifyPropertyChanged
  {
    public ShortCutStatistic(ShortCut sc)
      : base(sc.Key, sc.Expansion) {
      this.ID = sc.ID;
      this.WholeDayExpansion = sc.WholeDayExpansion;
      this.ValidFrom = sc.ValidFrom;
    }

    private double usedInMonth;
    public double UsedInMonth {
      get { return this.usedInMonth; }
      set {
        if (this.usedInMonth == value) {
          return;
        }
        this.usedInMonth = value;
        var tmp = this.PropertyChanged;
        if (tmp != null) {
          tmp(this, new PropertyChangedEventArgs("UsedInMonth"));
        }
      }
    }

    #region Implementation of INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}