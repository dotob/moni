using System;
using System.Collections.Generic;

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
  }

  public class SpecialDate
  {
    public string Name { get; set; }
    public DateTime Date { get; set; }
  }

  public class WorkDayParserSettings
  {
    public WorkDayParserSettings() {
      this.ProjectAbbreviations = new Dictionary<string, string>();
    }

    public bool InsertDayBreak { get; set; }
    public TimeItem DayBreakTime { get; set; }
    public int DayBreakDurationInMinutes { get; set; }
    public Dictionary<string, string> ProjectAbbreviations { get; set; }
  }
}