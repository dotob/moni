using System.Reflection;
using System.Windows;
using MONI.Util;

namespace MONI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppHelper.Instance.ConfigureApp(this, Assembly.GetExecutingAssembly().GetName().Name);
            base.OnStartup(e);
        }
    }
}