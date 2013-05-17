using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using MONI.Data;
using MONI.Util;

namespace MONI.ViewModels
{
  public class ShortcutViewModel : ViewModelBase, IDataErrorInfo
  {
    private ShortCut model;
    private readonly MoniSettings moniSettings;
    private readonly WorkWeek workWeek;
    private readonly Action viewcloseAction;
    private ICommand cancelCommand;
    private ICommand saveCommand;
    private bool isNew;
    private string shortCutKey;

    public ShortcutViewModel(ShortCut shortCut, WorkWeek workWeek, MoniSettings settings, Action closeAction) {
      this.viewcloseAction = closeAction;
      this.moniSettings = settings;
      this.workWeek = workWeek;
      this.Model = shortCut ?? new ShortCut();
      this.IsNew = shortCut == null;
      this.ShortCutKey = this.Model.Key;
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

    public bool IsNew {
      get { return this.isNew; }
      set {
        if (Equals(value, this.isNew)) {
          return;
        }
        this.isNew = value;
        this.OnPropertyChanged(() => this.IsNew);
      }
    }

    public string ShortCutKey {
      get { return this.shortCutKey; }
      set {
        if (Equals(value, this.shortCutKey)) {
          return;
        }
        this.shortCutKey = value;
        this.OnPropertyChanged(() => this.ShortCutKey);
        this.Model.Key = value;
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
             && !string.IsNullOrWhiteSpace(this.ShortCutKey) && !this.ShortCutKeyExists(this.ShortCutKey)
             && !string.IsNullOrWhiteSpace(this.Model.Expansion);
    }

    public ICommand CancelCommand {
      get { return this.cancelCommand ?? (this.cancelCommand = new DelegateCommand(this.viewcloseAction, () => this.viewcloseAction != null)); }
    }

    private bool ShortCutKeyExists(string key) {
      if (!this.IsNew) {
        return false;
      }
      var shortCut = this.moniSettings.ParserSettings.ShortCuts.FirstOrDefault(sc => Equals(sc.Key, key));
      return shortCut != null;
    }

    public string this[string columnName] {
      get {
        if (Equals("ShortCutKey", columnName)) {
          if (this.ShortCutKeyExists(this.ShortCutKey)) {
            return "Der Shortcut existiert schon!";
          }
        }
        return string.Empty;
      }
    }

    public string Error { get; private set; }
  }

}