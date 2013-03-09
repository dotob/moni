using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop;
using MONI.Data;
using MONI.Util;
using Newtonsoft.Json;

namespace MONI.ViewModels
{
  public class MainViewModel : ViewModelBase, IDropTarget
  {
    private ICommand nextWeekCommand;
    private readonly TextFilePersistenceLayer persistenceLayer;
    private ICommand previousWeekCommand;
    private WorkMonth workMonth;
    private WorkWeek workWeek;
    private WorkYear workYear;
    private readonly CSVExporter csvExporter;
    private MoniSettings monlistSettings;
    private ShortCut editShortCut;

    private readonly string settingsFile = "settings.json";
    Calendar calendar = new GregorianCalendar();
    private MoniSettings editPreferences;
    private Visibility projectListVisibility;
    private bool loadingData;
    private CustomWindowPlacementSettings customWindowPlacementSettings;

    private DispatcherTimer throttleSaveAndCalc;

    public MainViewModel(Dispatcher dispatcher) {
      this.MonlistSettings = ReadSettings(this.settingsFile);
      this.ProjectListVisibility = this.MonlistSettings.MainSettings.ShowProjectHitList ? Visibility.Visible : Visibility.Collapsed;
      this.Settings = this.MonlistSettings;
      this.CustomWindowPlacementSettings = new CustomWindowPlacementSettings(this.Settings);
      WorkDayParser.Instance = new WorkDayParser(this.Settings.ParserSettings);

      // read persistencedata
      this.persistenceLayer = new TextFilePersistenceLayer(this.MonlistSettings.MainSettings.DataDirectory);
      this.csvExporter = new CSVExporter(this.MonlistSettings.MainSettings.DataDirectory);
      this.persistenceLayer.ReadData();
      this.SelectToday(); // sets data from persistencelayer
      if (dispatcher != null) {
        this.throttleSaveAndCalc = new DispatcherTimer(DispatcherPriority.DataBind, dispatcher);
        this.throttleSaveAndCalc.Tick += new EventHandler(this.throttleSaveAndCalc_Tick);
      }
    }

    private void throttleSaveAndCalc_Tick(object sender, EventArgs e) {
      this.SaveAndCalc();
      this.throttleSaveAndCalc.Stop();
    }

    private static MoniSettings ReadSettings(string settingsFile) {
      if (File.Exists(settingsFile)) {
        var jsonString = File.ReadAllText(settingsFile);
        return JsonConvert.DeserializeObject<MoniSettings>(jsonString);
      }
      return MoniSettings.GetEmptySettings();
    }

    private static void WriteSettings(MoniSettings settings, string settingsFile) {
      var settingsAsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
      File.WriteAllText(settingsFile, settingsAsJson);
    }

    public CustomWindowPlacementSettings CustomWindowPlacementSettings {
      get { return this.customWindowPlacementSettings; }
      set {
        if (Equals(value, this.customWindowPlacementSettings)) {
          return;
        }
        this.customWindowPlacementSettings = value;
        this.OnPropertyChanged(() => this.CustomWindowPlacementSettings);
      }
    }

    public ICommand PreviousWeekCommand {
      get { return this.previousWeekCommand ?? (this.previousWeekCommand = new DelegateCommand(this.SelectPreviousWeek, () => true)); }
    }

    public ICommand NextWeekCommand {
      get { return this.nextWeekCommand ?? (this.nextWeekCommand = new DelegateCommand(this.SelectNextWeek, () => true)); }
    }

    public MoniSettings Settings { get; set; }

    public WorkWeek WorkWeek {
      get { return this.workWeek; }
      set {
        this.workWeek = value;
        this.OnPropertyChanged(() => this.WorkWeek);

        // don't know if this perfect, but it works
        if (value != null) {
          value.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(value.StartDate));
        }
      }
    }

    public WorkMonth WorkMonth {
      get { return this.workMonth; }
      set {
        this.workMonth = value;
        this.OnPropertyChanged(() => this.WorkMonth);
      }
    }

    public WorkYear WorkYear {
      get { return this.workYear; }
      set {
        if (this.workYear != null) {
          this.workYear.PropertyChanged -= new PropertyChangedEventHandler(this.workYear_PropertyChanged);
        }
        this.workYear = value;
        if (this.workYear != null) {
          this.workYear.PropertyChanged += new PropertyChangedEventHandler(this.workYear_PropertyChanged);
        }
        this.OnPropertyChanged(() => this.WorkYear);
      }
    }

    void workYear_PropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == "HoursDuration") {
        if (!this.loadingData) {
          // try next save in 500ms
          this.throttleSaveAndCalc.Stop();
          this.throttleSaveAndCalc.Interval = TimeSpan.FromMilliseconds(500);
          this.throttleSaveAndCalc.Start();
        }
      }
    }

    private void SaveAndCalc() {
      this.Save();
      this.WorkMonth.CalcShortCutStatistic();
    }

    public ShortCut EditShortCut {
      get { return this.editShortCut; }
      set {
        this.editShortCut = value;
        this.OnPropertyChanged(() => this.EditShortCut);
      }
    }

    public MoniSettings EditPreferences {
      get { return this.editPreferences; }
      set {
        this.editPreferences = value;
        this.OnPropertyChanged(() => this.EditPreferences);
      }
    }

    public Visibility ProjectListVisibility {
      get { return this.projectListVisibility; }
      private set {
        this.projectListVisibility = value;
        this.OnPropertyChanged(() => this.ProjectListVisibility);
      }
    }

    private MoniSettings MonlistSettings {
      get { return this.monlistSettings; }
      set { 
        this.monlistSettings = value;
        MoniSettings.Current = this.MonlistSettings;
      }
    }

    private void SelectPreviousWeek() {
      var look4PrevWeek = this.workYear.Weeks.ElementAtOrDefault(this.workYear.Weeks.IndexOf(this.workWeek) - 1);
      if (look4PrevWeek != null) {
        this.WorkWeek = look4PrevWeek;
      } else {
        // load previous year
        var lastWeekDay = this.WorkWeek.Days.First();
        var lastDayOfPreviousYear = new DateTime(lastWeekDay.Year - 1, 12, 31, this.calendar);
        this.SelectDate(lastDayOfPreviousYear);
      }
      this.WorkMonth = this.workWeek.Month;
    }

    private void SelectNextWeek() {
      var look4NextWeek = this.workYear.Weeks.ElementAtOrDefault(this.workYear.Weeks.IndexOf(this.workWeek) + 1);
      if (look4NextWeek != null) {
        this.WorkWeek = look4NextWeek;
        this.WorkMonth = this.workWeek.Month;
      } else {
        // load next year
        var lastWeekDay = this.WorkWeek.Days.Last();
        var firstDayOfNextYear = new DateTime(lastWeekDay.Year + 1, 1, 1, this.calendar);
        this.SelectDate(firstDayOfNextYear);
      }
    }

    public void SelectToday() {
      this.SelectDate(DateTime.Now);
    }

    public void SelectDate(DateTime date) {
      if (this.workYear == null || date.Year != this.workYear.Year) {
        this.CreateAndLoadYear(date.Year);
      }
      this.WorkMonth = this.WorkYear.Months.ElementAt(date.Month - 1);
      this.WorkWeek = this.WorkMonth.Weeks.First(ww => ww.WeekOfYear == this.calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
    }

    private void CreateAndLoadYear(int year) {
      if (this.WorkYear != null) {
        // need to save years data
        this.Save();
      }
      this.WorkYear = new WorkYear(year, this.MonlistSettings.ParserSettings.ShortCuts, this.MonlistSettings.MainSettings.HitListLookBackInWeeks, this.MonlistSettings.MainSettings.HoursPerDay);
      this.loadingData = true;
      this.persistenceLayer.SetDataOfYear(this.WorkYear);
      this.loadingData = false;
    }

    public void Save() {
      // save data
      this.persistenceLayer.SaveData(this.workYear);
      this.csvExporter.Export(this.WorkYear);
      // save settings
      WriteSettings(this.MonlistSettings, this.settingsFile);
    }

    public void CopyFromPreviousDay(WorkDay currentDay) {
      var lastValidBefore = this.WorkMonth.Days.LastOrDefault(x => x.Day < currentDay.Day && x.Items != null && x.Items.Any());
      if (lastValidBefore != null) {
        currentDay.OriginalString = lastValidBefore.OriginalString;
      }
    }

    public void DeleteShortcut(ShortCut delsc) {
      this.MonlistSettings.ParserSettings.ShortCuts.Remove(delsc);
      this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
      this.WorkWeek.Reparse();
    }

    public void SaveEditShortcut() {
      var shortCut = this.MonlistSettings.ParserSettings.ShortCuts.FirstOrDefault(sc => Equals(sc, this.EditShortCut));
      if (shortCut != null) {
        shortCut.GetData(this.EditShortCut);
      } else {
        this.MonlistSettings.ParserSettings.ShortCuts.Add(this.EditShortCut);
      }
      this.EditShortCut = null;
      this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
      this.WorkWeek.Reparse();
    }

    public void MoveShortcutUp(ShortCut sc) {
      var idx = this.MonlistSettings.ParserSettings.ShortCuts.IndexOf(sc);
      if (idx > 0) {
        this.MonlistSettings.ParserSettings.ShortCuts.Remove(sc);
        this.MonlistSettings.ParserSettings.ShortCuts.Insert(idx - 1, sc);
      }
      this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
    }

    public void MoveShortcutDown(ShortCut sc) {
      var idx = this.MonlistSettings.ParserSettings.ShortCuts.IndexOf(sc);
      if (idx < this.MonlistSettings.ParserSettings.ShortCuts.Count - 1) {
        this.MonlistSettings.ParserSettings.ShortCuts.Remove(sc);
        this.MonlistSettings.ParserSettings.ShortCuts.Insert(idx + 1, sc);
      }
      this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
    }

    public void StartEditingPreferences() {
      this.EditPreferences = this.MonlistSettings;
    }

    public void SaveEditingPreferences() {
      this.ProjectListVisibility = this.MonlistSettings.MainSettings.ShowProjectHitList ? Visibility.Visible : Visibility.Collapsed;
      this.EditPreferences = null;
    }

    public void CancelEditingPreferences() {
      this.EditPreferences = null;
      this.MonlistSettings = ReadSettings(this.settingsFile);
    }

    public void DragOver(IDropInfo dropInfo) {
      dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
      dropInfo.Effects = DragDropEffects.Move;
    }

    public void Drop(IDropInfo dropInfo) {
      try {
        GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
        this.MonlistSettings.ParserSettings.ShortCuts.Clear();
        this.MonlistSettings.ParserSettings.ShortCuts.AddRange(dropInfo.TargetCollection.OfType<KeyValuePair<string, ShortCutStatistic>>().Select(kvp => kvp.Value));
        this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
      } catch (Exception exception) {
        Console.WriteLine(exception);
      }
    }

    public void AddCurrentTime(WorkDay currentDay) {
      currentDay.OriginalString = WorkDayParser.Instance.AddCurrentTime(currentDay.OriginalString);
    }
  }
}