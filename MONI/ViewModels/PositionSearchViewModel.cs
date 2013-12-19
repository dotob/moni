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

namespace MONI.ViewModels {
  public class PositionSearchViewModel : ViewModelBase {
    private bool showPnSearch;
    private string searchText;
    private ICommand cancelCommand;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();


    public PositionSearchViewModel(string projectNumberFiles) {
      this.Results = new QuickFillObservableCollection<PositionNumber>();
      this.ProjectNumbers = new List<PositionNumber>();
      Task.Factory.StartNew(() => this.ReadPNFile(projectNumberFiles));
    }

    private void ReadPNFile(string pnFilePaths) {
      if (!string.IsNullOrWhiteSpace(pnFilePaths)) {
        foreach (var pnFileUnpatched in pnFilePaths.Split(';')) {
          var pnFile = Utils.PatchFilePath(pnFileUnpatched);
          if (!string.IsNullOrWhiteSpace(pnFile) && File.Exists(pnFile)) {
            var allPnLines = File.ReadAllLines(pnFile, Encoding.Default);
            foreach (string line in allPnLines.Skip(1)) {
              try {
                var pn = new PositionNumber();
                string[] columns = line.Split(';');
                pn.Number = columns[0];
                pn.Description = columns[1];
                pn.Customer = columns[2];
                this.ProjectNumbers.Add(pn);
              } catch (Exception e) {
                logger.Warn("Could not read as positionnumber info: {0}", line);
              }
            }
            // break after first file worked
            break;
          }
        }
      }
    }

    protected List<PositionNumber> ProjectNumbers { get; set; }

    public bool ShowPNSearch {
      get { return this.showPnSearch; }
      set {
        this.showPnSearch = value;
        this.OnPropertyChanged(() => this.ShowPNSearch);
      }
    }

    public string SearchText {
      get { return this.searchText; }
      set {
        this.searchText = value;
        this.Search();
      }
    }

    private void Search() {
      this.Results.Clear();
      var s = this.searchText;
      if (!string.IsNullOrWhiteSpace(s)) {
        try {
          var res = this.ProjectNumbers.Where(pn => Regex.IsMatch(pn.Number, s, RegexOptions.IgnoreCase) || Regex.IsMatch(pn.Description, s, RegexOptions.IgnoreCase) || Regex.IsMatch(pn.Customer, s, RegexOptions.IgnoreCase));
          this.Results.Fill(res);
        } catch (Exception e) {
          // ignore, usually there is an unfinished regex
        }
      }
    }

    public QuickFillObservableCollection<PositionNumber> Results { get; private set; }

    public ICommand CancelCommand {
      get { return this.cancelCommand ?? (this.cancelCommand = new DelegateCommand(() => this.ShowPNSearch = false, () => true)); }
    }

    public string GetDescriptionForPositionNumber(string key) {
      //TODO
      return key;
    }
  }

  public class PositionNumber {
    public string Number { get; set; }
    public string Description { get; set; }
    public string Customer { get; set; }
  }
}