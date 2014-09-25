using System.Collections.ObjectModel;
using MONI.Util;
using Newtonsoft.Json;

namespace MONI.Data
{
  public class ShortCutStatistic : ShortCut
  {
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