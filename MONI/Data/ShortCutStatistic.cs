using System.Collections.ObjectModel;
using System.Linq;
using MONI.Util;
using Newtonsoft.Json;
using NLog;

namespace MONI.Data
{
  public class ShortCutStatistic : ShortCut
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public ShortCutStatistic(ShortCut sc)
      : base(sc.Key, sc.Expansion) {
      this.ID = sc.ID;
      this.Index = sc.Index;
      this.WholeDayExpansion = sc.WholeDayExpansion;
      this.ValidFrom = sc.ValidFrom;
      this.Group = sc.Group;
    }

    private double usedInMonth;
    [JsonIgnore]
    public double UsedInMonth {
      get { return this.usedInMonth; }
      set {
        if (this.usedInMonth == value) {
          return;
        }
        this.usedInMonth = value;
        this.OnPropertyChanged(() => this.UsedInMonth);
      }
    }

    private ObservableCollection<UsageInfo> usageHistory;
    [JsonIgnore]
    public ObservableCollection<UsageInfo> UsageHistory
    {
      get { return this.usageHistory; }
      set {
        if (this.usageHistory == value) {
          return;
        }
        this.usageHistory = value;
        this.OnPropertyChanged(() => this.UsageHistory);
      }
    }

    public void Calculate(ObservableCollection<WorkDay> days) {
      // complete hours over all days
      this.UsedInMonth = days.SelectMany(d => d.Items).Where(i => i.ShortCut != null && Equals(this, i.ShortCut)).Sum(i => i.HoursDuration);

      // generate complete usage information over all days
      var usageInfos =
        (from workDay in days
         let hours = workDay.Items.Where(i => i.ShortCut != null && Equals(this, i.ShortCut)).Sum(i => i.HoursDuration)
         select new UsageInfo { Day = workDay.Day, Hours = hours, IsToday = workDay.IsToday }).ToList();

      if (this.UsageHistory == null)
      {
        logger.Debug("CalcShortCutStatistic => {0} Initial calculated shortcut statistics ({1}, {2})", usageInfos.Count(), this.Key, usageInfos.Sum(ui => ui.Hours));
        this.UsageHistory = new QuickFillObservableCollection<UsageInfo>(usageInfos);
      } else {
        foreach (var ui in this.UsageHistory)
        {
          var calculatedUI = usageInfos.ElementAtOrDefault(ui.Day - 1);
          if (calculatedUI != null) {
            ui.Hours = calculatedUI.Hours;
            ui.IsToday = calculatedUI.IsToday;
          } else {
            logger.Error("CalcShortCutStatistic => No usage info found for day {0}, shortcut {1}!", ui.Day, this.Key);
          }
        }
      }
    }
  }

  public class UsageInfo : ViewModelBase
  {
    public int Day { get; set; }

    private double hours;

    public double Hours
    {
      get { return this.hours; }
      set
      {
        if (!Equals(value, this.Hours)) {
          this.hours = value;
          this.OnPropertyChanged(() => this.Hours);
        }
      }
    }

    private bool isToday;

    public bool IsToday
    {
      get { return this.isToday; }
      set
      {
        if (!Equals(value, this.IsToday)) {
          this.isToday = value;
          this.OnPropertyChanged(() => this.IsToday);
        }
      }
    }
  }
}