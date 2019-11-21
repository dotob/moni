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
using System.Windows.Threading;

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

        public UpdateInfoViewModel(Dispatcher dispatcher, string updateInfoURL, Version currentVersion, int entryCount, DateTime firstEntryDate)
        {
            this.updateInfoURL = updateInfoURL;
            this.currentVersion = currentVersion;
            this.entryCount = entryCount;
            this.firstEntryDate = firstEntryDate;

            this.CancelCommand = new DelegateCommand(() => this.ShowUpdateInfo = false, () => true);

            dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(async () =>
            {
                var updates = await this.GetUpdateInfosAsync().ConfigureAwait(false);
                this.CheckVersions(updates);
            }));
        }

        private void CheckVersions(IEnumerable<UpdateInfo> uis)
        {
            foreach (var ui in uis)
            {
                var versionAsVersion = ui.VersionAsVersion();
                if (versionAsVersion != null && versionAsVersion.CompareTo(currentVersion) > 0)
                {
                    this.FoundNewerVersion(ui);
                }
            }
        }

        private void FoundNewerVersion(UpdateInfo ui)
        {
            logger.Info("found newer version {0} under {1}", ui, this.updateInfoURL);
            this.UpdateInfo = ui;
            this.ShowUpdateInfo = true;
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
                    var uisJson = await webClient.DownloadStringTaskAsync(new Uri(url, UriKind.RelativeOrAbsolute)).ConfigureAwait(false);
                    var updates = await Task.Run(() => JsonConvert.DeserializeObject<IEnumerable<UpdateInfo>>(uisJson)).ConfigureAwait(false);
                    return updates;
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Exception while downloading updateinfo from {0} an exception occured", updateInfoURL);
            }

            return Enumerable.Empty<UpdateInfo>();
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