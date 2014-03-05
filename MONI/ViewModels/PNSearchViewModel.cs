using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MONI.Util;

namespace MONI.ViewModels
{
  public class PNSearchViewModel : ViewModelBase
  {
    private bool showPnSearch;
    private string searchText;
    private ICommand cancelCommand;
    private Dictionary<string, string> pnHash;

    public PNSearchViewModel(string projectNumberFiles) {
      this.Results = new QuickFillObservableCollection<ProjectNumber>();
      this.ProjectNumbers = new List<ProjectNumber>();
      Task.Factory.StartNew(()=> this.ReadPNFile(projectNumberFiles));
    }

    private void ReadPNFile(string pnFilePaths) {
      if (!string.IsNullOrWhiteSpace(pnFilePaths)) {
        foreach (var pnFileUnpatched in pnFilePaths.Split(';')) {
          var pnFile = Utils.PatchFilePath(pnFileUnpatched);
          if (!string.IsNullOrWhiteSpace(pnFile) && File.Exists(pnFile)) {
            var allPnLines = File.ReadAllLines(pnFile, Encoding.Default);
            foreach (string line in allPnLines.Skip(1)) {
              var pn = new ProjectNumber();
              pn.Number = line.Substring(0, 5);
              pn.Description = line.Substring(14);
              this.ProjectNumbers.Add(pn);
            }
            // break after first file worked
            break;
          }
        }
        this.pnHash = this.ProjectNumbers.ToDictionary(pnum => pnum.Number, pnum => pnum.Description);
      }
    }

    protected List<ProjectNumber> ProjectNumbers { get; set; }

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
        var res = this.ProjectNumbers.Where(pn => Regex.IsMatch(pn.Number, s, RegexOptions.IgnoreCase) || Regex.IsMatch(pn.Description, s, RegexOptions.IgnoreCase));
        this.Results.Fill(res);
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

    public ICommand CancelCommand {
      get { return this.cancelCommand ?? (this.cancelCommand = new DelegateCommand(() => this.ShowPNSearch = false, () => true)); }
    }
  }

  public class ProjectNumber
  {
    public string Number { get; set; }
    public string Description { get; set; }
  }
}