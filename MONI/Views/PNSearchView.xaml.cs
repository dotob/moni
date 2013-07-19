using System.Windows;
using System.Windows.Controls;
using MONI.Util;
using MONI.ViewModels;

namespace MONI.Views
{
  /// <summary>
  /// Interaction logic for ShortcutView.xaml
  /// </summary>
  public partial class PNSearchView : UserControl
  {
    public static readonly DependencyProperty IsOpenProperty =
      DependencyProperty.Register("IsOpen", typeof(bool), typeof(PNSearchView), new PropertyMetadata(default(bool), IsOpenPropertyChangedCallback));
    public static readonly DependencyProperty AddShortCutServiceProperty =
      DependencyProperty.Register("AddShortCutService", typeof(IAddShortcutService), typeof(PNSearchView));

    private static void IsOpenPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
      var view = dependencyObject as PNSearchView;
      if (view != null && e.NewValue != e.OldValue && (bool)e.NewValue && view.searchTextBox.Focusable) {
        view.searchTextBox.Focus();
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

    public PNSearchView() {
      this.InitializeComponent();
    }

    private void AddAsShortcut_OnClick(object sender, RoutedEventArgs e) {
      Button button = sender as Button;
      if (button != null) {
        ProjectNumber pn = button.Tag as ProjectNumber;
        if (pn != null && this.AddShortCutService!=null) {
          this.AddShortCutService.AddShortCut(string.Empty, string.Format("{0}-000({1})", pn.Number, pn.Description));
        }
      }
    }
  }
}