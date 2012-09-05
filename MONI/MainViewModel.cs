using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using System.Linq;
using MONI.Data;
using MONI.Util;
using Newtonsoft.Json;

namespace MONI {
  public class MainViewModel : INotifyPropertyChanged {
    private ICommand nextWeekCommand;
    private readonly TextFilePersistenceLayer persistenceLayer;
    private ICommand previousWeekCommand;
    private WorkMonth workMonth;
    private WorkWeek workWeek;
    private WorkYear workYear;
    private readonly CSVExporter csvExporter;

    public MainViewModel() {
      var monlistSettings = ReadSettings();
      var now = DateTime.Now;
      var cal = new GregorianCalendar();
      this.WorkYear = new WorkYear(now.Year, monlistSettings.MainSettings.SpecialDates, monlistSettings.ParserSettings.ShortCuts, monlistSettings.MainSettings.HitListLookBackInWeeks);
      this.WorkMonth = this.WorkYear.Months.ElementAt(now.Month - 1);
      this.WorkWeek = this.WorkMonth.Weeks.First(ww => ww.WeekOfYear == cal.GetWeekOfYear(now, CalendarWeekRule.FirstDay, DayOfWeek.Monday));

      
      this.WeekDayParserSettings = monlistSettings.ParserSettings;
      WorkDayParser.Instance = new WorkDayParser(this.WeekDayParserSettings);

      // read persistencedata
      this.persistenceLayer = new TextFilePersistenceLayer(monlistSettings.MainSettings.DataDirectory);
      this.csvExporter = new CSVExporter(monlistSettings.MainSettings.DataDirectory);
      this.persistenceLayer.ReadData();
      foreach (WorkDayPersistenceData data in this.persistenceLayer.WorkDaysData.Where(wdpd => wdpd.Year == this.WorkYear.Year)) {
        var workDay = this.WorkYear.GetDay(data.Month, data.Day);
        workDay.OriginalString = data.OriginalString;
      }

      foreach (var p in this.WorkYear.ProjectHitlist) {
        Console.WriteLine(p);
      }
    }

    private static MoniSettings ReadSettings() {
      var settingsFile = "settings.json";
      if (File.Exists(settingsFile)) {
        var jsonString = File.ReadAllText(settingsFile);
        return JsonConvert.DeserializeObject<MoniSettings>(jsonString);
      }
      return new MoniSettings();
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
      this.persistenceLayer.SaveData(this.workYear);
      this.csvExporter.Export(this.WorkYear);
    }

    public void CopyFromPreviousDay(WorkDay currentDay)
    {
      var lastValidBefore = this.WorkMonth.Days.Last(x => x.Day < currentDay.Day && x.Items!=null && x.Items.Any());
      if (lastValidBefore != null)
      {
        currentDay.OriginalString = lastValidBefore.OriginalString;
      }
    }
  }
}