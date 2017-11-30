using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MONI.Util;
using System;
using System.Threading;
using NLog;

namespace MONI.ViewModels
{
    public class PNSearchViewModel : ViewModelBase
    {
        public static readonly Regex ProjectFileLineRegex = new Regex(@"^(?<pn>\d{5})\s(\S{1,5})\s(\S{1,5})\s(\S{1,5})\s(?<isold>\d{1})\s(?<desc>.*)$", RegexOptions.Compiled);
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private bool isProjectSearchViewOpen;
        private string searchText;
        private ICommand cancelCommand;
        private Dictionary<string, string> pnHash;
        private int currentGBNumber;
        private List<ProjectNumber> projectNumbers = new List<ProjectNumber>();
        private List<ProjectNumber> projectNumbersToSearch = new List<ProjectNumber>();
        private bool filterOldProjectsOut = true;

        public PNSearchViewModel(string projectNumberFiles, int gbNumber)
        {
            this.Results = new QuickFillObservableCollection<ProjectNumber>();
            this.CancelCommand = new DelegateCommand(() => this.IsProjectSearchViewOpen = false, () => true);
            ReadFileAsync(projectNumberFiles, gbNumber);
        }

        public Task ReadFileAsync(string projectNumberFiles, int gbNumber)
        {
            return Task
                .Run(() => this.ReadFile(projectNumberFiles, gbNumber))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        var exception = t.Exception?.InnerException ?? t.Exception;
                        if (exception != null)
                        {
                            logger.Error(exception, "Could not read the project numbers file:");
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void FilterByGBNumber(int gbNumber)
        {
            this.currentGBNumber = gbNumber / 10;
            this.projectNumbersToSearch = this.projectNumbers.Where(pn => this.currentGBNumber <= 0 || pn.GB == this.currentGBNumber).ToList();
            this.Search();
        }

        private void ReadFile(string pnFilePaths, int gbNumber)
        {
            this.projectNumbers.Clear();

            if (!string.IsNullOrWhiteSpace(pnFilePaths))
            {
                foreach (var pnFileUnpatched in pnFilePaths.Split(';'))
                {
                    var pnFile = Utils.PatchFilePath(pnFileUnpatched);
                    if (!string.IsNullOrWhiteSpace(pnFile) && File.Exists(pnFile))
                    {
                        var allPnLines = File.ReadAllLines(pnFile, Encoding.Default);
                        foreach (var line in allPnLines.Skip(1))
                        {
                            var pn = new ProjectNumber();
                            if (pn.TryParse(line))
                            {
                                this.projectNumbers.Add(pn);
                            }
                            else
                            {
                                logger.Warn($"Could not parse project numbers line: {line}");
                            }
                        }
                        // break after first file worked
                        break;
                    }
                }
            }

            this.FilterByGBNumber(gbNumber);

            this.pnHash = this.projectNumbers.ToDictionary(pnum => pnum.Number, pnum => pnum.Description);
        }

        public bool IsProjectSearchViewOpen
        {
            get { return this.isProjectSearchViewOpen; }
            set { this.Set(ref this.isProjectSearchViewOpen, value); }
        }

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                if (this.Set(ref this.searchText, value))
                {
                    this.Search();
                }
            }
        }

        private void Search()
        {
            var numbers = this.projectNumbersToSearch.Where(pn => (!this.FilterOldProjectsOut || !pn.IsOld));
            var s = this.searchText;
            if (!string.IsNullOrWhiteSpace(s))
            {
                try
                {
                    var res = numbers.Where(pn => Regex.IsMatch(pn.Number, s, RegexOptions.IgnoreCase) || Regex.IsMatch(pn.Description, s, RegexOptions.IgnoreCase));
                    this.Results.AddItems(res, true);
                }
                catch (Exception ex)
                {
                    // ignore, usually there is an unfinished regex
                }
            }
            else
            {
                this.Results.AddItems(numbers, true);
            }
        }

        public string GetDescriptionForProjectNumber(string positionNumber)
        {
            string ret;
            if (this.pnHash.TryGetValue(positionNumber, out ret))
            {
                return ret;
            }
            return null;
        }

        public QuickFillObservableCollection<ProjectNumber> Results { get; private set; }

        public ICommand CancelCommand
        {
            get { return this.cancelCommand; }
            set { this.Set(ref this.cancelCommand, value); }
        }

        public bool FilterOldProjectsOut
        {
            get { return this.filterOldProjectsOut; }
            set
            {
                if (this.Set(ref this.filterOldProjectsOut, value))
                {
                    this.Search();
                }
            }
        }
    }

    public class ProjectNumber
    {
        public ProjectNumber()
        {
        }

        public int GB { get; private set; }
        public string Number { get; private set; }
        public bool IsOld { get; private set; }
        public string Description { get; private set; }

        public bool TryParse(string line)
        {
            var lineMatch = PNSearchViewModel.ProjectFileLineRegex.Match(line);
            var success = lineMatch.Success;
            if (success)
            {
                this.Number = lineMatch.Groups["pn"].Value;
                this.GB = Convert.ToInt32(this.Number.Substring(0, 1));
                this.IsOld = lineMatch.Groups["isold"].Value == "1";
                this.Description = lineMatch.Groups["desc"].Value;
            }
            return success;
        }
    }
}