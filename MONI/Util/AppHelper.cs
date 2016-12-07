using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Linq;

namespace MONI.Util
{
    public sealed class AppHelper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private Timer gcTimer;
        private Timer gcTimerEnv;
        private static readonly object lockErrorDialog = 1;
        private static bool blockExceptionDialog;

        private static readonly AppHelper instance = new AppHelper();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AppHelper()
        {
        }

        private AppHelper()
        {
        }

        public static AppHelper Instance
        {
            get { return instance; }
        }

        public void SetCultureToCurrentThread(string cultureString)
        {
            if (!String.IsNullOrEmpty(cultureString))
            {
                var ci = CultureInfo.GetCultureInfo(cultureString);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;

                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
        }

        private string ApplicationName { get; set; }

        private DateTime AppStartedUtcTime { get; set; }

        private void LogMemoryUsageAndInfos(object state)
        {
            var workingSetInMiB = Environment.WorkingSet / 1024 / 1024;
            var gcTotalMemoryInMiB = GC.GetTotalMemory(true) / 1024 / 1024;
            var uptime = DateTime.UtcNow - this.AppStartedUtcTime;
            logger.Info("{0} Memory-Usage (GC.GetTotalMemory(true)/Environment.WorkingSet): {1,4}/{2,4} MiB of instance {3} (uptime: {4} ({5:0.#####} minutes))", this.ApplicationName, workingSetInMiB, gcTotalMemoryInMiB, this.AppStartedUtcTime, TimeSpanUtil.ConvertMinutes2String(uptime.TotalMinutes), uptime.TotalMinutes);
        }

        private void ConfigureHandlingUnhandledExceptions()
        {
            // see more: http://dotnet.dzone.com/news/order-chaos-handling-unhandled
            Thread.GetDomain().UnhandledException += this.BackgroundThreadUnhandledException;
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.DispatcherUnhandledException += this.WPFUIThreadException;
            }
        }

        private void BackgroundThreadUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Debug("BackgroundThreadUnhandledException");
            HandleUnhandledException(e.ExceptionObject, true);
        }

        private void WPFUIThreadException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true; // prevent app to quit automatically, see: http://dotnet.dzone.com/news/order-chaos-handling-unhandled
            logger.Debug("WPFUIThreadException");
            HandleUnhandledException(e.Exception, false);
        }

        public static void HandleUnhandledException(object exception, bool exitProgram)
        {
            // only allow one thread (UI)
            lock (lockErrorDialog)
            {
                try
                {
                    // check for special exitprogram conditions
                    if (!exitProgram)
                    {
                        exitProgram = ExceptionExtensions.IsFatalException((Exception)exception);
                    }
                    // prevent recursion on the message loop
                    if (!blockExceptionDialog)
                    {
                        blockExceptionDialog = true;
                        // only log once. Otherwise it is poosible to file the file system with exceptions (message pump + exception)
                        logger.Fatal("UnhandledException: {0}\n\n\nAt Stack: {1}", exception, Environment.StackTrace);
                        try
                        {
                            // we do not switch to the UI thread, because the dialog that we are going to show has its own message pump (all dialogs have). 
                            // As long as the dialog does not call methods of other windows there should be no problem.
                            if (exception == null)
                            {
                                //TS_StackTraceBox.ShowStackTraceBox("Unhandled Exception Occurred", Environment.StackTrace);
                            }
                            else if (exception is Exception)
                            {
                                //TS_ExceptionBox.ShowErrorBox((Exception)exception);
                            }
                            else
                            {
                                // won't happen really - exception is really always of type Exception
                                //TS_StackTraceBox.ShowStackTraceBox("Unhandled Exception Occurred: " + exception, Environment.StackTrace);
                            }
                        }
                        finally
                        {
                            if (!exitProgram)
                            {
                                blockExceptionDialog = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "Unable to Handle UnhandledException");
                }
                finally
                {
                    if (exitProgram)
                    {
                        logger.Warn("Application exits due to UnhandledException");
                        Environment.Exit(0);
                    }
                }
            }
        }

        public void ConfigureApp(Application app, string appName)
        {
            // really need this?
            this.SetCultureToCurrentThread("de-DE");

            this.AppStartedUtcTime = DateTime.UtcNow;
            this.ApplicationName = appName;

            this.ConfigureLogging();

            logger.Info("========================== {0} started ==========================", this.ApplicationName);
            app.Exit += (sender, args) => logger.Info("========================== {0} stopped ==========================", this.ApplicationName);

            // setup exception handling
            this.ConfigureHandlingUnhandledExceptions();

            // configure timer (log all 10min == 600000ms)
            this.gcTimer = new Timer(this.LogMemoryUsageAndInfos, this.AppStartedUtcTime, 0, 30000); //600000);
            this.gcTimerEnv = new Timer(o => logger.Info(EnvironmentInfos.Instance.PrettyPrintInfos(this.AppStartedUtcTime)), this.AppStartedUtcTime, 0, 600000); //600000);

            this.CheckForRunningMoni(app);
        }

        private void CheckForRunningMoni(Application app)
        {
            var myName = Process.GetCurrentProcess().ProcessName;
            var sameNameProcesses = Process.GetProcessesByName(myName);
            if (sameNameProcesses.Length > 1)
            {
                MessageBox.Show("MONI läuft schon. Mit zwei laufenden MONIs haste nur Ärger...Tschö", "Problem");
                app.Shutdown();
            }
        }

        private void ConfigureLogging()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 

#if DEBUG
            var consoleTarget = new ColoredConsoleTarget();
            consoleTarget.Layout = "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=Message,Type,StackTrace:separator=//}";
            config.AddTarget("console", consoleTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, consoleTarget));

            // check where we can write
            if (Utils.CanCreateFile("."))
            {
                fileTarget.FileName = "${basedir}/logs/" + this.ApplicationName + ".${shortdate}.log";
            }
            else
            {
                fileTarget.FileName = Utils.MoniAppDataPath() + "/logs/" + this.ApplicationName + ".${shortdate}.log";
            }
#else
            fileTarget.FileName = Utils.MoniAppDataPath() + "/logs/" + this.ApplicationName + ".${shortdate}.log";
#endif
            fileTarget.KeepFileOpen = false;
            fileTarget.CreateDirs = true;
            fileTarget.ConcurrentWrites = true;
            fileTarget.Layout = "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=Message,Type,StackTrace:separator=//}";

            // Step 4. Define rules
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }
    }
}