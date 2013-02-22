using System.Collections.ObjectModel;
using System.ComponentModel;

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
    public ObservableCollection<UsageInfo> UsageHistory {
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

  public class UsageInfo
  {
    public double Hours { get; set; }
    public bool IsToday { get; set; }
  }
}