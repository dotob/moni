using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop;
using MONI.Data;
using MONI.Util;
using NLog;
using Newtonsoft.Json;
using Calendar = System.Globalization.Calendar;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace MONI.ViewModels {
  public class MainViewModel : ViewModelBase, IDropTarget {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private readonly Calendar calendar = new GregorianCalendar();

    private readonly CSVExporter csvExporter;
    private readonly TextFilePersistenceLayer persistenceLayer;

    private readonly string settingsFile;
    private readonly DispatcherTimer throttleSaveAndCalc;
    private CustomWindowPlacementSettings customWindowPlacementSettings;
    private MoniSettings editPreferences;
    private ShortcutViewModel editShortCut;
    private bool loadingData;
    private MoniSettings monlistSettings;
    private ICommand nextMonthCommand;
    private ICommand nextWeekCommand;
    private Visibility positionHitListVisibility;
    private ICommand previousMonthCommand;
    private ICommand previousWeekCommand;
    private Visibility projectHitListVisibility;
    private bool showPasswordDialog;
    private WorkMonth workMonth;
    private WorkWeek workWeek;
    private WorkYear workYear;

    public MainViewModel(Dispatcher dispatcher) {
      // handle settings
      this.settingsFile = this.DetermineSettingsFile();
      this.MonlistSettings = ReadSettings(this.settingsFile);

      this.UpdateVisibility();

      this.Settings = this.MonlistSettings;
      this.CustomWindowPlacementSettings = new CustomWindowPlacementSettings(this.Settings);
      WorkDayParser.Instance = new WorkDayParser(this.Settings.ParserSettings);

      // pnsearch
      this.PNSearch = new PNSearchViewModel(this.Settings.MainSettings.ProjectNumberFilePath, this.Settings.MainSettings.MonlistGBNumber);
      
      // posnumsearch
      this.PositionSearch = new PositionSearchViewModel(this.Settings.MainSettings.PositionNumberFilePath);

      // read persistencedata
      string dataDirectory = Utils.PatchFilePath(this.MonlistSettings.MainSettings.DataDirectory);
      this.persistenceLayer = new TextFilePersistenceLayer(dataDirectory);
      this.csvExporter = new CSVExporter(dataDirectory);
      //this.persistentResult = this.persistenceLayer.ReadData();
      this.SelectToday(); // sets data from persistencelayer
      if (dispatcher != null) {
        this.throttleSaveAndCalc = new DispatcherTimer(DispatcherPriority.DataBind, dispatcher);
        this.throttleSaveAndCalc.Tick += this.throttleSaveAndCalc_Tick;
      }

      // updateinfo
      Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
      this.UpdateInfoViewModel = new UpdateInfoViewModel(this.Settings.MainSettings.UpdateInfoURL, currentVersion, this.persistenceLayer.EntryCount, this.persistenceLayer.FirstEntryDate);


      // load help
      this.Help = this.ReadHelp();
         
      this.SelectWorkItemTextComplete += DoSelectWorkItemTextComplete;
      this.SelectWorkItemTextWithOutTime += DoSelectWorkItemTextWithOutTime;
      this.GoToDay = DoGoToDay;
    }


    private string ReadHelp() {
      string completeHelp = Encoding.UTF8.GetString(Properties.Resources.README);
      var asLines = completeHelp.Split(new string[]{Environment.NewLine}, StringSplitOptions.None);
      var sb = new StringBuilder();
      var gotit = false;
      foreach (var asLine in asLines) {
        if (gotit) {
          sb.AppendLine(asLine);
        } else {
          gotit = asLine.Contains("shorthelp:");
        }
      }
      return sb.ToString();
    }

    private string help;

    public string Help
    {
      get { return this.help; }
      set
      {
        if (Equals(value, this.help)) {
          return;
        }
        this.help = value;
        this.OnPropertyChanged(() => this.Help);
      }
    }

    private bool showHelp;
    private Visibility monthListVisibility;

    public bool ShowHelp
    {
      get { return this.showHelp; }
      set
      {
        if (Equals(value, this.showHelp)) {
          return;
        }
        this.showHelp = value;
        this.OnPropertyChanged(() => this.ShowHelp);
      }
    }

    public PositionSearchViewModel PositionSearch { get; set; }
    public PNSearchViewModel PNSearch { get; set; }

    public Action<object> SelectWorkItemTextComplete { get; private set; }
    public Action<object> SelectWorkItemTextWithOutTime { get; private set; }
    public Action<object> GoToDay { get; private set; }

    public UpdateInfoViewModel UpdateInfoViewModel { get; set; }

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
        bool monthChanged = value == null || this.workWeek == null || value.Month != this.workWeek.Month;
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
          this.workYear.PropertyChanged -= this.workYear_PropertyChanged;
        }
        this.workYear = value;
        if (this.workYear != null) {
          this.workYear.PropertyChanged += this.workYear_PropertyChanged;
        }
        this.OnPropertyChanged(() => this.WorkYear);
      }
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

    public Visibility ProjectHitListVisibility {
      get { return this.projectHitListVisibility; }
      private set {
        this.projectHitListVisibility = value;
        this.OnPropertyChanged(() => this.ProjectHitListVisibility);
      }
    }

    public Visibility PositionHitListVisibility {
      get { return this.positionHitListVisibility; }
      set {
        this.positionHitListVisibility = value;
        this.OnPropertyChanged(() => this.PositionHitListVisibility);
      }
    }

    public Visibility MonthListVisibility {
      get { return this.monthListVisibility; }
      set {
        this.monthListVisibility = value;
        this.OnPropertyChanged(() => this.MonthListVisibility);
      }
    }


    private MoniSettings MonlistSettings {
      get { return this.monlistSettings; }
      set {
        this.monlistSettings = value;
        MoniSettings.Current = this.MonlistSettings;
      }
    }

    public ReadWriteResult PersistentResult { get; set; }

    public string CurrentMonthMonlistImportFile { get; private set; }

    public bool ShowPasswordDialog {
      get { return this.showPasswordDialog; }
      set {
        this.showPasswordDialog = value;
        this.OnPropertyChanged(() => this.ShowPasswordDialog);
      }
    }


    public void DragOver(IDropInfo dropInfo) {
      dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
      dropInfo.Effects = DragDropEffects.Move;
    }

    public void Drop(IDropInfo dropInfo) {
      try {
        var sourceIndex = dropInfo.DragInfo.SourceIndex;
        var targetIndex = dropInfo.InsertIndex;
        
        var target = (KeyValuePair<string, ShortCutStatistic>)dropInfo.TargetItem;
        var source = (KeyValuePair<string, ShortCutStatistic>)dropInfo.DragInfo.Data;
        //source.Value.Group = target.Value.Group;

        //DragDrop.DefaultDropHandler.Drop(dropInfo);
        //this.MonlistSettings.ParserSettings.ShortCuts.Clear();
        //this.MonlistSettings.ParserSettings.ShortCuts.AddRange(
          //dropInfo.TargetCollection.OfType<KeyValuePair<string, ShortCutStatistic>>().Select(kvp => kvp.Value));
        //this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
      } catch (Exception exception) {
        Console.WriteLine(exception);
      }
    }

    private static void DoSelectWorkItemTextComplete(object o) {
      WorkItem workItem;
      TextBox tb;
      string wholeString;
      if (!GetTextBoxWorkItemInfo(o, out workItem, out tb, out wholeString)){ return;}

      if (!string.IsNullOrWhiteSpace(wholeString)) {
        string searchString = workItem.OriginalString;
        int selStart = wholeString.IndexOf(searchString, StringComparison.InvariantCulture);
        if (selStart >= 0) {
          tb.Select(selStart, searchString.Length);
          Clipboard.SetText(searchString);
        }
      }
    }

    private void DoSelectWorkItemTextWithOutTime(object o) {
      WorkItem workItem;
      TextBox tb;
      string wholeString;
      if (!GetTextBoxWorkItemInfo(o, out workItem, out tb, out wholeString)) { return; }

      if (!string.IsNullOrWhiteSpace(wholeString)) {
        string searchString = workItem.OriginalString.Token(";", 2);
        int selStart = wholeString.IndexOf(searchString, StringComparison.InvariantCulture);
        if (selStart >= 0) {
          tb.Select(selStart, searchString.Length);
          Clipboard.SetText(searchString);
        }
      }
    }

    private void DoGoToDay(object o) {
      var wd = o as WorkDay;
      if (wd != null) {
        this.SelectDate(wd.DateTime);
      }
    }

    private static bool GetTextBoxWorkItemInfo(object o, out WorkItem workItem, out TextBox tb, out string wholeString) {
      workItem = null;
      tb = null;
      wholeString = null;
      var bindedValues = o as IEnumerable;
      if (bindedValues == null) {
        return false;
      }
      workItem = bindedValues.OfType<object>().ElementAtOrDefault(0) as WorkItem;
      tb = bindedValues.OfType<object>().ElementAtOrDefault(1) as TextBox;
      if (workItem == null || tb == null) {
        return false;
      }
      tb.Focus();
      wholeString = tb.Text;
      return true;
    }

    private string DetermineSettingsFile() {
      logger.Debug("determine settingsfile location");
      // check if there is a settings file in userdir
      string fileName = "settings.json";
      string moniAppData = Utils.MoniAppDataPath();
      string moniAppDataSettingsFile = Path.Combine(moniAppData, fileName);
      if (File.Exists(moniAppDataSettingsFile)) {
        logger.Debug("found settingsfile in appdata: {0}", moniAppDataSettingsFile);
        return moniAppDataSettingsFile;
      }
      // check if we can create settings file in exe dir
      if (Utils.CanCreateFile(".")) {
        logger.Debug("could write in currentdir: {0} use {1} as settingsfile", Directory.GetCurrentDirectory(), fileName);
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
      MoniSettings settings = ReadSettingsInternal(settingsFile);
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
        string jsonString = File.ReadAllText(settingsFile);
        return JsonConvert.DeserializeObject<MoniSettings>(jsonString);
      }
      // no settingsfile found, try to read sample settings
      string settingsJsonSkeleton = "settings.json.skeleton";
      if (File.Exists(settingsJsonSkeleton)) {
        string jsonString = File.ReadAllText(settingsJsonSkeleton);
        return JsonConvert.DeserializeObject<MoniSettings>(jsonString);
      }

      // no samplesettings, use default
      return MoniSettings.GetEmptySettings();
    }

    private static void WriteSettings(MoniSettings settings, string settingsFile) {
      string settingsAsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
      File.WriteAllText(settingsFile, settingsAsJson);
    }

    private void workYear_PropertyChanged(object sender, PropertyChangedEventArgs e) {
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

    private void SelectPreviousWeek() {
      DateTime firstDayOfWeek = this.workWeek.StartDate;
      DateTime dateTime = firstDayOfWeek.AddDays(-1);
      this.SelectDate(dateTime);
    }

    private void SelectNextWeek() {
      var firstDayOfWeek = this.workWeek.StartDate;
      var dateTime = firstDayOfWeek.AddDays(7);
      if (firstDayOfWeek.Month == dateTime.Month) {
        this.SelectDate(dateTime);
      } else {
        var nextMonth = firstDayOfWeek.AddMonths(1);
        var nuDate = nextMonth.AddDays(-1 * nextMonth.Day + 1);
        this.SelectDate(nuDate);
      }
    }


    private void SelectPreviousMonth() {
      DateTime firstDayOfWeek = this.workWeek.StartDate;
      this.SelectDate(firstDayOfWeek.AddMonths(-1));
    }

    private void SelectNextMonth() {
      DateTime firstDayOfWeek = this.workWeek.StartDate;
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
      this.WorkWeek =
        this.WorkMonth.Weeks.First(
          ww => ww.WeekOfYear == this.calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
      // look for workday and focus
      var selectedWorkDay = this.workWeek.Days.FirstOrDefault(d => d.Day == date.Day);
      SelectWorkDay(selectedWorkDay);
    }

    public void SelectWorkDay(WorkDay selectedWorkDay) {
      if (this.SelectedWorkDay != null) {
        this.SelectedWorkDay.FocusMe = false;
      }
      this.SelectedWorkDay = selectedWorkDay;
      if (this.SelectedWorkDay != null) {
        this.SelectedWorkDay.FocusMe = true;
      }
    }

    public WorkDay SelectedWorkDay { get; set; }


    private void CreateAndLoadYear(int year) {
      if (this.WorkYear != null) {
        // need to save years data
        this.Save();
      }
      var sw = Stopwatch.StartNew();
      this.WorkYear = new WorkYear(year, this.MonlistSettings.ParserSettings.ShortCuts,
                                   this.MonlistSettings.MainSettings.HitListLookBackInWeeks,
                                   this.MonlistSettings.MainSettings.HoursPerDay,
                                   this.PNSearch,
                                   this.PositionSearch);
      this.loadingData = true;
      this.PersistentResult = this.persistenceLayer.SetDataOfYear(this.WorkYear);
      this.loadingData = false;
      Console.WriteLine("reading data took: {0}ms", sw.ElapsedMilliseconds);
    }

    public void Save() {
      // save data
      this.persistenceLayer.SaveData(this.workYear);
      this.csvExporter.Export(this.WorkYear);
      // save settings
      WriteSettings(this.MonlistSettings, this.settingsFile);
    }

    public void CopyFromPreviousDay(WorkDay currentDay) {
      WorkDay lastValidBefore =
        this.WorkMonth.Days.LastOrDefault(x => x.Day < currentDay.Day && x.Items != null && x.Items.Any());
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
      int idx = this.MonlistSettings.ParserSettings.ShortCuts.IndexOf(sc);
      if (idx > 0) {
        this.MonlistSettings.ParserSettings.ShortCuts.Remove(sc);
        this.MonlistSettings.ParserSettings.ShortCuts.Insert(idx - 1, sc);
      }
      this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
    }

    public void MoveShortcutDown(ShortCut sc) {
      int idx = this.MonlistSettings.ParserSettings.ShortCuts.IndexOf(sc);
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
      UpdateVisibility();
      if (this.PNSearch != null) {
        this.PNSearch.SetGBNumber(this.Settings.MainSettings.MonlistGBNumber, true);
      }
      this.EditPreferences = null;
      this.WorkWeek.Reparse();
    }

    private void UpdateVisibility() {
      this.ProjectHitListVisibility = this.MonlistSettings.MainSettings.ShowProjectHitList ? Visibility.Visible : Visibility.Collapsed;
      this.PositionHitListVisibility = this.MonlistSettings.MainSettings.ShowPositionHitList ? Visibility.Visible : Visibility.Collapsed;
      this.MonthListVisibility = this.MonlistSettings.MainSettings.ShowMonthList ? Visibility.Visible : Visibility.Collapsed;
    }

    public void CancelEditingPreferences() {
      this.EditPreferences = null;
      this.MonlistSettings = ReadSettings(this.settingsFile);
    }

    public void AddCurrentTime(WorkDay currentDay) {
      currentDay.OriginalString = WorkDayParser.Instance.AddCurrentTime(currentDay.OriginalString);
    }
  }
}