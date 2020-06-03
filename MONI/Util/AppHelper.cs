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

namespace MONI.Util
{
    public sealed class AppHelper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private DispatcherTimer gcTimer;
        private DispatcherTimer gcTimerEnv;
        private static readonly object lockErrorDialog = 1;
        private static bool blockExceptionDialog;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AppHelper()
        {
        }

        private AppHelper()
        {
        }

        public static AppHelper Instance { get; } = new AppHelper();

        public void SetCultureToCurrentThread(string cultureString)
        {
            var ci = Thread.CurrentThread.CurrentCulture;
            if (!string.IsNullOrEmpty(cultureString))
            {
                ci = CultureInfo.GetCultureInfo(cultureString);
                logger.Info($"{this.ApplicationName} set CurrentCulture to {ci}");
            }
            else
            {
                logger.Info($"{this.ApplicationName} use CurrentCulture {ci}");
            }

            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(ci.IetfLanguageTag)));
        }

        private string ApplicationName { get; set; }

        private DateTime AppStartedUtcTime { get; set; }

        private void ConfigureHandlingUnhandledExceptions()
        {
            // see more: http://dotnet.dzone.com/news/order-chaos-handling-unhandled
            Thread.GetDomain().UnhandledException += this.BackgroundThreadUnhandledException;
            if (Application.Current != null)
            {
                Application.Current.DispatcherUnhandledException += this.WPFUIThreadException;
                Application.Current.Exit += this.OnApplicationExit;
            }
        }

        private void OnApplicationExit(object sender, ExitEventArgs args)
        {
            // Not really fatal, but that way it is easy to find in the logs.
            logger.Info($"{this.ApplicationName} exits with exit-code {args.ApplicationExitCode} after uptime {TimeSpanUtil.UptimeString(this.AppStartedUtcTime)}");
            logger.Fatal("========================================:: {0} stopped", this.ApplicationName);
            LogManager.Shutdown();
        }

        private void BackgroundThreadUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error(nameof(BackgroundThreadUnhandledException));
            HandleUnhandledException(e.ExceptionObject, true);
        }

        private void WPFUIThreadException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true; // prevent app to quit automatically, see: http://dotnet.dzone.com/news/order-chaos-handling-unhandled

            logger.Error(nameof(WPFUIThreadException));
            HandleUnhandledException(e.Exception, false);
        }

        public static void HandleUnhandledException(object exception, bool exitProgram)
        {
            if (Debugger.IsAttached)
            {
                // When debugging or running unit tests, let the unhandled exception surface.
                return;
            }

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

                        try
                        {
                            // only log once. Otherwise it is possible to fill the file system with exceptions (message pump + exception)
                            LogFatalException("UnhandledException", exception as Exception);
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
                catch (Exception ex)
                {
                    LogFatalException("Unable to handle UnhandledException", ex);
                }
                finally
                {
                    if (exitProgram)
                    {
                        logger.Fatal($"Application exits due to UnhandledException, after uptime:{TimeSpanUtil.UptimeString(AppHelper.Instance.AppStartedUtcTime)}");

                        Environment.Exit(0);
                    }
                }
            }
        }

        private static void LogFatalException(string message, Exception ex)
        {
            if (ex is InvalidOperationException invalidOpEx)
            {
                logger.Fatal(invalidOpEx, message);
                logger.Fatal($"{message} details: {invalidOpEx}");
                if (invalidOpEx.InnerException != null)
                {
                    logger.Fatal($"{message} InnerException: {invalidOpEx.InnerException}");
                }
            }
            else
            {
                logger.Fatal($"{message} {Environment.NewLine} {ex} {Environment.NewLine} At Stack {Environment.NewLine} {Environment.StackTrace}");
            }
        }

        public void ConfigureApp(Application app, string appName)
        {
            // really need this?
            this.SetCultureToCurrentThread("de-DE");

            this.AppStartedUtcTime = DateTime.UtcNow;
            this.ApplicationName = appName;

            this.ConfigureLogging();

            logger.Fatal("========================================:: {0} started", this.ApplicationName);

            // setup exception handling
            this.ConfigureHandlingUnhandledExceptions();

            logger.Info(EnvironmentInfos.Instance.PrettyPrintInfos());
            logger.Info(EnvironmentInfos.Instance.MemoryUsage(this.ApplicationName, this.AppStartedUtcTime));

            // configure timer (log all 10min == 600000ms)
            this.gcTimer = new DispatcherTimer(TimeSpan.FromSeconds(30),
                                               DispatcherPriority.Background,
                                               (sender, eventArgs) => logger.Info(EnvironmentInfos.Instance.MemoryUsage(this.ApplicationName, this.AppStartedUtcTime)),
                                               Dispatcher.CurrentDispatcher);
            this.gcTimer.Start();

            this.gcTimerEnv = new DispatcherTimer(TimeSpan.FromSeconds(60),
                                                  DispatcherPriority.Background,
                                                  (sender, eventArgs) => logger.Info(EnvironmentInfos.Instance.PrettyPrintInfos()),
                                                  Dispatcher.CurrentDispatcher);
            this.gcTimerEnv.Start();

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