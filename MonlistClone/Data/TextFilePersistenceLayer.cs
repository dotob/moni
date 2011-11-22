using System;
using System.Collections.Generic;
using System.IO;
using MonlistClone.Util;
using System.Linq;

namespace MonlistClone.Data {
  public class TextFilePersistenceLayer {
    private List<WorkDayPersistenceData> workDaysData = new List<WorkDayPersistenceData>();

    public IEnumerable<WorkDayPersistenceData> WorkDaysData {
      get {
        return workDaysData;
      }
    }

    public void ReadData(string fileName) {
      if (File.Exists(fileName)) {
        var readAllLines = File.ReadAllLines(fileName);
        foreach (var wdLine in readAllLines) {
          string wdDateData = wdLine.Token('|', 1);
          var wdDateParts = wdDateData.Split(',').Select(s => Convert.ToInt32(s));
          WorkDayPersistenceData wdpd = new WorkDayPersistenceData();
          wdpd.Year = wdDateParts.ElementAt(0);
          wdpd.Month = wdDateParts.ElementAt(1);
          wdpd.Day = wdDateParts.ElementAt(2);

          string wdStringData = wdLine.Token('|', 2);
          wdpd.OriginalString = wdStringData;
          workDaysData.Add(wdpd);
        }
      }
    }

    public void SaveData(WorkYear year, string fileName) {
      List<string> data = new List<string>();
      foreach (var month in year.Months) {
        foreach (var workDay in month.Days.Where(wd=>!string.IsNullOrEmpty(wd.OriginalString))) {
          data.Add(string.Format("{0},{1},{2}|{3}", workDay.Year, workDay.Month, workDay.Day, workDay.OriginalString));
        }
      }
      File.WriteAllLines(fileName, data);
    }
  }

  public class WorkDayPersistenceData {
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public string OriginalString { get; set; }
  }
}