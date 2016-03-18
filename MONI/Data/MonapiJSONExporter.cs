using System.Collections.Generic;
using Newtonsoft.Json;

namespace MONI.Data {
    public class MonapiJSONExporter {
        private readonly int monlistUserNUmber;
        private readonly string dataDirectory;

        public MonapiJSONExporter(int monlistUserNUmber) {
            this.monlistUserNUmber = monlistUserNUmber;
        }

        public string Export(WorkMonth wmonth) {
            var month = new MonapiMonth();
            month.Nummer = this.monlistUserNUmber;
            month.Jahr = wmonth.Year;
            month.Monat = wmonth.Month;
            var items = new List<MonapiWorkItem>();
            foreach (var day in wmonth.Days) {
                foreach (var item in day.Items) {
                    items.Add(new MonapiWorkItem(item));
                }
            }
            month.Stunden = items;
            var serializedObject = JsonConvert.SerializeObject(month, Formatting.Indented);
            return serializedObject;
        }
    }

    class MonapiMonth {
        public int Nummer { get; set; }
        public int Jahr { get; set; }
        public int Monat { get; set; }
        public IEnumerable<MonapiWorkItem> Stunden { get; set; }
    }

    class MonapiWorkItem {
        public MonapiWorkItem(WorkItem item) {
            this.Tag = item.WorkDay.Day;
            this.Von = item.Start.ToString();
            this.Bis = item.End.ToString();
            this.Projekt = int.Parse(item.Project);
            this.Position = item.Position;
            this.Beschreibung = item.Description;
        }

        public int Tag { get; set; }
        public string Von { get; set; }
        public string Bis { get; set; }
        public int Projekt { get; set; }
        public string Position { get; set; }
        public string Beschreibung { get; set; }
    }
}