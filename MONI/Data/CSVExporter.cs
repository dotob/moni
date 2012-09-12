using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MONI.Data
{
  public class CSVExporter
  {
    private readonly string dataDirectory;

    public CSVExporter(string dataDirectory) {
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

    public void Export(WorkYear year) {
      foreach (var month in year.Months) {
        var dataFileName = string.Format("monlist_{0}_{1}.txt", year.Year, month.Month.ToString("00"));
        var dataFilePath = Path.Combine(this.dataDirectory, dataFileName);
        this.Export(month, dataFilePath);
      }
    }

    private void Export(WorkMonth month, string filename) {
      List<string> data = new List<string>();
      this.AddHeader(data);
      var gotData = this.AddData(month, data);
      if (gotData) {
        File.WriteAllLines(filename, data, Encoding.ASCII);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="month"></param>
    /// <param name="data"></param>
    /// <returns>if there was any data</returns>
    private bool AddData(WorkMonth month, List<string> data) {
      bool gotData = false;
      foreach (var day in month.Days) {
        foreach (var item in day.Items) {
          var description = string.IsNullOrEmpty(item.Description) ? "-" : item.Description;
          data.Add(string.Format(CultureInfo.InvariantCulture, " {0:00}     {1}    {2}    {3:00.00}   {4}    {5}      {6}", day.Day, item.Start.ToMonlistString(), item.End.ToMonlistString(), day.HoursDuration, item.Project, item.Position, description));
          gotData = true;
        }
      }
      return gotData;
    }

    private void AddHeader(List<string> data) {
      data.Add("Tag     Von     Bis     Std.    Prj-Nr   Pos.     durchgefuehrte Arbeiten");
      data.Add("========================================================================");
    }
  }
}