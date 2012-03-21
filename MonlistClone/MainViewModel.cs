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
    private string persistenceFileName;
    private TextFilePersistenceLayer persistenceLayer;
    private ICommand previousWeekCommand;
    private WorkMonth workMonth;
    private WorkWeek workWeek;
    private WorkYear workYear;

    public MainViewModel() {
      var now = DateTime.Now;
      var cal = new GregorianCalendar();
      this.WorkYear = new WorkYear(now.Year);
      persistenceFileName = string.Format("data_{0}.txt", this.workYear.Year);
      this.WorkMonth = this.WorkYear.Months.ElementAt(now.Month - 1);
      this.WorkWeek = this.WorkMonth.Weeks.First(ww => ww.WeekOfYear == cal.GetWeekOfYear(now, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
      this.WeekDayParserSettings = new WorkDayParserSettings();
      this.WeekDayParserSettings.ProjectAbbreviations.Add("ctbn", "25482-420(features)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("ctbp", "25482-811(performanceverbesserungen)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("ctbf", "25482-811(tracker)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("ctbm", "25482-140(meeting)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("ctbr", "25482-050(ac-hh-ac)");

      this.WeekDayParserSettings.ProjectAbbreviations.Add("ktln", "25710-420(feature)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("ktlf", "25710-811(tracker)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("ktlm", "25710-140(meeting)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("ktlr", "25710-050(reise)");

      this.WeekDayParserSettings.ProjectAbbreviations.Add("u", "20030-000(urlaub)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("krank", "20020-000(krank/doc)");

      this.WeekDayParserSettings.ProjectAbbreviations.Add("tm", "20018-140(terminalmeeting)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("mm", "20018-140(tess/monatsmeeting)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("swe", "20308-000(swe projekt)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("jmb", "20308-000(jean-marie ausbildungsbetreuung)");
      this.WeekDayParserSettings.ProjectAbbreviations.Add("w", "20180-000(weiterbildung)");
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
      this.WorkMonth = this.workWeek.Month;
    }

    private void SelectNextWeek() {
      this.WorkWeek = this.workYear.Weeks.ElementAt(this.workYear.Weeks.IndexOf(this.workWeek) + 1);
      this.WorkMonth = this.workWeek.Month;
    }

    public void Save() {
      this.persistenceLayer.SaveData(this.workYear, this.persistenceFileName);
      CSVExporter ce = new CSVExporter();
      ce.Export(this.WorkMonth, "monlist.txt");
    }
  }
}