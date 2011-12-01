using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace MonlistClone.Data {
  public class WorkMonth : INotifyPropertyChanged {
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
        this.Days.Add(wd);
        var weekOfYear = cal.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        if (lastWeek == null || lastWeek.WeekOfYear != weekOfYear) {
          lastWeek = new WorkWeek(this, weekOfYear);
          lastWeek.PropertyChanged += new PropertyChangedEventHandler(this.WeekPropertyChanged);
          this.Weeks.Add(lastWeek);
        }
        lastWeek.AddDay(wd);
      }
    }


    public double HoursDuration {
      get { return this.Weeks.Sum(i => i.HoursDuration); }
    }

    public int Month {
      get { return this.month; }
    }

    public ObservableCollection<WorkWeek> Weeks { get; set; }
    public ObservableCollection<WorkDay> Days { get; set; }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    private void WeekPropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == "HoursDuration") {
        var tmp = this.PropertyChanged;
        if (tmp != null) {
          tmp(this, new PropertyChangedEventArgs("HoursDuration"));
        }
      }
    }

    public override string ToString() {
      return string.Format("year:{0},month:{1},weeks:{2},days:{3}", this.year, this.month, this.Weeks.Count, this.Days.Count);
    }
  }
}