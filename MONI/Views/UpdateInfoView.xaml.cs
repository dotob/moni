using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MONI.ViewModels;

namespace MONI.Views
{
  /// <summary>
  /// Interaction logic for ShortcutView.xaml
  /// </summary>
  public partial class UpdateInfoView : UserControl
  {
    public UpdateInfoView() {
      this.InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
      var ui = this.DataContext as UpdateInfoViewModel;
      if (ui != null) {
        Process.Start(ui.UpdateInfo.DownLoadURL);
      }
    }
  }
}