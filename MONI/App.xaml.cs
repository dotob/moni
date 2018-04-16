using System.Reflection;
using System.Windows;
using MONI.Util;
using MONI.Views;

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

            // Check command line arguments for automatic upload
            var arguments = e.Args;
            var windowMode = true;
            string password = "";
            foreach (var arg in e.Args)
            {
                var values = arg.Split('=');
                switch(values[0])
                {
                    case "windowmode":
                      if (values[1] == "false")
                      {
                          windowMode = false;
                      }
                      break;

                    case "password":
                      password = values[1];
                      break;
                }
            }
            if (!windowMode)
            {
                var MV = new MainView();
                MV.uploadWrapper(password);
                MV.Close();
            }

            base.OnStartup(e);
        }
    }
}