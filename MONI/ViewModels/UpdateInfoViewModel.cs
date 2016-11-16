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
        private ICommand cancelCommand;

        public UpdateInfoViewModel(string updateInfoURL, Version currentVersion, int entryCount, DateTime firstEntryDate)
        {
            this.updateInfoURL = updateInfoURL;
            this.currentVersion = currentVersion;
            this.entryCount = entryCount;
            this.firstEntryDate = firstEntryDate;

            Task<IEnumerable<UpdateInfo>>.Factory.StartNew(this.GetUpdateInfos, TaskCreationOptions.LongRunning).ContinueWith(x => this.CheckVersions(x.Result), TaskScheduler.Default);
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
            logger.Debug("found newer version {0} under {1}", ui, this.updateInfoURL);
            this.UpdateInfo = ui;
            this.ShowUpdateInfo = true;
        }

        private IEnumerable<UpdateInfo> GetUpdateInfos()
        {
            try
            {
                WebClient wc = new WebClient();
                var url = this.updateInfoURL;
                try
                {
                    url = string.Format("{0}?user={1}&machine={2}&framework={3}&entrycount={4}&firstentrydate={5:s}", this.updateInfoURL, Environment.UserName, Environment.MachineName, Environment.Version, entryCount, firstEntryDate);
                }
                catch (Exception e)
                {
                    logger.Error(e, "error while collecting enviroiniment info");
                }
                var uisJson = wc.DownloadString(url);
                UpdateInfo[] uis = JsonConvert.DeserializeObject<UpdateInfo[]>(uisJson);
                return uis;
            }
            catch (Exception exception)
            {
                logger.Error(exception, "while downloading updateinfo from {0} an exception occured", updateInfoURL);
            }
            return Enumerable.Empty<UpdateInfo>();
        }

        public bool ShowUpdateInfo
        {
            get { return this.showUpdateInfo; }
            set
            {
                this.showUpdateInfo = value;
                this.OnPropertyChanged(() => this.ShowUpdateInfo);
            }
        }

        public UpdateInfo UpdateInfo
        {
            get { return this.updateInfo; }
            set
            {
                this.updateInfo = value;
                this.OnPropertyChanged(() => this.UpdateInfo);
            }
        }

        public ICommand CancelCommand
        {
            get { return this.cancelCommand ?? (this.cancelCommand = new DelegateCommand(() => this.ShowUpdateInfo = false, () => true)); }
        }
    }
}