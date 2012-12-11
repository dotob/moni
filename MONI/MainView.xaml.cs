using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MONI.Data;
using MahApps.Metro.Controls;
using System.Linq;

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
        if(workDay!=null && workDay.Name=="today") {
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
      // show dataentry, set daatacontext
      // save
      this.ViewModel.AddShortcut(newSc);
    }

    private void RemoveShortcut_OnClick(object sender, RoutedEventArgs e) {
      var button = sender as Button;
      if (button != null) {
        ShortCut delsc = button.Tag as ShortCut;
        if (delsc != null) {
          this.ViewModel.DeleteShortcut(delsc);
        }
      }
    }
  }
}
