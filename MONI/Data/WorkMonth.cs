using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace MONI.Data
{
  public class WorkMonth : INotifyPropertyChanged
  {
    private readonly int month;
    private readonly int year;

    public WorkMonth(int year, int month, IEnumerable<SpecialDate> specialDates, IEnumerable<ShortCut> shortCuts) {
      this.year = year;
      this.month = month;
      this.Weeks = new ObservableCollection<WorkWeek>();
      this.Days = new ObservableCollection<WorkDay>();
      this.ShortCutStatistic = new ObservableCollection<KeyValuePair<string, ShortCutStatistic>>(shortCuts.Select(s => new KeyValuePair<string, ShortCutStatistic>(s.Key, new ShortCutStatistic(s))));

      var cal = new GregorianCalendar();
      WorkWeek lastWeek = null;
      for (int day = 1; day <= cal.GetDaysInMonth(year, month); day++) {
        var dt = new DateTime(year, month, day);

        WorkDay wd = new WorkDay(year, month, day, specialDates);
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

    private double previewHours;
    public double PreviewHours {
      get { return this.previewHours; }
      set {
        if (this.previewHours == value) {
          return;
        }
        this.previewHours = value;
        var tmp = this.PropertyChanged;
        if (tmp != null) {
          tmp(this, new PropertyChangedEventArgs("PreviewHours"));
        }
      }
    }

    public double HoursDuration {
      get { return this.Weeks.Sum(i => i.HoursDuration); }
    }

    public int Month {
      get { return this.month; }
    }

    public string MonthName {
      get {
        switch (this.month) {
          case 1:
            return "Januar";
          case 2:
            return "Februar";
          case 3:
            return "März";
          case 4:
            return "April";
          case 5:
            return "Mai";
          case 6:
            return "Juni";
          case 7:
            return "Juli";
          case 8:
            return "August";
          case 9:
            return "September";
          case 10:
            return "Oktober";
          case 11:
            return "November";
          case 12:
            return "Dezember";
        }
        return string.Empty;
      }
    }

    public ObservableCollection<WorkWeek> Weeks { get; set; }
    public ObservableCollection<WorkDay> Days { get; set; }

    public ObservableCollection<KeyValuePair<string, ShortCutStatistic>> ShortCutStatistic { get; set; }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    private void WeekPropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == "HoursDuration") {
        var tmp = this.PropertyChanged;
        if (tmp != null) {
          tmp(this, new PropertyChangedEventArgs("HoursDuration"));
        }
        this.CalcPreviewHours();
        this.CalcShortCutStatistic();
      }
    }

    private void CalcShortCutStatistic() {
      foreach (var kvp in this.ShortCutStatistic) {
        KeyValuePair<string, ShortCutStatistic> kvp1 = kvp;
        kvp.Value.UsedInMonth = this.Days.SelectMany(d => d.Items).Where(i => i.ShortCut != null).Where(i => kvp1.Key == i.ShortCut.Key).Sum(i => i.HoursDuration);
      }
    }

    private void CalcPreviewHours() {
      this.PreviewHours = this.HoursDuration + this.Weeks.SelectMany(w => w.Days).Count(d => d.DayType == DayType.Working && d.HoursDuration == 0) * 8;
    }

    public override string ToString() {
      return string.Format("year:{0},month:{1},weeks:{2},days:{3}", this.year, this.month, this.Weeks.Count, this.Days.Count);
    }
  }
}