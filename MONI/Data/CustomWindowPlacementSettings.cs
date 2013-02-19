using MahApps.Metro.Controls;
using MahApps.Metro.Native;

namespace MONI.Data
{
  public class CustomWindowPlacementSettings : IWindowPlacementSettings
  {
    private readonly MoniSettings moniSettings;

    public CustomWindowPlacementSettings(MoniSettings settings) {
      this.moniSettings = settings;
    }

    public WINDOWPLACEMENT? Placement { get; set; }

    public void Reload() {
      this.Placement = this.moniSettings.MainSettings.Placement;
    }

    public void Save() {
      this.moniSettings.MainSettings.Placement = this.Placement;
    }
  }
}