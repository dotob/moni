using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.Controls.Dialogs;
using MONI.Data;
using MONI.Util;
using NLog;
using Newtonsoft.Json;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace MONI.ViewModels
{
    public class MainViewModel : ViewModelBase, IDropTarget
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
        private bool isPositionHitListVisible;
        private ICommand previousMonthCommand;
        private ICommand previousWeekCommand;
        private bool isProjectHitListVisible;
        private WorkMonth workMonth;
        private WorkWeek workWeek;
        private WorkYear workYear;

        public MainViewModel(Dispatcher dispatcher)
        {
            // handle settings
            this.settingsFile = this.DetermineSettingsFile();
            this.MonlistSettings = ReadSettings(this.settingsFile);

            dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(UpdateVisibility));

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
            this.jsonExporter = new JSONExporter(dataDirectory);
            //this.persistentResult = this.persistenceLayer.ReadData();
            this.SelectToday(); // sets data from persistencelayer
            if (dispatcher != null)
            {
                this.throttleSaveAndCalc = new DispatcherTimer(DispatcherPriority.DataBind, dispatcher);
                this.throttleSaveAndCalc.Tick += this.throttleSaveAndCalc_Tick;
            }

            // updateinfo
            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            this.UpdateInfoViewModel = new UpdateInfoViewModel(dispatcher, this.Settings.MainSettings.UpdateInfoURL, currentVersion, this.persistenceLayer.EntryCount, this.persistenceLayer.FirstEntryDate);

            // load help
            dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(async () =>
            {
                var readme = await this.ReadHelpAsync().ConfigureAwait(false);
                this.Help = readme;
            }));

            this.SelectWorkItemTextComplete += DoSelectWorkItemTextComplete;
            this.SelectWorkItemTextWithOutTime += DoSelectWorkItemTextWithOutTime;
            this.GoToDay = DoGoToDay;
        }

        private async Task<string> ReadHelpAsync()
        {
            var reader = new AssemblyTextFileReader(Assembly.GetExecutingAssembly());
            var readme = await reader.ReadFileAsync("README.md").ConfigureAwait(false);
            var index = readme.IndexOf("## Keyboad Shortcuts ##", StringComparison.InvariantCulture);
            return readme.Remove(0, index);
        }

        private string help;

        public string Help
        {
            get { return this.help; }
            set { this.Set(ref this.help, value); }
        }

        private bool showHelp;
        private bool isMonthListVisible;
        private JSONExporter jsonExporter;

        public bool ShowHelp
        {
            get { return this.showHelp; }
            set { this.Set(ref this.showHelp, value); }
        }

        public PositionSearchViewModel PositionSearch { get; private set; }
        public PNSearchViewModel PNSearch { get; private set; }

        public Action<object> SelectWorkItemTextComplete { get; private set; }
        public Action<object> SelectWorkItemTextWithOutTime { get; private set; }
        public Action<object> GoToDay { get; private set; }

        public UpdateInfoViewModel UpdateInfoViewModel { get; set; }

        public CustomWindowPlacementSettings CustomWindowPlacementSettings
        {
            get { return this.customWindowPlacementSettings; }
            set { this.Set(ref this.customWindowPlacementSettings, value); }
        }

        public ICommand PreviousWeekCommand
        {
            get { return this.previousWeekCommand ?? (this.previousWeekCommand = new DelegateCommand(this.SelectPreviousWeek, () => true)); }
        }

        public ICommand NextWeekCommand
        {
            get { return this.nextWeekCommand ?? (this.nextWeekCommand = new DelegateCommand(this.SelectNextWeek, () => true)); }
        }

        public ICommand PreviousMonthCommand
        {
            get { return this.previousMonthCommand ?? (this.previousMonthCommand = new DelegateCommand(this.SelectPreviousMonth, () => true)); }
        }

        public ICommand NextMonthCommand
        {
            get { return this.nextMonthCommand ?? (this.nextMonthCommand = new DelegateCommand(this.SelectNextMonth, () => true)); }
        }

        public MoniSettings Settings { get; set; }

        public WorkWeek WorkWeek
        {
            get { return this.workWeek; }
            set
            {
                var monthChanged = value == null || this.workWeek == null || value.Month != this.workWeek.Month;
                if (this.Set(ref this.workWeek, value))
                {
                    // don't know if this perfect, but it works
                    if (monthChanged)
                    {
                        value?.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(value.StartDate));
                    }
                }
            }
        }

        public WorkMonth WorkMonth
        {
            get { return this.workMonth; }
            set
            {
                if (this.Set(ref this.workMonth, value))
                {
                    this.CurrentMonthMonlistImportFile = this.csvExporter.FilenameForMonth(this.workMonth);
                }
            }
        }

        public WorkYear WorkYear
        {
            get { return this.workYear; }
            set
            {
                if (this.workYear != null)
                {
                    this.workYear.PropertyChanged -= this.workYear_PropertyChanged;
                }
                if (this.Set(ref this.workYear, value))
                {
                    if (this.workYear != null)
                    {
                        this.workYear.PropertyChanged += this.workYear_PropertyChanged;
                    }
                }
            }
        }

        public ShortcutViewModel EditShortCut
        {
            get { return this.editShortCut; }
            set { this.Set(ref this.editShortCut, value); }
        }

        public MoniSettings EditPreferences
        {
            get { return this.editPreferences; }
            set { this.Set(ref this.editPreferences, value); }
        }

        public bool IsProjectHitListVisible
        {
            get { return this.isProjectHitListVisible; }
            private set { this.Set(ref this.isProjectHitListVisible, value); }
        }

        public bool IsPositionHitListVisible
        {
            get { return this.isPositionHitListVisible; }
            set { this.Set(ref this.isPositionHitListVisible, value); }
        }

        public bool IsMonthListVisible
        {
            get { return this.isMonthListVisible; }
            set { this.Set(ref this.isMonthListVisible, value); }
        }

        private MoniSettings MonlistSettings
        {
            get { return this.monlistSettings; }
            set
            {
                this.monlistSettings = value;
                MoniSettings.Current = this.MonlistSettings;
            }
        }

        public ReadWriteResult PersistentResult { get; set; }

        public string CurrentMonthMonlistImportFile { get; private set; }

        public LoginDialogData MonlistImportLoginData { get; internal set; }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }

        public void Drop(IDropInfo dropInfo)
        {
            try
            {
                var source = (ShortCutStatistic)dropInfo.DragInfo.Data;
                if (source != null)
                {
                    var targetIndex = dropInfo.InsertIndex;
                    var targetGroup = dropInfo.TargetGroup != null ? dropInfo.TargetGroup.Name.ToString() : string.Empty;
                    source.SetNewGroup(targetGroup, targetIndex);
                    DragDrop.DefaultDropHandler.Drop(dropInfo);

                    this.MonlistSettings.ParserSettings.ShortCuts.Clear();
                    this.MonlistSettings.ParserSettings.ShortCuts.AddRange(dropInfo.TargetCollection.OfType<ShortCutStatistic>());
                    //this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static void DoSelectWorkItemTextComplete(object o)
        {
            WorkItem workItem;
            TextBox tb;
            string wholeString;
            if (!GetTextBoxWorkItemInfo(o, out workItem, out tb, out wholeString)) { return; }

            if (!string.IsNullOrWhiteSpace(wholeString))
            {
                string searchString = workItem.OriginalString;
                int selStart = wholeString.IndexOf(searchString, StringComparison.InvariantCulture);
                if (selStart >= 0)
                {
                    tb.Select(selStart, searchString.Length);
                    Clipboard.SetText(searchString);
                }
            }
        }

        private void DoSelectWorkItemTextWithOutTime(object o)
        {
            WorkItem workItem;
            TextBox tb;
            string wholeString;
            if (!GetTextBoxWorkItemInfo(o, out workItem, out tb, out wholeString)) { return; }

            if (!string.IsNullOrWhiteSpace(wholeString))
            {
                string searchString = workItem.OriginalString.Token(";", 2);
                int selStart = wholeString.IndexOf(searchString, StringComparison.InvariantCulture);
                if (selStart >= 0)
                {
                    tb.Select(selStart, searchString.Length);
                    Clipboard.SetText(searchString);
                }
            }
        }

        private void DoGoToDay(object o)
        {
            var wd = o as WorkDay;
            if (wd != null)
            {
                this.SelectDate(wd.DateTime);
            }
        }

        private static bool GetTextBoxWorkItemInfo(object o, out WorkItem workItem, out TextBox tb, out string wholeString)
        {
            workItem = null;
            tb = null;
            wholeString = null;
            var bindedValues = o as IEnumerable;
            if (bindedValues == null)
            {
                return false;
            }
            workItem = bindedValues.OfType<object>().ElementAtOrDefault(0) as WorkItem;
            tb = bindedValues.OfType<object>().ElementAtOrDefault(1) as TextBox;
            if (workItem == null || tb == null)
            {
                return false;
            }
            tb.Focus();
            wholeString = tb.Text;
            return true;
        }

        private string DetermineSettingsFile()
        {
            logger.Debug("determine settingsfile location");
            // check if there is a settings file in userdir
            var fileName = "settings.json";
            var moniAppData = Utils.MoniAppDataPath();
            var moniAppDataSettingsFile = Path.Combine(moniAppData, fileName);
            if (File.Exists(moniAppDataSettingsFile))
            {
                logger.Debug("found settingsfile in appdata: {0}", moniAppDataSettingsFile);
                return moniAppDataSettingsFile;
            }
            // check if we can create settings file in exe dir
            if (Utils.CanCreateFile("."))
            {
                logger.Debug("could write in currentdir: {0} use {1} as settingsfile", Directory.GetCurrentDirectory(), fileName);
                return fileName;
            }
            logger.Debug("create new settingsfile in appdata: {0}", moniAppDataSettingsFile);
            return moniAppDataSettingsFile;
        }

        private void throttleSaveAndCalc_Tick(object sender, EventArgs e)
        {
            this.SaveAndCalc();
            this.throttleSaveAndCalc.Stop();
        }

        private static MoniSettings ReadSettings(string settingsFile)
        {
            MoniSettings settings = ReadSettingsInternal(settingsFile);
            PatchSettings(settings);
            return settings;
        }

        private static void PatchSettings(MoniSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.MainSettings.UpdateInfoURL))
            {
                settings.MainSettings.UpdateInfoURL = MoniSettings.GetEmptySettings().MainSettings.UpdateInfoURL;
            }
        }

        private static MoniSettings ReadSettingsInternal(string settingsFile)
        {
            if (File.Exists(settingsFile))
            {
                var jsonString = File.ReadAllText(settingsFile);
                logger.Debug("read settings from {0}: {1}", settingsFile, jsonString);
                return JsonConvert.DeserializeObject<MoniSettings>(jsonString);
            }

            // no settingsfile found, try to read sample settings
            string settingsJsonSkeleton = "settings.json.skeleton";
            if (File.Exists(settingsJsonSkeleton))
            {
                string jsonString = File.ReadAllText(settingsJsonSkeleton);
                return JsonConvert.DeserializeObject<MoniSettings>(jsonString);
            }

            // no samplesettings, use default
            return MoniSettings.GetEmptySettings();
        }

        private static void WriteSettings(MoniSettings settings, string settingsFile)
        {
            string settingsAsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(settingsFile, settingsAsJson);
        }

        private void workYear_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HoursDuration")
            {
                if (!this.loadingData)
                {
                    // try next save in 500ms
                    this.throttleSaveAndCalc.Stop();
                    this.throttleSaveAndCalc.Interval = TimeSpan.FromMilliseconds(500);
                    this.throttleSaveAndCalc.Start();
                }
            }
        }

        private void SaveAndCalc()
        {
            this.Save();
            this.WorkMonth.CalcShortCutStatistic();
        }

        private void SelectPreviousWeek()
        {
            DateTime firstDayOfWeek = this.workWeek.StartDate;
            DateTime dateTime = firstDayOfWeek.AddDays(-1);
            this.SelectDate(dateTime);
        }

        private void SelectNextWeek()
        {
            var firstDayOfWeek = this.workWeek.StartDate;
            var dateTime = firstDayOfWeek.AddDays(7);
            if (firstDayOfWeek.Month == dateTime.Month)
            {
                this.SelectDate(dateTime);
            }
            else
            {
                var nextMonth = firstDayOfWeek.AddMonths(1);
                var nuDate = nextMonth.AddDays(-1 * nextMonth.Day + 1);
                this.SelectDate(nuDate);
            }
        }


        private void SelectPreviousMonth()
        {
            DateTime firstDayOfWeek = this.workWeek.StartDate;
            this.SelectDate(firstDayOfWeek.AddMonths(-1));
        }

        private void SelectNextMonth()
        {
            DateTime firstDayOfWeek = this.workWeek.StartDate;
            this.SelectDate(firstDayOfWeek.AddMonths(1));
        }

        public void SelectToday()
        {
            this.SelectDate(DateTime.Now);
        }

        public void SelectDate(DateTime date)
        {
            if (this.workYear == null || date.Year != this.workYear.Year)
            {
                this.CreateAndLoadYear(date.Year);
            }
            if (this.workMonth == null || date.Year != this.workMonth.Year || date.Month != this.workMonth.Month)
            {
                this.WorkMonth = this.WorkYear.Months.ElementAt(date.Month - 1);
                this.WorkMonth.CalcPreviewHours();
            }
            this.WorkWeek = this.WorkMonth.Weeks.First(ww => ww.WeekOfYear == CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date));
            // look for workday and focus
            var selectedWorkDay = this.workWeek.Days.FirstOrDefault(d => d.Day == date.Day);
            SelectWorkDay(selectedWorkDay);
        }

        public void SelectWorkDay(WorkDay selectedWorkDay)
        {
            if (this.SelectedWorkDay != null)
            {
                this.SelectedWorkDay.FocusMe = false;
            }
            this.SelectedWorkDay = selectedWorkDay;
            if (this.SelectedWorkDay != null)
            {
                this.SelectedWorkDay.FocusMe = true;
            }
        }

        public WorkDay SelectedWorkDay { get; set; }


        private void CreateAndLoadYear(int year)
        {
            if (this.WorkYear != null)
            {
                // need to save years data
                this.Save();
            }
            var sw = Stopwatch.StartNew();
            this.WorkYear = new WorkYear(year, this.MonlistSettings.ParserSettings,
                this.MonlistSettings.MainSettings.HitListLookBackInWeeks,
                this.MonlistSettings.MainSettings.HoursPerDay,
                this.PNSearch,
                this.PositionSearch);
            this.loadingData = true;
            this.PersistentResult = this.persistenceLayer.SetDataOfYear(this.WorkYear);
            this.loadingData = false;
            Console.WriteLine("reading data took: {0}ms", sw.ElapsedMilliseconds);
        }

        public void Save()
        {
            // save data
            this.persistenceLayer.SaveData(this.workYear);
            this.csvExporter.Export(this.WorkYear);
            this.jsonExporter.Export(this.WorkYear);
            // save settings
            WriteSettings(this.MonlistSettings, this.settingsFile);
        }

        public void CopyFromPreviousDay(WorkDay currentDay)
        {
            WorkDay lastValidBefore = this.WorkMonth.Days.LastOrDefault(x => x.Day < currentDay.Day && x.Items != null && x.Items.Any());
            if (lastValidBefore != null)
            {
                currentDay.OriginalString = lastValidBefore.OriginalString;
            }
        }

        public void DeleteShortcut(ShortCut delsc)
        {
            this.MonlistSettings.ParserSettings.ShortCuts.Remove(delsc);
            this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
            this.WorkWeek.Reparse();
        }

        public void MoveShortcutUp(ShortCut sc)
        {
            int idx = this.MonlistSettings.ParserSettings.ShortCuts.IndexOf(sc);
            if (idx > 0)
            {
                this.MonlistSettings.ParserSettings.ShortCuts.Remove(sc);
                this.MonlistSettings.ParserSettings.ShortCuts.Insert(idx - 1, sc);
            }
            this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
        }

        public void MoveShortcutDown(ShortCut sc)
        {
            int idx = this.MonlistSettings.ParserSettings.ShortCuts.IndexOf(sc);
            if (idx < this.MonlistSettings.ParserSettings.ShortCuts.Count - 1)
            {
                this.MonlistSettings.ParserSettings.ShortCuts.Remove(sc);
                this.MonlistSettings.ParserSettings.ShortCuts.Insert(idx + 1, sc);
            }
            this.WorkWeek.Month.ReloadShortcutStatistic(this.MonlistSettings.ParserSettings.GetValidShortCuts(this.WorkWeek.StartDate));
        }

        public void StartEditingPreferences()
        {
            this.EditPreferences = this.MonlistSettings;
        }

        public async Task SaveEditingPreferencesAsync()
        {
            UpdateVisibility();
            if (this.PNSearch != null)
            {
                await this.PNSearch.ReadFileAsync(this.Settings.MainSettings.ProjectNumberFilePath, this.Settings.MainSettings.MonlistGBNumber);
            }
            if (this.PositionSearch != null)
            {
                await this.PositionSearch.ReadFileAsync(this.Settings.MainSettings.PositionNumberFilePath);
            }
            this.EditPreferences = null;
            this.WorkWeek.Reparse();
        }

        private void UpdateVisibility()
        {
            this.IsProjectHitListVisible = this.MonlistSettings.MainSettings.ShowProjectHitList;
            this.IsPositionHitListVisible = this.MonlistSettings.MainSettings.ShowPositionHitList;
            this.IsMonthListVisible = this.MonlistSettings.MainSettings.ShowMonthList;
        }

        public void CancelEditingPreferences()
        {
            this.EditPreferences = null;
            this.MonlistSettings = ReadSettings(this.settingsFile);
        }

        public void AddCurrentTime(WorkDay currentDay)
        {
            currentDay.OriginalString = WorkDayParser.Instance.AddCurrentTime(currentDay.OriginalString);
        }
    }
}