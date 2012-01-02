using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace MonlistClone.Data {
  public class WorkDay : INotifyPropertyChanged {
    private readonly int day;
    private readonly int month;
    private readonly int year;
    private string originalString;

    private WorkDay() {
      this.Items = new ObservableCollection<WorkItem>();
    }

    public WorkDay(int year, int month, int day) : this() {
      this.year = year;
      this.month = month;
      this.day = day;
      var dt = new DateTime(year, month, day);
      var cal = new GregorianCalendar();
      this.DayOfWeek = cal.GetDayOfWeek(dt);
    }

    public string Name {
      get {
        var now = DateTime.Now;
        if (now.Day == day && now.Month == month && now.Year == year) {
          return "today";
        }
        return string.Format("{0}_{1}_{2}", this.year, this.month, this.day);
      }
    }

    public int Month {
      get { return this.month; }
    }

    public int Year {
      get { return this.year; }
    }

    public int Day {
      get { return this.day; }
    }

    // HACK

    public string OriginalString {
      get { return this.originalString; }
      set {
        if (this.originalString == value) {
          return;
        }
        this.originalString = value;
        if (string.IsNullOrEmpty(this.originalString)) {
          this.Items.Clear();
          this.ImportantStuffChanged();
        } else {
          // do parsing
          if (WorkDayParser.Instance != null) {
            WorkDay wd = this;
            var result = WorkDayParser.Instance.Parse(value, ref wd);
            if (!result.Success) {
              // todo what now?
            } else {
              this.ImportantStuffChanged();
            }
          }
        }
      }
    }

    public double HoursDuration {
      get { return this.Items.Sum(i => i.End - i.Start); }
    }

    public DayOfWeek DayOfWeek { get; set; }
    public ObservableCollection<WorkItem> Items { get; set; }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    private void ImportantStuffChanged() {
      var tmp = this.PropertyChanged;
      if (tmp != null) {
        tmp(this, new PropertyChangedEventArgs("OriginalString"));
        tmp(this, new PropertyChangedEventArgs("HoursDuration"));
      }
    }

    public override string ToString() {
      return string.Format("{0},items:{1},origString:{2}", this.DayOfWeek, this.Items.Count, this.OriginalString);
    }
  }
}