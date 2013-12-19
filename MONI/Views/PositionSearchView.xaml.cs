using System.Windows;
using System.Windows.Controls;
using MONI.Util;
using MONI.ViewModels;

namespace MONI.Views
{
  /// <summary>
  /// Interaction logic for ShortcutView.xaml
  /// </summary>
  public partial class PositionSearchView : UserControl
  {
    public static readonly DependencyProperty IsOpenProperty =
      DependencyProperty.Register("IsOpen", typeof(bool), typeof(PositionSearchView), new PropertyMetadata(default(bool), IsOpenPropertyChangedCallback));
    public static readonly DependencyProperty AddShortCutServiceProperty =
      DependencyProperty.Register("AddShortCutService", typeof(IAddShortcutService), typeof(PositionSearchView));

    private static void IsOpenPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
      var view = dependencyObject as PositionSearchView;
      if (view != null && e.NewValue != e.OldValue && (bool)e.NewValue && view.posNumSearchTextBox.Focusable) {
        view.posNumSearchTextBox.Focus();
      }
    }

    public bool IsOpen {
      get { return (bool)this.GetValue(IsOpenProperty); }
      set { this.SetValue(IsOpenProperty, value); }
    }

    public IAddShortcutService AddShortCutService {
      get { return (IAddShortcutService)this.GetValue(AddShortCutServiceProperty); }
      set { this.SetValue(AddShortCutServiceProperty, value); }
    }

    public PositionSearchView() {
      this.InitializeComponent();
    }

    private void AddAsShortcut_OnClick(object sender, RoutedEventArgs e) {
      Button button = sender as Button;
      if (button != null) {
        PositionNumber pn = button.Tag as PositionNumber;
        if (pn != null && this.AddShortCutService!=null) {
          this.AddShortCutService.AddShortCut(string.Empty, string.Format("000-{0}({1}:{2})", pn.Number, pn.Customer, pn.Description));
        }
      }
    }
  }
}