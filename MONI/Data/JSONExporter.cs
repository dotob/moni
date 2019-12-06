using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MONI.Data
{
    public class JSONExporter
    {
        private readonly string dataDirectory;

        public JSONExporter(string dataDirectory)
        {
            this.dataDirectory = Path.Combine(dataDirectory, "json");
            // check for dir
            if (!Directory.Exists(this.dataDirectory))
            {
                try
                {
                    Directory.CreateDirectory(this.dataDirectory);
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
            var pack = new JSONPackage {UserId = 123};
            var items = new List<JSONWorkItem>();
            foreach (var day in month.Days)
            {
                items.AddRange(day.Items.Select(item => new JSONWorkItem(item)));
            }

            pack.Items = items;

            var serializeObject = await Task.Run(() => JsonConvert.SerializeObject(pack, Formatting.Indented));

            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, true))
            using (var sw = new StreamWriter(stream))
            {
                await sw.WriteAsync(serializeObject);
                await sw.FlushAsync();
                await stream.FlushAsync();
            }
        }

        public string FilenameForMonth(WorkMonth month)
        {
            var filename = $"monlist_{month.Year}_{month.Month:00}.json";
            return Path.Combine(this.dataDirectory, filename);
        }
    }

    class JSONPackage
    {
        public int UserId { get; set; }
        public IEnumerable<JSONWorkItem> Items { get; set; }
    }

    class JSONWorkItem
    {
        public JSONWorkItem(WorkItem item)
        {
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