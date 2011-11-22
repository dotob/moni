using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace MonlistClone.Data {
  public class WorkYear {
    public int Year { get; set; }

    public WorkYear(int year) {
      this.Year = year;
      this.Months = new ObservableCollection<WorkMonth>();
      this.Weeks = new ObservableCollection<WorkWeek>();

      var cal = new GregorianCalendar();
      for (int month = 1; month <= cal.GetMonthsInYear(year); month++) {
        WorkMonth wm = new WorkMonth(year, month);
        this.Months.Add(wm);
        foreach (var workWeek in wm.Weeks) {
          this.Weeks.Add(workWeek);
        }
      }
    }

    public ObservableCollection<WorkMonth> Months { get; set; }
    public ObservableCollection<WorkWeek> Weeks { get; set; }

    public override string ToString() {
      return string.Format("{0}:{1} months", this.Year, this.Months.Count);
    }

    public WorkDay GetDay(int month, int day) {
      return this.Months.ElementAt(month-1).Days.ElementAt(day-1);
    }
  }
}