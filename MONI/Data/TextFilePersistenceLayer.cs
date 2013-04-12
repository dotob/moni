using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MONI.Util;

namespace MONI.Data {
  public class TextFilePersistenceLayer {
    private readonly string dataDirectory;
    private List<WorkDayPersistenceData> workDaysData = new List<WorkDayPersistenceData>();

    public TextFilePersistenceLayer(string dataDirectory)
    {
        this.dataDirectory = dataDirectory;
        // check for dir
        if (!Directory.Exists(dataDirectory)) {
            try {
                Directory.CreateDirectory(dataDirectory);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }

      public IEnumerable<WorkDayPersistenceData> WorkDaysData {
      get {
        return this.workDaysData;
      }
    }

    public ReadWriteResult ReadData() {
      ReadWriteResult ret = new ReadWriteResult { Success = true};
      try {
        var dataFiles = Directory.GetFiles(this.dataDirectory, "*md", SearchOption.TopDirectoryOnly);
        foreach (var dataFile in dataFiles) {
          if (File.Exists(dataFile)) {
            var readAllLines = File.ReadAllLines(dataFile);
            foreach (var wdLine in readAllLines) {
              string wdDateData = wdLine.Token("|", 1);
              var wdDateParts = wdDateData.Split(',').Select(s => Convert.ToInt32(s));
              WorkDayPersistenceData wdpd = new WorkDayPersistenceData();
              wdpd.Year = wdDateParts.ElementAt(0);
              wdpd.Month = wdDateParts.ElementAt(1);
              wdpd.Day = wdDateParts.ElementAt(2);

              string wdStringData = wdLine.Token("|", 2);
              wdpd.OriginalString = wdStringData.Replace("<br />", Environment.NewLine);
              this.workDaysData.Add(wdpd);
            }
          }
        }
      }
      catch (Exception exception) {
        ret.Success = false;
        ret.Error = exception.Message;
      }
      return ret;
    }

    public ReadWriteResult SaveData(WorkYear year) {
      foreach (var month in year.Months) {
        List<string> data = new List<string>();
        var changedWorkDays = month.Days.Where(wd => wd.IsChanged || !string.IsNullOrWhiteSpace(wd.OriginalString)).ToList();
        foreach (var workDay in changedWorkDays) {
          data.Add(string.Format("{0},{1},{2}|{3}", workDay.Year, workDay.Month, workDay.Day, workDay.OriginalString.Replace(Environment.NewLine, "<br />")));
        }
        var dataFileName = string.Format("{0}_{1}.md", year.Year, month.Month.ToString("00"));
        var dataFilePath = Path.Combine(this.dataDirectory, dataFileName);
        if (data.Any()) {
          File.WriteAllLines(dataFilePath, data);
        }
      }
      return new ReadWriteResult {Success = true};
    }

    public ReadWriteResult SetDataOfYear(WorkYear workYear) {
      var ret = ReadData();
      WorkDay errorDay = null;
      if (ret.Success) {
        try {
          foreach (WorkDayPersistenceData data in this.WorkDaysData.Where(wdpd => wdpd.Year == workYear.Year)) {
            var workDay = workYear.GetDay(data.Month, data.Day);
            errorDay = workDay;
            workDay.SetData(data.OriginalString);
          }
        }
        catch (Exception exception) {
          ret.Success = false;
          string errorMessage = exception.Message;
          if (errorDay != null) {
            errorMessage = string.Format("Beim Einlesen von {0} mit Originalstring {1} trat folgender Fehler auf: {2}", errorDay, errorDay.OriginalString, exception.Message);
          }
          ret.Error = errorMessage;
        }
      }
      return ret;
    }
  }

  public class WorkDayPersistenceData {
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public string OriginalString { get; set; }
  }

  public class ReadWriteResult
  {
    public string Error { get; set; }
    public bool Success { get; set; }
  }
}