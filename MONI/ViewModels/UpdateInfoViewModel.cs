using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
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
    private bool showUpdateInfo;
    private UpdateInfo updateInfo;
    private ICommand cancelCommand;

    public UpdateInfoViewModel(string updateInfoURL, Version currentVersion) {
      this.updateInfoURL = updateInfoURL;
      this.currentVersion = currentVersion;

      Task<IEnumerable<UpdateInfo>>.Factory.StartNew(this.GetUpdateInfos, TaskCreationOptions.LongRunning).ContinueWith(x => this.CheckVersions(x.Result),TaskScheduler.Default);
    }

    private void CheckVersions(IEnumerable<UpdateInfo> uis) {
      foreach (var ui in uis) {
        var versionAsVersion = ui.VersionAsVersion();
        if (versionAsVersion != null && versionAsVersion.CompareTo(currentVersion) > 0) {
          this.FoundNewerVersion(ui);
        }
      }
    }

    private void FoundNewerVersion(UpdateInfo ui) {
      logger.Debug("found newer version {0} under {1}", ui, this.updateInfoURL);
      this.UpdateInfo = ui;
      this.ShowUpdateInfo = true;
    }

    private IEnumerable<UpdateInfo> GetUpdateInfos() {
      try {
        WebClient wc = new WebClient();
        var uisJson = wc.DownloadString(this.updateInfoURL);
        UpdateInfo[] uis = JsonConvert.DeserializeObject<UpdateInfo[]>(uisJson);
        return uis;
      }
      catch (Exception exception) {
        logger.ErrorException(string.Format("while downloading updateinfo from {0} an exception occured", updateInfoURL), exception);
      }
      return Enumerable.Empty<UpdateInfo>();
    }

    public bool ShowUpdateInfo {
      get { return this.showUpdateInfo; }
      set {
        this.showUpdateInfo = value;
        this.OnPropertyChanged(() => this.ShowUpdateInfo);
      }
    }

    public UpdateInfo UpdateInfo {
      get { return this.updateInfo; }
      set {
        this.updateInfo = value;
        this.OnPropertyChanged(() => this.UpdateInfo);
      }
    }

    public ICommand CancelCommand {
      get { return this.cancelCommand ?? (this.cancelCommand = new DelegateCommand(() => this.ShowUpdateInfo = false, () => true)); }
    }
  }
}