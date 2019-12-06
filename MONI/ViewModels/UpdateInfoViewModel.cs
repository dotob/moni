using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using MONI.Data;
using MONI.Util;
using NLog;
using Newtonsoft.Json;
using System.Linq;
using System.Windows;
using AsyncAwaitBestPractices;

namespace MONI.ViewModels
{
    public class UpdateInfoViewModel : ViewModelBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string updateInfoURL;
        private readonly Version currentVersion;
        private readonly int entryCount;
        private readonly DateTime firstEntryDate;
        private bool showUpdateInfo;
        private UpdateInfo updateInfo;

        public UpdateInfoViewModel(string updateInfoURL, Version currentVersion, int entryCount, DateTime firstEntryDate)
        {
            this.updateInfoURL = updateInfoURL;
            this.currentVersion = currentVersion;
            this.entryCount = entryCount;
            this.firstEntryDate = firstEntryDate;

            this.CancelCommand = new DelegateCommand(() => Application.Current.MainWindow?.Close(), () => true);

            this.CheckVersionsAsync().SafeFireAndForget(onException: exception => { logger.Error(exception, "Error while checking for a newer version."); });
        }

        private async Task CheckVersionsAsync()
        {
            var updates = await this.GetUpdateInfosAsync();
            var newerVersion = updates.FirstOrDefault(u => u.VersionAsVersion() != null && u.VersionAsVersion().CompareTo(currentVersion) > 0);
            if (newerVersion != null)
            {
                logger.Info("Found newer version {0} under {1}", newerVersion, this.updateInfoURL);
                this.UpdateInfo = newerVersion;
                this.ShowUpdateInfo = true;
            }
        }

        private async Task<IEnumerable<UpdateInfo>> GetUpdateInfosAsync()
        {
            try
            {
                var url = this.updateInfoURL;
                try
                {
                    url = $"{this.updateInfoURL}?user={Environment.UserName}&machine={Environment.MachineName}&framework={Environment.Version}&entrycount={entryCount}&firstentrydate={firstEntryDate:s}";
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error while collecting enviroiniment info");
                }

                using (var webClient = new WebClient())
                {
                    var uisJson = await webClient.DownloadStringTaskAsync(new Uri(url, UriKind.RelativeOrAbsolute));
                    var updates = await Task.Run(() => JsonConvert.DeserializeObject<IEnumerable<UpdateInfo>>(uisJson));
                    return updates;
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Exception while downloading updateinfo from {0} an exception occured", updateInfoURL);
            }

            return await Task.FromResult(Enumerable.Empty<UpdateInfo>());
        }

        public bool ShowUpdateInfo
        {
            get { return this.showUpdateInfo; }
            set { this.Set(ref this.showUpdateInfo, value); }
        }

        public UpdateInfo UpdateInfo
        {
            get { return this.updateInfo; }
            set { this.Set(ref this.updateInfo, value); }
        }

        public ICommand CancelCommand { get; }
    }
}