using System.Collections.ObjectModel;
using System.Linq;

namespace MonlistClone.Data {
  public class WorkWeek {
    public WorkMonth Month { get; set; }
    private readonly int weekOfYear;

    public WorkWeek(WorkMonth month, int weekOfYear) {
      Month = month;
      this.weekOfYear = weekOfYear;
      Days = new ObservableCollection<WorkDay>();
    }

    public int WeekOfYear {
      get { return this.weekOfYear; }
    }

    public ObservableCollection<WorkDay> Days { get; set; }

    public override string ToString() {
      if (Days.Any()) {
        return string.Format("kw:{0},days:{1},firstday:{2},lastday:{3}", weekOfYear, Days.Count, Days.First().DayOfWeek, Days.Last().DayOfWeek);
      }
      return string.Format("kw:{0},days:{1},empty", this.weekOfYear, this.Days.Count);
    }
  }
}