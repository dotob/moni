using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using MonlistClone.Data;
using System.Linq;
using MonlistClone.Util;

namespace MonlistClone {
  public class MainViewModel : INotifyPropertyChanged {
    private ICommand nextWeekCommand;
    private string persistenceFileName = "data.txt";
    private TextFilePersistenceLayer persistenceLayer;
    private ICommand previousWeekCommand;
    private WorkMonth workMonth;
    private WorkWeek workWeek;
    private WorkYear workYear;

    public MainViewModel() {
      var now = DateTime.Now;
      var cal = new GregorianCalendar();
      this.WorkYear = new WorkYear(now.Year);
      this.WorkMonth = this.WorkYear.Months.ElementAt(now.Month - 1);
      this.WorkWeek = this.WorkMonth.Weeks.First(ww => ww.WeekOfYear == cal.GetWeekOfYear(now, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
      this.WeekDayParserSettings = new WorkDayParserSettings();
      this.WeekDayParserSettings.ProjectAbbreviations.Add("ktl", "25710-420");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("ctb", "25482-420");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("u", "20030");
      this.WeekDayParserSettings.InsertDayBreak = true;
      this.WeekDayParserSettings.DayBreakTime = new TimeItem(12);
      this.WeekDayParserSettings.DayBreakDurationInMinutes = 30;
      WorkDayParser.Instance = new WorkDayParser(this.WeekDayParserSettings);

      // read persistencedata
      this.persistenceLayer = new TextFilePersistenceLayer();
      this.persistenceLayer.ReadData(this.persistenceFileName);
      foreach (WorkDayPersistenceData data in this.persistenceLayer.WorkDaysData.Where(wdpd => wdpd.Year == this.WorkYear.Year)) {
        var workDay = this.WorkYear.GetDay(data.Month, data.Day);
        workDay.OriginalString = data.OriginalString;
      }
    }

    public ICommand PreviousWeekCommand {
      get { return this.previousWeekCommand ?? (this.previousWeekCommand = new DelegateCommand(this.SelectPreviousWeek, this.IsThereAPreviousWeek)); }
    }

    public ICommand NextWeekCommand {
      get { return this.nextWeekCommand ?? (this.nextWeekCommand = new DelegateCommand(this.SelectNextWeek, this.IsThereANextWeek)); }
    }

    public WorkDayParserSettings WeekDayParserSettings { get; set; }

    public WorkWeek WorkWeek {
      get { return this.workWeek; }
      set {
        this.workWeek = value;
        NotifyPropertyChangedHelper.OnPropertyChanged(this, this.PropertyChanged, () => this.WorkWeek);
      }
    }

    public WorkMonth WorkMonth {
      get { return this.workMonth; }
      set {
        this.workMonth = value;
        NotifyPropertyChangedHelper.OnPropertyChanged(this, this.PropertyChanged, () => this.WorkMonth);
      }
    }

    public WorkYear WorkYear {
      get { return this.workYear; }
      set {
        this.workYear = value;
        NotifyPropertyChangedHelper.OnPropertyChanged(this, this.PropertyChanged, () => this.WorkYear);
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    private bool IsThereAPreviousWeek() {
      return this.workYear.Weeks.IndexOf(this.workWeek) > 0;
    }

    private bool IsThereANextWeek() {
      return this.workYear.Weeks.Count - 1 > this.workYear.Weeks.IndexOf(this.workWeek);
    }

    private void SelectPreviousWeek() {
      this.WorkWeek = this.workYear.Weeks.ElementAt(this.workYear.Weeks.IndexOf(this.workWeek) - 1);
    }

    private void SelectNextWeek() {
      this.WorkWeek = this.workYear.Weeks.ElementAt(this.workYear.Weeks.IndexOf(this.workWeek) + 1);
    }

    public void Save() {
      this.persistenceLayer.SaveData(this.workYear, this.persistenceFileName);
      CSVExporter ce = new CSVExporter();
      ce.Export(this.WorkMonth, "monlist.txt");
    }
  }
}