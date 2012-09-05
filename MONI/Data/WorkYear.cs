using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace MONI.Data {
  public class WorkYear {
    private readonly int hitListLookBackInWeeks;
    public int Year { get; set; }

    public WorkYear(int year, IEnumerable<SpecialDate> specialDates, IEnumerable<ShortCut> shortCuts, int hitListLookBackInWeeks) {
      this.hitListLookBackInWeeks = hitListLookBackInWeeks;
      this.Year = year;
      this.Months = new ObservableCollection<WorkMonth>();
      this.Weeks = new ObservableCollection<WorkWeek>();

      var cal = new GregorianCalendar();
      for (int month = 1; month <= cal.GetMonthsInYear(year); month++) {
        WorkMonth wm = new WorkMonth(year, month, specialDates, shortCuts);
        this.Months.Add(wm);
        foreach (var workWeek in wm.Weeks) {
          this.Weeks.Add(workWeek);
        }
      }
    }

    public ObservableCollection<WorkMonth> Months { get; set; }
    public ObservableCollection<WorkWeek> Weeks { get; set; }

    public ObservableCollection<HitlistInfo> ProjectHitlist {
      get {
        var allDays = this.Months.SelectMany(m => m.Days);
        var daysFromLookback = hitListLookBackInWeeks > 0 ? allDays.Where(m => m.DateTime > DateTime.Now.AddDays(hitListLookBackInWeeks * -7)) : allDays;
        var hitlistInfos = daysFromLookback
          .SelectMany(d => d.Items)
          .GroupBy(p => p.Project)
          .OrderByDescending(g => g.Count())
          .Select(g => 
            new HitlistInfo(
              g.Key, 
              g.Count(), 
              g.OrderByDescending(p => p.WorkDay.DateTime).Select(p => p.Description).FirstOrDefault())
              );
        return new ObservableCollection<HitlistInfo>(hitlistInfos);
      }
    }

    public override string ToString() {
      return string.Format("{0}:{1} months", this.Year, this.Months.Count);
    }

    public WorkDay GetDay(int month, int day) {
      return this.Months.ElementAt(month-1).Days.ElementAt(day-1);
    }
  }

  public class HitlistInfo
  {
    private readonly string key;
    public string Key {
      get { return this.key; }
    }
    public int Count {
      get { return this.count; }
    }
    public string LastUsedDescription {
      get { return this.lastUsedDescription; }
    }
    private readonly int count;
    private readonly string lastUsedDescription;

    public HitlistInfo(string key, int count, string lastUsedDescription) {
      this.key = key;
      this.count = count;
      this.lastUsedDescription = lastUsedDescription;
    }
  }
}