using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MONI.Util;
using NLog;

namespace MONI.ViewModels
{
    public class PositionSearchViewModel : ViewModelBase
    {
        private bool isPosSearchViewOpen;
        private string searchText;
        private ICommand cancelCommand;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<string, string> pnHash;
        private List<PositionNumber> posNumbers = new List<PositionNumber>();

        public PositionSearchViewModel(string posNumberFiles)
        {
            this.Results = new QuickFillObservableCollection<PositionNumber>();
            this.CancelCommand = new DelegateCommand(() => this.IsPosSearchViewOpen = false, () => true);
            ReadFileAsync(posNumberFiles);
        }

        public Task ReadFileAsync(string posNumberFiles)
        {
            return Task
                .Run(() => this.ReadFile(posNumberFiles))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        var exception = t.Exception?.InnerException ?? t.Exception;
                        if (exception != null)
                        {
                            logger.Error(exception, "Could not read the position numbers file:");
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ReadFile(string posNumberFiles)
        {
            this.posNumbers.Clear();

            if (!string.IsNullOrWhiteSpace(posNumberFiles))
            {
                foreach (var pnFileUnpatched in posNumberFiles.Split(';'))
                {
                    var pnFile = Utils.PatchFilePath(pnFileUnpatched);
                    if (!string.IsNullOrWhiteSpace(pnFile) && File.Exists(pnFile))
                    {
                        var allPnLines = File.ReadAllLines(pnFile, Encoding.Default);
                        foreach (string line in allPnLines.Skip(1))
                        {
                            try
                            {
                                var pn = new PositionNumber();
                                if (pn.TryParse(line))
                                {
                                    this.posNumbers.Add(pn);
                                }
                                else
                                {
                                    if (pn.Exception != null)
                                    {
                                        logger.Warn(pn.Exception, $"Could not parse position number line: {line} :");
                                    }
                                    else
                                    {
                                        logger.Warn($"Could not parse position number line: {line}");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn(e, $"Could not parse position number line: {line} :");
                            }
                        }
                        // break after first file worked
                        break;
                    }
                }
            }

            this.Search();

            this.pnHash = this.posNumbers.ToDictionary(pnum => pnum.Number, pnum => pnum.Description);
        }

        public bool IsPosSearchViewOpen
        {
            get { return this.isPosSearchViewOpen; }
            set { this.Set(ref this.isPosSearchViewOpen, value); }
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
            var s = this.searchText;
            if (!string.IsNullOrWhiteSpace(s))
            {
                try
                {
                    var res = this.posNumbers.Where(pn => Regex.IsMatch(pn.Number, s, RegexOptions.IgnoreCase) || Regex.IsMatch(pn.Description, s, RegexOptions.IgnoreCase) || Regex.IsMatch(pn.Customer, s, RegexOptions.IgnoreCase));
                    this.Results.AddItems(res, true);
                }
                catch (Exception ex)
                {
                    // ignore, usually there is an unfinished regex
                }
            }
            else
            {
                this.Results.AddItems(this.posNumbers, true);
            }
        }

        public QuickFillObservableCollection<PositionNumber> Results { get; private set; }

        public ICommand CancelCommand
        {
            get { return this.cancelCommand; }
            set { this.Set(ref this.cancelCommand, value); }
        }

        public string GetDescriptionForPositionNumber(string positionNumber)
        {
            string ret;
            if (this.pnHash.TryGetValue(positionNumber, out ret))
            {
                return ret;
            }
            return null;
        }
    }

    public class PositionNumber
    {
        public string Number { get; private set; }
        public string Description { get; private set; }
        public string Customer { get; private set; }
        public Exception Exception { get; private set; }

        public bool TryParse(string line)
        {
            try
            {
                var columns = line.Split(';');
                Number = columns[0];
                Description = columns[1];
                Customer = columns[2];
            }
            catch (Exception e)
            {
                this.Exception = e;
                return false;
            }
            return true;
        }
    }
}