using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MONI.Data;
using MONI.Util;

namespace MONI.ViewModels
{
  public class PNSearchViewModel : ViewModelBase
  {
    private bool showPnSearch;
    private readonly string projectNumberFile;
    private string searchText;
    private ICommand cancelCommand;

    public PNSearchViewModel(string projectNumberFile) {
      this.Results = new ObservableCollection<ProjectNumber>();
      this.ProjectNumbers = new List<ProjectNumber>();
      this.projectNumberFile = projectNumberFile;
      Task.Factory.StartNew(()=> this.ReadPNFile(projectNumberFile));
    }

    private void ReadPNFile(string pnFilePath) {
      if (!string.IsNullOrWhiteSpace(pnFilePath) && File.Exists(pnFilePath)) {
        var allPnLines = File.ReadAllLines(pnFilePath, Encoding.Default);
        foreach (string line in allPnLines.Skip(1)) {
          var pn = new ProjectNumber();
          pn.Number = line.Substring(0, 5);
          pn.Description = line.Substring(14);
          this.ProjectNumbers.Add(pn);
        }
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
      var res = this.ProjectNumbers.Where(pn => Regex.IsMatch(pn.Number, s, RegexOptions.IgnoreCase) || Regex.IsMatch(pn.Description, s, RegexOptions.IgnoreCase));
      foreach (var pn in res) {
        this.Results.Add(pn);
      }
    }

    public ObservableCollection<ProjectNumber> Results { get; private set; }

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