using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace MonlistClone.Data {
  public class WorkDay :INotifyPropertyChanged {
    private readonly int day;
    private readonly int month;
    private readonly int year;

    public int Month {
      get { return this.month; }
    }

    public int Year {
      get { return this.year; }
    }

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

    public int Day {
      get { return this.day; }
    }

    // HACK
    private string originalString;
    public string OriginalString {
      get { return this.originalString; }
      set {
        if(this.originalString==value) {
          return;
        }
        this.originalString = value;
        // do parsing
        if(WorkDayParser.Instance!=null) {
          WorkDay wd = this;
          var result = WorkDayParser.Instance.Parse(value, ref wd);
          if(!result.Success) {
            // todo what now?

          }else {
            var tmp = this.PropertyChanged;
            if (tmp != null) {
              tmp(this, new PropertyChangedEventArgs("OriginalString"));
              tmp(this, new PropertyChangedEventArgs("HoursDuration"));
            }
          }
        }
      }
    }

    public double HoursDuration {
      get { return Items.Sum(i => i.End - i.Start); }
    }

    public DayOfWeek DayOfWeek { get; set; }
    public ObservableCollection<WorkItem> Items { get; set; }

    public override string ToString() {
      return string.Format("{0},items:{1},origString:{2}", this.DayOfWeek, this.Items.Count, this.OriginalString);
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}