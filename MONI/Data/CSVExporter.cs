using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MONI.Data
{
    public class CSVExporter
    {
        private readonly string dataDirectory;

        public CSVExporter(string dataDirectory)
        {
            this.dataDirectory = dataDirectory;
            // check for dir
            if (!Directory.Exists(dataDirectory))
            {
                try
                {
                    Directory.CreateDirectory(dataDirectory);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public async Task ExportAsync(WorkYear year)
        {
            var listOfTasks = year.Months.Select(month => this.ExportAsync(month, FilenameForMonth(month))).ToList();
            await Task.WhenAll(listOfTasks);
        }

        private async Task ExportAsync(WorkMonth month, string filename)
        {
            var data = new List<string>();
            this.AddHeader(data);
            var gotData = this.AddData(month, data);
            if (gotData)
            {
                using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, true))
                using (var sw = new StreamWriter(stream, Encoding.GetEncoding("ISO-8859-1")))
                {
                    foreach (var line in data)
                    {
                        await sw.WriteLineAsync(line);
                    }

                    await sw.FlushAsync();
                    await stream.FlushAsync();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="month"></param>
        /// <param name="data"></param>
        /// <returns>if there was any data</returns>
        private bool AddData(WorkMonth month, List<string> data)
        {
            bool gotData = false;
            foreach (var day in month.Days)
            {
                foreach (var item in day.Items)
                {
                    var description = string.IsNullOrEmpty(item.Description) ? "-" : item.Description;
                    data.Add(string.Format(CultureInfo.InvariantCulture, " {0:00}     {1}    {2}    {3:00.00}   {4}    {5}      {6}", day.Day, item.Start.ToMonlistString(), item.End.ToMonlistString(), item.HoursDuration, item.Project, item.Position, description));
                    gotData = true;
                }
            }

            return gotData;
        }

        private void AddHeader(List<string> data)
        {
            data.Add("Tag     Von     Bis     Std.    Prj-Nr   Pos.     durchgefuehrte Arbeiten");
            data.Add("========================================================================");
        }

        public string FilenameForMonth(WorkMonth month)
        {
            var filename = $"monlist_{month.Year}_{month.Month:00}.txt";
            return Path.Combine(this.dataDirectory, filename);
        }
    }
}