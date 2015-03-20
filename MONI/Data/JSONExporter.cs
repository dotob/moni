using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MONI.Data
{
  public class JSONExporter
  {
    private readonly string dataDirectory;

    public JSONExporter(string dataDirectory) {
      this.dataDirectory = Path.Combine(dataDirectory, "json");
      // check for dir
      if (!Directory.Exists(this.dataDirectory)) {
          try {
            Directory.CreateDirectory(this.dataDirectory);
          } catch (Exception e) {
              Console.WriteLine(e);
          }
      }
    }

    public void Export(WorkYear year) {
      foreach (var month in year.Months) {
        var dataFileName = FilenameForMonth(month);
        this.Export(month, dataFileName);
      }
    }

    private void Export(WorkMonth month, string filename) {
      var pack = new JSONPackage();
      pack.UserId = 123;
      var items = new List<JSONWorkItem>();
      foreach (var day in month.Days) {
        foreach (var item in day.Items) {
           items.Add(new JSONWorkItem(item));
        }
      }
      pack.Items = items;
      var serializeObject = JsonConvert.SerializeObject(pack, Formatting.Indented);
      File.WriteAllText(filename, serializeObject);
    }

    public string FilenameForMonth(WorkMonth month) {
      var filename = string.Format("monlist_{0}_{1}.json", month.Year, month.Month.ToString("00"));
      return Path.Combine(this.dataDirectory, filename);
    }
  }

  class JSONPackage {
    public int UserId { get; set; }
    public IEnumerable<JSONWorkItem> Items { get; set; }
  }

  class JSONWorkItem {
    public JSONWorkItem(WorkItem item) {
      this.Day = item.WorkDay.Day;
      this.Month = item.WorkDay.Month;
      this.Year = item.WorkDay.Year;
      this.Start = item.Start.ToString();
      this.End = item.End.ToString();
      this.Project = item.Project;
      this.Position = item.Position;
      this.Description = item.Description;
    }

    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
    public string Project { get; set; }
    public string Position { get; set; }
    public string Description { get; set; }
  }
}