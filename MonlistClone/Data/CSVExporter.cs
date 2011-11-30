using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MonlistClone.Data {
  public class CSVExporter {
    public void Export(WorkMonth month, string filename) {
      List<string> data = new List<string>();
      AddHeader(data);
      AddData(month, data);
      File.WriteAllLines(filename, data, Encoding.ASCII);
    }

    private void AddData(WorkMonth month, List<string> data) {
      foreach (var day in month.Days) {
        foreach (var item in day.Items) {
          var description = string.IsNullOrEmpty(item.Description)?"-":item.Description;
          data.Add(string.Format(CultureInfo.InvariantCulture, " {0:00}     {1}    {2}    {3:00.00}   {4}    {5}      {6}", day.Day, item.Start.ToMonlistString(), item.End.ToMonlistString(), day.HoursDuration, item.Project, item.Position, description));
        }
      }
    }

    private void AddHeader(List<string> data) {
      data.Add("Tag     Von     Bis     Std.    Prj-Nr   Pos.     durchgefuehrte Arbeiten");
      data.Add("========================================================================");
    }
  }
}