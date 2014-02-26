using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using MONI.Util;
using Newtonsoft.Json;

namespace MONI.Data
{
  public class ShortCutStatistic : ShortCut, INotifyPropertyChanged
  {
    public ShortCutStatistic(ShortCut sc)
      : base(sc.Key, sc.Expansion) {
      this.ID = sc.ID;
      this.WholeDayExpansion = sc.WholeDayExpansion;
      this.ValidFrom = sc.ValidFrom;
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
        var tmp = this.PropertyChanged;
        if (tmp != null) {
          tmp(this, new PropertyChangedEventArgs("UsedInMonth"));
        }
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
        var tmp = this.PropertyChanged;
        if (tmp != null) {
          tmp(this, new PropertyChangedEventArgs("UsageHistory"));
        }
      }
    }

    #region Implementation of INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
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