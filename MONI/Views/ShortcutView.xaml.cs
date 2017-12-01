using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MONI.ViewModels;

namespace MONI.Views
{
    /// <summary>
    /// Interaction logic for ShortcutView.xaml
    /// </summary>
    public partial class ShortcutView : UserControl
    {
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(ShortcutView), new PropertyMetadata(default(bool), IsOpenPropertyChangedCallback));

        private static void IsOpenPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var view = dependencyObject as ShortcutView;
            if (view != null && e.NewValue != e.OldValue && (bool)e.NewValue)
            {
                var vm = view.DataContext as ShortcutViewModel;

                // need to refresh list binding, because plain list does not fire CollectionChanged-events
                RefreshShortcutGroupsList(view);
                if (vm != null && vm.IsNew && view.shortcutTextBox.Focusable)
                {
                    view.shortcutTextBox.Focus();
                }
                else if (view.expansionTextBox.Focusable)
                {
                    view.expansionTextBox.Focus();
                }
            }
        }

        private static void RefreshShortcutGroupsList(ShortcutView view)
        {
            // need to refresh list binding, because plain list does not fire CollectionChanged-events
            var shortcutsListSource = view.layoutRoot?.TryFindResource("ShortCutGroupCVS") as CollectionViewSource;
            shortcutsListSource?.View?.Refresh();
        }

        public bool IsOpen
        {
            get { return (bool)this.GetValue(IsOpenProperty); }
            set { this.SetValue(IsOpenProperty, value); }
        }

        public ShortcutView()
        {
            this.InitializeComponent();
        }
    }
}