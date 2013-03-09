using System;
using System.Windows;
using System.Windows.Controls;

namespace MONI.Views
{
  /// <summary>
  /// Interaction logic for ShortcutView.xaml
  /// </summary>
  public partial class ShortcutView : UserControl
  {
    public static readonly DependencyProperty IsOpenProperty =
      DependencyProperty.Register("IsOpen", typeof(bool), typeof(ShortcutView), new PropertyMetadata(default(bool), IsOpenPropertyChangedCallback));

    private static void IsOpenPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
      var view = dependencyObject as ShortcutView;
      if (view != null && e.NewValue != e.OldValue && (bool)e.NewValue) {
        if (view.shortcutTextBox.Focusable) {
          view.shortcutTextBox.Focus();
        }
      }
    }

    public bool IsOpen {
      get { return (bool)this.GetValue(IsOpenProperty); }
      set { this.SetValue(IsOpenProperty, value); }
    }

    public ShortcutView() {
      this.InitializeComponent();
    }
  }
}