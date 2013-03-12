using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using MONI.Data.SpecialDays;

namespace MONI.Data {
  public class WorkDay : INotifyPropertyChanged, IDataErrorInfo {
    private readonly int day;
    private readonly int month;
    private readonly int year;
    private string originalString;
    private DayType dayType;
    private ObservableCollection<WorkItem> items;
    private WorkDayParserResult lastParseResult;
    private bool isChanged;

    private WorkDay() {
      this.items = new ObservableCollection<WorkItem>();
    }

    public WorkDay(int year, int month, int day, GermanSpecialDays specialDays) : this() {
      this.year = year;
      this.month = month;
      this.day = day;
      var dt = new DateTime(year, month, day);
      var cal = new GregorianCalendar();
      this.DayOfWeek = cal.GetDayOfWeek(dt);
      GermanSpecialDay specialDay;
      this.DayType = this.calculateDayType(dt, this.DayOfWeek, specialDays, out specialDay);
      this.SpecialDay = specialDay;
    }


    private DayType calculateDayType(DateTime dt, DayOfWeek dayOfWeek, GermanSpecialDays specialDays, out GermanSpecialDay foundSpecialDay) {
      foundSpecialDay = null;
      if (specialDays != null) {
        specialDays.TryGetValue(dt, out foundSpecialDay);
      }
      if (foundSpecialDay != null) {
        return DayType.Holiday;
      }

      DayType ret = DayType.Unknown;
      switch (dayOfWeek) {
        case DayOfWeek.Monday:
        case DayOfWeek.Tuesday:
        case DayOfWeek.Wednesday:
        case DayOfWeek.Thursday:
        case DayOfWeek.Friday:
          ret = DayType.Working;
          break;
        case DayOfWeek.Saturday:
        case DayOfWeek.Sunday:
          ret = DayType.Weekend;
          break;
      }

      // TODO: more calculations required...
      return ret;
    }

    public DayType DayType {
      get { return this.dayType; }
      set { this.dayType = value; }
    }

    public string Name {
      get {
        var now = DateTime.Now;
        if (now.Day == this.day && now.Month == this.month && now.Year == this.year) {
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

    public ISpecialDay SpecialDay { get; set; }

    // HACK
    public string OriginalString {
      get { return this.originalString; }
      set {
        if (this.originalString == value) {
          return;
        }
        this.originalString = value;
        this.isChanged = true;
        if (string.IsNullOrEmpty(this.originalString)) {
          this.items.Clear();
          this.ImportantStuffChanged();
        } else {
          this.ParseData(value);
        }
      }
    }

    private void ParseData(string value) {
      // do parsing
      if (WorkDayParser.Instance != null) {
        WorkDay wd = this;
        this.lastParseResult = WorkDayParser.Instance.Parse(value, ref wd);
        if (!this.LastParseResult.Success) {
          // todo what now?
        }
        this.ImportantStuffChanged();
      }
    }

    public void Reparse() {
      if (!string.IsNullOrWhiteSpace(this.originalString)) {
        this.ParseData(this.OriginalString);
      }
    }

    public double HoursDuration {
      get { return this.Items.Sum(i => i.HoursDuration); }
    }

    public DayOfWeek DayOfWeek { get; set; }
    public IEnumerable<WorkItem> Items {
      get { return this.items; }
    }
    public DateTime DateTime {
      get { return new DateTime(this.year, this.month, this.day); }
    }

    public bool IsToday {
      get {
        var now = DateTime.Now;
        return now.Year == this.year && now.Month == this.month && now.Day == this.day;
      }
    }
    public WorkDayParserResult LastParseResult {
      get { return this.lastParseResult; }
    }

    public string this[string columnName] {
      get {
        if (columnName == "OriginalString") {
          if (LastParseResult != null && !LastParseResult.Success) {
            return LastParseResult.Error;
          }
        }
        return null;
      }
    }

    public string Error { get; private set; }
    public bool IsChanged {
      get { return this.isChanged; }
      set { this.isChanged = value; }
    }

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
      return string.Format("{0},items:{1},origString:{2}", this.DayOfWeek, this.Items.Count(), this.OriginalString);
    }

    public void Clear() {
      this.items.Clear();
    }

    public void AddWorkItem(WorkItem workItem) {
      this.items.Add(workItem);
      workItem.WorkDay = this;
    }

    public void SetData(string originalString) {
      this.OriginalString = originalString;
      this.isChanged = false;
    }
  }

  public enum DayType {
    Unknown,
    Working,
    Weekend,
    Holiday // means feiertag
  }
}