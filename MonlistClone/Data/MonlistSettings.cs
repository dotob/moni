using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MonlistClone.Data
{
  public class MonlistSettings
  {
    public WorkDayParserSettings ParserSettings { get; set; }
    public MainSettings MainSettings { get; set; }
  }

  public class MainSettings
  {
    public List<SpecialDate> SpecialDates { get; set; }
    public string DataDirectory { get; set; }
  }

  public class SpecialDate
  {
    public string Name { get; set; }
    public DateTime Date { get; set; }
  }

  public class WorkDayParserSettings
  {
    public WorkDayParserSettings() {
      this.ShortCuts = new Dictionary<string, ShortCut>();
    }

    [JsonIgnore]
    public Dictionary<string, ShortCut> AllCurrentShortcuts {
      get { return this.GetValidShortCuts(DateTime.Now); }
    }

    private Dictionary<string, ShortCut> GetValidShortCuts(DateTime from) {
      return this.ShortCuts;
    }

    public bool InsertDayBreak { get; set; }
    public TimeItem DayBreakTime { get; set; }
    public int DayBreakDurationInMinutes { get; set; }
    public Dictionary<string, ShortCut> ShortCuts { get; set; }
  }

  public class ShortCut
  {
    public ShortCut() {
      this.ValidFrom = DateTime.MinValue;
      this.Expansion = string.Empty;
    }

    public ShortCut(string expansion)
      : this() {
      this.Expansion = expansion;
    }

    public string Expansion { get; set; }
    public bool WholeDayExpansion { get; set; }
    public DateTime ValidFrom { get; set; }
  }

  public class ShortCutStatistic : ShortCut, INotifyPropertyChanged
  {
    public ShortCutStatistic(ShortCut sc)
      : base(sc.Expansion) {
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