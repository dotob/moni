using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MONI.Util;
using NLog;

namespace MONI.Data {
  public class TextFilePersistenceLayer
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private readonly string dataDirectory;
    private List<WorkDayPersistenceData> workDaysData = new List<WorkDayPersistenceData>();

    public TextFilePersistenceLayer(string dataDirectory) {
      this.dataDirectory = dataDirectory;
      // check for dir
      if (!Directory.Exists(dataDirectory)) {
        try {
          logger.Debug("try to create data dir: {0}", dataDirectory);
          Directory.CreateDirectory(dataDirectory);
        }
        catch (Exception e) {
          logger.Error("failed to create data dir: {0} with: {1}", dataDirectory, e);
        }
      }
    }

    public IEnumerable<WorkDayPersistenceData> WorkDaysData {
      get { return this.workDaysData; }
    }

    public int EntryCount {
      get { return this.workDaysData.Count; }
    }

    public DateTime FirstEntryDate {
      get {
        var workDayPersistenceData = this.workDaysData.FirstOrDefault();
        if (workDayPersistenceData != null) {
          return workDayPersistenceData.Date;
        }
        return DateTime.Today;
      }
    }

    public ReadWriteResult ReadData() {
      ReadWriteResult ret = new ReadWriteResult {Success = true};
      try {
        var dataFiles = Directory.GetFiles(this.dataDirectory, "*md", SearchOption.TopDirectoryOnly);
        foreach (var dataFile in dataFiles) {
          logger.Debug("start reading data from file: {0}", dataFile);
          if (File.Exists(dataFile)) {
            var readAllLines = File.ReadAllLines(dataFile);
            int i = 0;
            foreach (var wdLine in readAllLines) {
              string wdDateData = wdLine.Token("|", 1);
              var wdDateParts = wdDateData.Split(',').Select(s => Convert.ToInt32(s));
              WorkDayPersistenceData wdpd = new WorkDayPersistenceData();
              wdpd.Year = wdDateParts.ElementAt(0);
              wdpd.Month = wdDateParts.ElementAt(1);
              wdpd.Day = wdDateParts.ElementAt(2);
              wdpd.LineNumber = i;
              wdpd.FileName = dataFile;

              string wdStringData = wdLine.Token("|", 2);
              wdpd.OriginalString = wdStringData.Replace("<br />", Environment.NewLine);
              this.workDaysData.Add(wdpd);
              i++;
            }
          }
        }
      }
      catch (Exception exception) {
        ret.Success = false;
        ret.Error = exception.Message;
        logger.Error(exception, "readdata failed");
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
      if (ret.Success) {
        foreach (WorkDayPersistenceData data in this.WorkDaysData.Where(wdpd => wdpd.Year == workYear.Year).ToList()) {
          var workDay = workYear.GetDay(data.Month, data.Day);
          WorkDay errorDay = workDay;
          try {
            workDay.SetData(data.OriginalString);
          }
          catch (Exception exception) {
            ret.ErrorCount++;
            // only remember first error for now
            if (ret.Success) {
              ret.Success = false;
              string errorMessage = exception.Message;
              if (errorDay != null) {
                errorMessage = string.Format("Beim Einlesen von {0} in der Datei {1} trat in Zeile {2} folgender Fehler auf: {3}", errorDay, data.FileName, data.LineNumber, exception.Message);
                logger.Error(errorMessage);
              }
              ret.Error = errorMessage;
            }
          }
        }
        if (ret.ErrorCount > 1) {
          ret.Error += string.Format("\n\n\nEs liegen noch {0} weitere Fehler vor.", ret.ErrorCount);
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
    public int LineNumber { get; set; }
    public string FileName { get; set; }
    public DateTime Date {
      get { return new DateTime(this.Year, this.Month, this.Day);}
    }
  }

  public class ReadWriteResult
  {
    public string Error { get; set; }
    public bool Success { get; set; }
    public int ErrorCount { get; set; }
  }
}