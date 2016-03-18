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
      var ms = new MainSettings {
	      DataDirectory = AppDomain.CurrentDomain.BaseDirectory, 
				MonlistExecutablePath = @"n:\Monlist2\Monlist2.exe", 
				ProjectNumberFilePath = @"n:\Monlist2\projekte.txt", 
				UpdateInfoURL = "http://mtools/moni_updates.json", 
				ShowMonthList = true
      };
      return new MoniSettings { ParserSettings = new WorkDayParserSettings(), MainSettings = ms };
    }

    public static MoniSettings Current { get; set; }
  }

  public class MainSettings
  {
    public MainSettings() {
      HoursPerDay = 8;
      ShowMonthList = true;
    }

    public string DataDirectory { get; set; }
    public string MonlistExecutablePath { get; set; }
    public string MonlistEmployeeNumber { get; set; }
    public int MonlistGBNumber { get; set; }
    public int HitListLookBackInWeeks { get; set; }
    public bool ShowProjectHitList { get; set; }
    public bool ShowPositionHitList { get; set; }
    public WINDOWPLACEMENT? Placement { get; set; }
    public float HoursPerDay { get; set; }
    public string ProjectNumberFilePath { get; set; }
    public string PositionNumberFilePath { get; set; }
    public string UpdateInfoURL { get; set; }
    public bool ShowMonthList { get; set; }
	  public bool UseMonApi { get; set; }
  }

  public class WorkDayParserSettings
  {
    public WorkDayParserSettings() {
      this.ShortCutGroups = new List<ShortCutGroup>();
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
    public List<ShortCutGroup> ShortCutGroups { get; set; }
  }

}