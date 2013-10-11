using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MONI.Data;
using MONI.Util;
using MONI.ViewModels;
using MahApps.Metro.Controls;
using NLog;
using System.Linq;

namespace MONI.Views
{
  /// <summary>
  /// Interaction logic for MainView.xaml
  /// </summary>
  public partial class MainView : MetroWindow, IAddShortcutService
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public MainView() {
      try {
        this.ViewModel = new MainViewModel(this.Dispatcher);
      }
      catch (Exception exception) {
        MessageBox.Show(this, exception.Message, "Fehler beim Starten", MessageBoxButton.OK, MessageBoxImage.Error);
        logger.ErrorException("error while starting", exception);
      }
      this.InitializeComponent();
      this.Title = string.Format("MONI {0}", Assembly.GetExecutingAssembly().GetName().Version);
      this.CheckForMonlist();
      this.Closed += (sender, e) => this.ViewModel.Save();
      this.Activated += (s, e) =>
        {
          var readWriteResult = this.ViewModel.PersistentResult;
          if (readWriteResult != null && !readWriteResult.Success) {
            this.ViewModel.PersistentResult = null; // reset error because we have shown it. unfortunately activate gets called after messagebox ok. so this could be an endless loop
            MessageBox.Show(this, readWriteResult.Error, "Fehler beim Daten einlesen", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
          }
        };
    }

    private void CheckForMonlist() {
      var mlp = this.ViewModel.Settings.MainSettings.MonlistExecutablePath;
      this.OpenMonlist.IsEnabled = !(string.IsNullOrWhiteSpace(mlp) || !File.Exists(mlp));
    }

    public MainViewModel ViewModel { get; set; }

    private void mv_PreviewKeyDown(object sender, KeyEventArgs e) {
      if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
        switch (e.Key) {
          case Key.U:
            var activeControl = e.OriginalSource as TextBox;
            if (activeControl != null) {
              var currentDay = activeControl.DataContext as WorkDay;
              this.ViewModel.CopyFromPreviousDay(currentDay);
              e.Handled = true;
            }
            break;
          case Key.N:
            var activeTB = e.OriginalSource as TextBox;
            if (activeTB != null) {
              var currentDay = activeTB.DataContext as WorkDay;
              if (currentDay != null) {
                this.ViewModel.AddCurrentTime(currentDay);
                // set cursor to end
                activeTB.SelectionStart = currentDay.OriginalString.Length;
              }
              e.Handled = true;
            }
            break;
        }
      } else if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) {
        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
          switch (e.SystemKey) {
              // MOVE MONTH
            case Key.Left:
              if (this.ViewModel.PreviousMonthCommand.CanExecute(null)) {
                //FocusManager.SetFocusedElement(this, this.btnPrev);
                this.ViewModel.PreviousMonthCommand.Execute(null);
              }
              break;
            case Key.Right:
              if (this.ViewModel.NextMonthCommand.CanExecute(null)) {
                //FocusManager.SetFocusedElement(this, this.btnNext);
                this.ViewModel.NextMonthCommand.Execute(null);
              }
              break;
          }
        } else {
          switch (e.SystemKey) {
            // MOVE WEEK
            case Key.Left:
              if (this.ViewModel.PreviousWeekCommand.CanExecute(null)) {
                FocusManager.SetFocusedElement(this, this.btnPrev);
                this.ViewModel.PreviousWeekCommand.Execute(null);
              }
              break;
            case Key.Right:
              if (this.ViewModel.NextWeekCommand.CanExecute(null)) {
                FocusManager.SetFocusedElement(this, this.btnNext);
                this.ViewModel.NextWeekCommand.Execute(null);
              }
              break;
          }
        }
      } else {
        if (e.Key == Key.Escape) {
          // goto today
          this.ViewModel.SelectToday();
        }
      }
    }

    private void TextBox_Loaded(object sender, RoutedEventArgs e) {
      var textBox = sender as TextBox;
      if (textBox != null) {
        WorkDay workDay = textBox.Tag as WorkDay;
        if (workDay != null && workDay.Name == "today") {
          textBox.Focus();
          textBox.Select(textBox.Text.Length, 0);
        }
      }
    }

    private void GitHub_Button_Click(object sender, RoutedEventArgs e) {
      System.Diagnostics.Process.Start("https://github.com/dotob/moni");
    }

    private void AddShortcut_OnClick(object sender, RoutedEventArgs e) {
      this.ViewModel.EditShortCut = new ShortcutViewModel(null, this.ViewModel.WorkWeek, this.ViewModel.Settings, () => this.ViewModel.EditShortCut = null);
    }

    private void RemoveShortcut_OnClick(object sender, RoutedEventArgs e) {
      var sc = GetShortCutFromButton(sender);
      if (sc != null) {
        this.ViewModel.DeleteShortcut(sc);
      }
    }

    private void EditShortcut_OnClick(object sender, RoutedEventArgs e) {
      var sc = GetShortCutFromButton(sender);
      if (sc != null) {
        this.ViewModel.EditShortCut = new ShortcutViewModel(sc, this.ViewModel.WorkWeek, this.ViewModel.Settings, () => this.ViewModel.EditShortCut = null);
      }
    }

    private static ShortCut GetShortCutFromButton(object sender) {
      ShortCut sc = null;
      var button = sender as Button;
      if (button != null) {
        sc = button.Tag as ShortCut;
      }
      return sc;
    }

    private void EditPreferences_Button_Click(object sender, RoutedEventArgs e) {
      if (this.ViewModel.EditPreferences == null) {
        this.ViewModel.StartEditingPreferences();
      } else {
        this.ViewModel.CancelEditingPreferences();
      }
    }

    private void EditPreferencesCancel_OnClick(object sender, RoutedEventArgs e) {
      this.ViewModel.CancelEditingPreferences();
    }

    private void EditPreferencesSave_OnClick(object sender, RoutedEventArgs e) {
      this.ViewModel.SaveEditingPreferences();
      this.CheckForMonlist();
    }

    private void ToMonlist_Button_Click(object sender, RoutedEventArgs e) {
      this.ViewModel.ShowPasswordDialog = true;
      this.passwordBox.Focus();
    }

    private void WorkDayTextBox_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
      this.ActiveInputTextBox = sender as TextBox;
    }

    protected TextBox ActiveInputTextBox { get; set; }

    private void WorkDayTextBox_OnKeyDown(object sender, KeyEventArgs e) {
      var tb = sender as TextBox;
      if (tb != null) {
        if (e.Key == Key.PageUp) {
          var selectionStart = tb.SelectionStart;
          var text = tb.Text;
          tb.Text = WorkDayParser.Instance.Increment(text, 1, ref selectionStart);
          tb.SelectionStart = selectionStart;
          e.Handled = true;
        }
        if (e.Key == Key.PageDown) {
          var selectionStart = tb.SelectionStart;
          var text = tb.Text;
          tb.Text = WorkDayParser.Instance.Decrement(text, 1, ref selectionStart);
          tb.SelectionStart = selectionStart;
          e.Handled = true;
        }
      }
    }

    private void PasswordCancel_OnClick(object sender, RoutedEventArgs e) {
      this.ViewModel.ShowPasswordDialog = false;
    }

    private void PasswordOK_OnClick(object sender, RoutedEventArgs e) {
      this.ViewModel.ShowPasswordDialog = false;
      var password = this.passwordBox.Password;
      if (!string.IsNullOrWhiteSpace(password)) {
        var currentMonthMonlistImportFile = this.ViewModel.CurrentMonthMonlistImportFile;
        string user = this.ViewModel.Settings.MainSettings.MonlistEmployeeNumber;
        string pw = password;
        var args = string.Format("--user=\"{0}\" --pw=\"{1}\" --monat=\"{2}\" --jahr=\"{3}\" --file=\"{4}\"", user, pw, this.ViewModel.WorkMonth.Month, this.ViewModel.WorkMonth.Year, currentMonthMonlistImportFile);
        Process.Start(this.ViewModel.Settings.MainSettings.MonlistExecutablePath, args);
      }
    }

    private void ShowPNSearch_Button_Click(object sender, RoutedEventArgs e) {
      this.ViewModel.PNSearch.ShowPNSearch = true;
    }

    public void AddShortCut(string key, string expansion) {
      this.ViewModel.PNSearch.ShowPNSearch = false;
      var sc = new ShortCut(key, expansion);
      this.ViewModel.EditShortCut = new ShortcutViewModel(sc, this.ViewModel.WorkWeek, this.ViewModel.Settings, () => this.ViewModel.EditShortCut = null) { IsNew = true};
    }

    private void UIElement_OnPreviewMouseUp(object sender, MouseButtonEventArgs e) {
      var o = sender as Grid;
      if (o != null) {
        var tb = o.Tag as TextBox;
        if (tb != null) {
          var workItem = o.DataContext as WorkItem;
          if (workItem != null) {
            tb.Focus();
            var wholeString = tb.Text;
            if (!string.IsNullOrWhiteSpace(wholeString)) {
              var searchString = workItem.OriginalString;
              int selStart = wholeString.IndexOf(searchString,StringComparison.InvariantCulture);
              tb.Select(selStart, searchString.Length);
            }
          }
        }
      }
    }
  }
}