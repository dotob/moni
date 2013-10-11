using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop;
using MONI.Data;
using MONI.Util;
using NLog;
using Newtonsoft.Json;

namespace MONI.ViewModels
{
  public class MainViewModel : ViewModelBase, IDropTarget
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private ICommand nextWeekCommand;
    private readonly TextFilePersistenceLayer persistenceLayer;
    private ICommand previousWeekCommand;
    private WorkMonth workMonth;
    private WorkWeek workWeek;
    private WorkYear workYear;
    private readonly CSVExporter csvExporter;
    private MoniSettings monlistSettings;
    private ShortcutViewModel editShortCut;

    private string settingsFile;
    Calendar calendar = new GregorianCalendar();
    private MoniSettings editPreferences;
    private Visibility projectListVisibility;
    private bool loadingData;
    private CustomWindowPlacementSettings customWindowPlacementSettings;

    private DispatcherTimer throttleSaveAndCalc;
    private ReadWriteResult persistentResult;
    private bool showPasswordDialog;
    private ICommand nextMonthCommand;
    private ICommand previousMonthCommand;

    public MainViewModel(Dispatcher dispatcher) {
      // handle settings
      this.settingsFile = this.DetermineSettingsFile();
      this.MonlistSettings = ReadSettings(this.settingsFile);

      this.ProjectListVisibility = this.MonlistSettings.MainSettings.ShowProjectHitList ? Visibility.Visible : Visibility.Collapsed;
      this.Settings = this.MonlistSettings;
      this.CustomWindowPlacementSettings = new CustomWindowPlacementSettings(this.Settings);
      WorkDayParser.Instance = new WorkDayParser(this.Settings.ParserSettings);

      // pnsearch
      this.PNSearch = new PNSearchViewModel(this.Settings.MainSettings.ProjectNumberFilePath);

      // updateinfo
      var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
      this.UpdateInfoViewModel = new UpdateInfoViewModel(this.Settings.MainSettings.UpdateInfoURL, currentVersion);

      // read persistencedata
      var dataDirectory = this.MonlistSettings.MainSettings.DataDirectory.Replace("#{appdata}", Utils.MoniAppDataPath());
      this.persistenceLayer = new TextFilePersistenceLayer(dataDirectory);
      this.csvExporter = new CSVExporter(dataDirectory);
      //this.persistentResult = this.persistenceLayer.ReadData();
      this.SelectToday(); // sets data from persistencelayer
      if (dispatcher != null) {
        this.throttleSaveAndCalc = new DispatcherTimer(DispatcherPriority.DataBind, dispatcher);
        this.throttleSaveAndCalc.Tick += new EventHandler(this.throttleSaveAndCalc_Tick);
      }
    }

    public UpdateInfoViewModel UpdateInfoViewModel { get; set; }

    private string DetermineSettingsFile() {
      logger.Debug("determine settingsfile location");
      // check if there is a settings file in userdir
      var fileName = "settings.json";
      var moniAppData = Utils.MoniAppDataPath();
      var moniAppDataSettingsFile = Path.Combine(moniAppData, fileName);
      if (File.Exists(moniAppDataSettingsFile)) {
        logger.Debug("found settingsfile in appdata: {0}", moniAppDataSettingsFile);
        return moniAppDataSettingsFile;
      }
      // check if we can create settings file in exe dir
      if (Utils.CanCreateFile(".")) {
        logger.Debug("could write in currentdir: {0} use {1} as settingsfile",Directory.GetCurrentDirectory(), fileName);
        return fileName;
      }
      logger.Debug("create new settingsfile in appdata: {0}", moniAppDataSettingsFile);
      return moniAppDataSettingsFile;
    }

    private void throttleSaveAndCalc_Tick(object sender, EventArgs e) {
      this.SaveAndCalc();
      this.throttleSaveAndCalc.Stop();
    }

    private static MoniSettings ReadSettings(string settingsFile) {
      var settings = ReadSettingsInternal(settingsFile);
      PatchSettings(settings);
      return settings;
    }

    private static void PatchSettings(MoniSettings settings) {
      if (string.IsNullOrWhiteSpace(settings.MainSettings.UpdateInfoURL)) {
        settings.MainSettings.UpdateInfoURL = MoniSettings.GetEmptySettings().MainSettings.UpdateInfoURL;
      }
    }

    private static MoniSettings ReadSettingsInternal(string settingsFile) {
      if (File.Exists(settingsFile)) {
        var jsonString = File.ReadAllText(settingsFile);
        return JsonConvert.DeserializeObject<MoniSettings>(jsonString);
      }
      // no settingsfile found, try to read sample settings
      var settingsJsonSkeleton = "settings.json.skeleton";
      if (File.Exists(settingsJsonSkeleton)) {
        var jsonString = File.ReadAllText(settingsJsonSkeleton);
        return JsonConvert.DeserializeObject<MoniSettings>(jsonString);
      }

      // no samplesettings, use default
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

    public ICommand PreviousMonthCommand {
      get { return this.previousMonthCommand ?? (this.previousMonthCommand = new DelegateCommand(this.SelectPreviousMonth, () => true)); }
    }
    public ICommand NextMonthCommand {
      get { return this.nextMonthCommand ?? (this.nextMonthCommand = new DelegateCommand(this.SelectNextMonth, () => true)); }
    }

    public MoniSettings Settings { get; set; }

    public WorkWeek WorkWeek {
      get { return this.workWeek; }
      set {
        if (this.workWeek == value) {
          return;
        }
        bool monthChanged = value == null || this.workWeek==null || value.Month != this.workWeek.Month;
        this.workWeek = value;
        this.OnPropertyChanged(() => this.WorkWeek);

        // don't know if this perfect, but it works
        if (monthChanged && value != null) {
          value.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(value.StartDate));
        }
      }
    }

    public WorkMonth WorkMonth {
      get { return this.workMonth; }
      set {
        this.workMonth = value;
        this.CurrentMonthMonlistImportFile = this.csvExporter.FilenameForMonth(value);
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

    public ShortcutViewModel EditShortCut {
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
    public ReadWriteResult PersistentResult {
      get { return this.persistentResult; }
      set { this.persistentResult = value; }
    }

    public string CurrentMonthMonlistImportFile { get; private set; }

    public bool ShowPasswordDialog {
      get { return this.showPasswordDialog; }
      set {
        this.showPasswordDialog = value;
        this.OnPropertyChanged(() => this.ShowPasswordDialog);
      }
    }

    public PNSearchViewModel PNSearch { get; set; }

    private void SelectPreviousWeek() {
      var firstDayOfWeek = this.workWeek.StartDate;
      var dateTime = firstDayOfWeek.AddDays(-1);
      this.SelectDate(dateTime);
    }

    private void SelectNextWeek() {
      var firstDayOfWeek = this.workWeek.StartDate;
      var dateTime = firstDayOfWeek.AddDays(7);
      if (firstDayOfWeek.Month == dateTime.Month) {
        this.SelectDate(dateTime);
      } else {
        var nuDate = firstDayOfWeek.AddMonths(1).AddDays(-1 * firstDayOfWeek.Day+1);
        this.SelectDate(nuDate);
      }
    }


    private void SelectPreviousMonth() {
      var firstDayOfWeek = this.workWeek.StartDate;
      this.SelectDate(firstDayOfWeek.AddMonths(-1));
    }

    private void SelectNextMonth() {
      var firstDayOfWeek = this.workWeek.StartDate;
      this.SelectDate(firstDayOfWeek.AddMonths(1));
    }

    public void SelectToday() {
      this.SelectDate(DateTime.Now);
    }

    public void SelectDate(DateTime date) {
      if (this.workYear == null || date.Year != this.workYear.Year) {
        this.CreateAndLoadYear(date.Year);
      }
      if (this.workMonth == null || date.Month != this.workMonth.Month) {
        this.WorkMonth = this.WorkYear.Months.ElementAt(date.Month - 1);
        this.WorkMonth.CalcPreviewHours();
      }
      this.WorkWeek = this.WorkMonth.Weeks.First(ww => ww.WeekOfYear == this.calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
    }

    private void CreateAndLoadYear(int year) {
      if (this.WorkYear != null) {
        // need to save years data
        this.Save();
      }
      this.WorkYear = new WorkYear(year, this.MonlistSettings.ParserSettings.ShortCuts, this.MonlistSettings.MainSettings.HitListLookBackInWeeks, this.MonlistSettings.MainSettings.HoursPerDay);
      this.loadingData = true;
      this.PersistentResult = this.persistenceLayer.SetDataOfYear(this.WorkYear);
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
      this.WorkWeek.Reparse();
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