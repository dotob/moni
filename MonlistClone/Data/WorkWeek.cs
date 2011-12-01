using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MonlistClone.Data {
  public class WorkWeek : INotifyPropertyChanged {
    private readonly int weekOfYear;
    private ObservableCollection<WorkDay> days;

    public WorkWeek(WorkMonth month, int weekOfYear) {
      this.Month = month;
      this.weekOfYear = weekOfYear;
      this.days = new ObservableCollection<WorkDay>();
    }

    public WorkMonth Month { get; set; }

    public int WeekOfYear {
      get { return this.weekOfYear; }
    }

    public double HoursDuration {
      get { return this.Days.Sum(i => i.HoursDuration); }
    }

    public IEnumerable<WorkDay> Days {
      get { return this.days; }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    public override string ToString() {
      if (this.Days.Any()) {
        return string.Format("kw:{0},days:{1},firstday:{2},lastday:{3}", this.weekOfYear, this.days.Count, this.Days.First().DayOfWeek, this.Days.Last().DayOfWeek);
      }
      return string.Format("kw:{0},days:{1},empty", this.weekOfYear, this.days.Count);
    }

    public void AddDay(WorkDay wd) {
      this.days.Add(wd);
      wd.PropertyChanged += this.WDPropertyChanged;
    }

    private void WDPropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == "HoursDuration") {
        var tmp = this.PropertyChanged;
        if (tmp != null) {
          tmp(this, new PropertyChangedEventArgs("HoursDuration"));
        }
      }
    }
  }
}