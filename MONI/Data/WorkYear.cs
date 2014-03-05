using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using MONI.Data.SpecialDays;
using MONI.ViewModels;

namespace MONI.Data
{
  public class WorkYear : INotifyPropertyChanged
  {
    private readonly int hitListLookBackInWeeks;
    private readonly PNSearchViewModel pnSearch;
    private readonly PositionSearchViewModel positionSearch;
    public int Year { get; set; }

    public WorkYear(int year, IEnumerable<ShortCut> shortCuts, int hitListLookBackInWeeks, float hoursPerDay, PNSearchViewModel pnSearch, PositionSearchViewModel positionSearch) {
      this.hitListLookBackInWeeks = hitListLookBackInWeeks;
      this.pnSearch = pnSearch;
      this.positionSearch = positionSearch;
      this.Year = year;
      this.Months = new ObservableCollection<WorkMonth>();
      this.Weeks = new ObservableCollection<WorkWeek>();

      var germanSpecialDays = SpecialDaysUtils.GetGermanSpecialDays(year);

      var cal = new GregorianCalendar();
      for (int month = 1; month <= cal.GetMonthsInYear(year); month++) {
        WorkMonth wm = new WorkMonth(year, month, germanSpecialDays, shortCuts, hoursPerDay);
        this.Months.Add(wm);
        foreach (var workWeek in wm.Weeks) {
          this.Weeks.Add(workWeek);
          workWeek.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(this.workWeek_PropertyChanged);
        }
      }
    }

    private void workWeek_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
      this.OnPropertyChanged("HoursDuration");
      this.OnPropertyChanged("ProjectHitList");
      this.OnPropertyChanged("PositionHitlist");
    }

    public ObservableCollection<WorkMonth> Months { get; set; }
    public ObservableCollection<WorkWeek> Weeks { get; set; }

    public ObservableCollection<HitlistInfo> ProjectHitlist {
      get {
        var allDays = this.Months.SelectMany(m => m.Days);
        var daysFromLookback = this.hitListLookBackInWeeks > 0 ? allDays.Where(m => m.DateTime > DateTime.Now.AddDays(this.hitListLookBackInWeeks * -7)) : allDays;
        var hitlistInfos = daysFromLookback
          .SelectMany(d => d.Items)
          .GroupBy(p => p.Project)
          .Select(g =>
                  new HitlistInfo(
                    g.Key,
                    g.Count(),
                    g.Sum(wi => wi.HoursDuration),
                    g.OrderByDescending(p => p.WorkDay.DateTime).Select(p => this.pnSearch.GetDescriptionForProjectNumber(p.Project)).FirstOrDefault())
          );
        return new ObservableCollection<HitlistInfo>(hitlistInfos.OrderByDescending(g => g.HoursUsed));
      }
    }

    public ObservableCollection<HitlistInfo> PositionHitlist {
      get {
        if (positionSearch != null) {
          var allDays = this.Months.SelectMany(m => m.Days);
          var daysFromLookback = this.hitListLookBackInWeeks > 0 ? allDays.Where(m => m.DateTime > DateTime.Now.AddDays(this.hitListLookBackInWeeks*-7)) : allDays;
          var hitlistInfos = daysFromLookback
            .SelectMany(d => d.Items)
            .GroupBy(p => p.Position)
            .Select(g =>
                    new HitlistInfo(
                      g.Key,
                      g.Count(),
                      g.Sum(wi => wi.HoursDuration),
                      positionSearch.GetDescriptionForPositionNumber(g.Key))
            );
          return new ObservableCollection<HitlistInfo>(hitlistInfos.OrderByDescending(g => g.HoursUsed));
        } else {
          return new ObservableCollection<HitlistInfo>();
        }
      }
    }

    public override string ToString() {
      return string.Format("{0}:{1} months", this.Year, this.Months.Count);
    }

    public WorkDay GetDay(int month, int day) {
      return this.Months.ElementAt(month - 1).Days.ElementAt(day - 1);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) {
      PropertyChangedEventHandler handler = this.PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }

  public class HitlistInfo
  {
    public double HoursUsed { get; set; }
    public string Key { get; private set; }
    public int Count { get; private set; }
    public string LastUsedDescription { get; private set; }

    public HitlistInfo(string key, int count, double hoursUsed, string lastUsedDescription) {
      this.HoursUsed = hoursUsed;
      this.Key = key;
      this.Count = count;
      this.LastUsedDescription = lastUsedDescription;
    }
  }
}