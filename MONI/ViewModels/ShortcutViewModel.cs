using System;
using System.Linq;
using System.Windows.Input;
using MONI.Data;
using MONI.Util;

namespace MONI.ViewModels
{
  public class ShortcutViewModel : ViewModelBase
  {
    private ShortCut model;
    private readonly MoniSettings moniSettings;
    private readonly WorkWeek workWeek;
    private readonly Action viewcloseAction;
    private ICommand cancelCommand;
    private ICommand saveCommand;

    public ShortcutViewModel(ShortCut shortCut, WorkWeek workWeek, MoniSettings settings, Action closeAction) {
      this.viewcloseAction = closeAction;
      this.moniSettings = settings;
      this.workWeek = workWeek;
      this.Model = shortCut;
    }

    public ShortCut Model {
      get { return this.model; }
      set {
        if (Equals(value, this.model)) {
          return;
        }
        this.model = value;
        this.OnPropertyChanged(() => this.Model);
      }
    }

    public ICommand SaveCommand {
      get { return this.saveCommand ?? (this.saveCommand = new DelegateCommand(this.SaveShortcut, this.CanSave)); }
    }

    public void SaveShortcut() {
      var shortCut = this.moniSettings.ParserSettings.ShortCuts.FirstOrDefault(sc => Equals(sc, this.Model));
      if (shortCut != null) {
        shortCut.GetData(this.Model);
      } else {
        this.moniSettings.ParserSettings.ShortCuts.Add(this.Model);
      }
      this.Model = null;
      this.workWeek.Month.ReloadShortcutStatistic(this.moniSettings.ParserSettings.GetValidShortCuts(this.workWeek.StartDate));
      this.workWeek.Reparse();

      var action = this.viewcloseAction;
      if (action != null) {
        action();
      }
    }

    public bool CanSave() {
      return this.Model != null
             && !string.IsNullOrWhiteSpace(this.Model.Key)
             && !string.IsNullOrWhiteSpace(this.Model.Expansion);
    }

    public ICommand CancelCommand {
      get { return this.cancelCommand ?? (this.cancelCommand = new DelegateCommand(this.viewcloseAction, () => this.viewcloseAction != null)); }
    }
  }
}