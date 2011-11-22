using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MonlistClone.Data {
  public class WorkMonth {
    private readonly int month;
    private readonly int year;

    public WorkMonth(int year, int month) {
      this.year = year;
      this.month = month;
      this.Weeks = new ObservableCollection<WorkWeek>();
      this.Days = new ObservableCollection<WorkDay>();

      var cal = new GregorianCalendar();
      WorkWeek lastWeek = null;
      for (int day = 1; day <= cal.GetDaysInMonth(year, month); day++) {
        var dt = new DateTime(year, month, day);
        WorkDay wd = new WorkDay(year, month, day);
        Days.Add(wd);
        var weekOfYear = cal.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        if (lastWeek == null || lastWeek.WeekOfYear != weekOfYear) {
          lastWeek = new WorkWeek(this, weekOfYear);
          this.Weeks.Add(lastWeek);
        }
        lastWeek.Days.Add(wd);
      }
    }

    public int Month { get { return month; } }

    public ObservableCollection<WorkWeek> Weeks { get; set; }
    public ObservableCollection<WorkDay> Days { get; set; }

    public override string ToString() {
      return string.Format("year:{0},month:{1},weeks:{2},days:{3}", year, month, Weeks.Count, Days.Count);
    }
  }
}