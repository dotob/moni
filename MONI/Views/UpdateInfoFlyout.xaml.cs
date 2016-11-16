using System.Diagnostics;
using System.Windows;
using MahApps.Metro.Controls;
using MONI.ViewModels;

namespace MONI.Views
{
  /// <summary>
  /// Interaction logic for UpdateInfoFlyout.xaml
  /// </summary>
  public partial class UpdateInfoFlyout : Flyout
  {
    public UpdateInfoFlyout()
    {
      this.InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      var ui = this.DataContext as UpdateInfoViewModel;
      if (ui != null)
      {
        Process.Start(ui.UpdateInfo.DownLoadURL);
      }
    }
  }
}