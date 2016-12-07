using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MONI.Util;
using MONI.ViewModels;

namespace MONI.Views
{
    /// <summary>
    /// Interaction logic for PNSearchFlyout.xaml
    /// </summary>
    public partial class PNSearchFlyout : Flyout
    {
        public static readonly DependencyProperty AddShortCutServiceProperty =
            DependencyProperty.Register("AddShortCutService", typeof(IAddShortcutService), typeof(PNSearchFlyout));

        public IAddShortcutService AddShortCutService
        {
            get { return (IAddShortcutService)this.GetValue(AddShortCutServiceProperty); }
            set { this.SetValue(AddShortCutServiceProperty, value); }
        }

        public PNSearchFlyout()
        {
            this.InitializeComponent();
        }

        private void AddAsShortcut_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ProjectNumber pn = button.Tag as ProjectNumber;
                if (pn != null && this.AddShortCutService != null)
                {
                    this.AddShortCutService.AddShortCut(string.Empty, string.Format("{0}-000({1})", pn.Number, pn.Description));
                }
            }
        }
    }
}