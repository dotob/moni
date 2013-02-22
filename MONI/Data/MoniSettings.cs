using System;
using System.Collections.Generic;
using MahApps.Metro.Native;
using Newtonsoft.Json;
using System.Linq;

namespace MONI.Data
{
  public class MoniSettings
  {
    public WorkDayParserSettings ParserSettings { get; set; }
    public MainSettings MainSettings { get; set; }
    public static MoniSettings GetEmptySettings() {
      var ms = new MainSettings { DataDirectory = AppDomain.CurrentDomain.BaseDirectory };
      return new MoniSettings { ParserSettings = new WorkDayParserSettings(), MainSettings = ms };
    }
  }

  public class MainSettings
  {
    public string DataDirectory { get; set; }
    public string MonlistExecutablePath { get; set; }
    public int HitListLookBackInWeeks { get; set; }
    public bool ShowProjectHitList { get; set; }
    public WINDOWPLACEMENT? Placement { get; set; }
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

}