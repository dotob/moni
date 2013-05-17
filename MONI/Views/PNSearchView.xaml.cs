using System.Windows;
using System.Windows.Controls;
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

    public PNSearchView() {
      this.InitializeComponent();
    }
  }
}