using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MONI.Data;
using MahApps.Metro.Controls;

namespace MONI
{
  /// <summary>
  /// Interaction logic for MainView.xaml
  /// </summary>
  public partial class MainView : MetroWindow
  {
    public MainView() {
      this.ViewModel = new MainViewModel();
      this.InitializeComponent();
      this.Title = string.Format("MONI {0}", Assembly.GetExecutingAssembly().GetName().Version);
      this.CheckForMonlist();
    }

    private void CheckForMonlist() {
      var mlp = this.ViewModel.Settings.MainSettings.MonlistExecutablePath;
      if (string.IsNullOrWhiteSpace(mlp) || !File.Exists(mlp)) {
        this.OpenMonlist.IsEnabled = false;
      }
    }

    public MainViewModel ViewModel { get; set; }

    private void mv_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      this.ViewModel.Save();
    }

    private void mv_PreviewKeyDown(object sender, KeyEventArgs e) {
      if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
        switch (e.Key) {
          case Key.Left:
            if (this.ViewModel.PreviousWeekCommand.CanExecute(null)) {
              this.ViewModel.PreviousWeekCommand.Execute(null);
            }
            break;
          case Key.Right:
            if (this.ViewModel.NextWeekCommand.CanExecute(null)) {
              this.ViewModel.NextWeekCommand.Execute(null);
            }
            break;
          case Key.U:
            var activeControl = e.OriginalSource as TextBox;
            if (activeControl != null) {
              var currentDay = activeControl.DataContext as WorkDay;
              this.ViewModel.CopyFromPreviousDay(currentDay);
              e.Handled = true;
            }
            break;
        }
      } else if (e.Key == Key.Escape) {
        // goto today
        this.ViewModel.SelectToday();
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
      ShortCut newSc = new ShortCut();
      this.ViewModel.EditShortCut = newSc;
      this.shortcutTextBox.Focus();
    }

    private void RemoveShortcut_OnClick(object sender, RoutedEventArgs e) {
      var sc = GetShortCutFromButton(sender);
      if (sc != null) {
        this.ViewModel.DeleteShortcut(sc);
      }
    }

    private void EditShortcutCancel_OnClick(object sender, RoutedEventArgs e) {
      this.ViewModel.EditShortCut = null;
    }

    private void EditShortcutSave_OnClick(object sender, RoutedEventArgs e) {
      this.ViewModel.SaveEditShortcut();
    }

    private void EditShortcut_OnClick(object sender, RoutedEventArgs e) {
      var sc = GetShortCutFromButton(sender);
      if (sc != null) {
        this.ViewModel.EditShortCut = sc;
        this.shortcutTextBox.Focus();
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
    }

    private void ToMonlist_Button_Click(object sender, RoutedEventArgs e) {
      // TODO
    }
  }
}
