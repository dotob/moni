using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MONI.Util;
using MONI.ViewModels;

namespace MONI.Views
{
    /// <summary>
    /// Interaction logic for PositionSearchFlyout.xaml
    /// </summary>
    public partial class PositionSearchFlyout : Flyout
    {
        public static readonly DependencyProperty AddShortCutServiceProperty =
            DependencyProperty.Register("AddShortCutService", typeof(IAddShortcutService), typeof(PositionSearchFlyout));

        public IAddShortcutService AddShortCutService
        {
            get { return (IAddShortcutService)this.GetValue(AddShortCutServiceProperty); }
            set { this.SetValue(AddShortCutServiceProperty, value); }
        }

        public PositionSearchFlyout()
        {
            this.InitializeComponent();
        }

        private void AddAsShortcut_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                PositionNumber pn = button.Tag as PositionNumber;
                if (pn != null && this.AddShortCutService != null)
                {
                    this.AddShortCutService.AddShortCut(string.Empty, string.Format("000-{0}({1}:{2})", pn.Number, pn.Customer, pn.Description));
                }
            }
        }
    }
}