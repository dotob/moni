using System;
using System.Reflection;
using System.Windows;
using MONI.Util;
using MahApps.Metro;

namespace MONI
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e) {
      ThemeManager.MainResourceDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/MONI;component/Resources/Controls.xaml") });
      AppHelper.Instance.ConfigureApp(this, Assembly.GetExecutingAssembly().GetName().Name);
      base.OnStartup(e);
    }
  }
}