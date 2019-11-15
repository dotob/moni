using ControlzEx.Standard;
using MahApps.Metro.Controls;

namespace MONI.Data
{
    public class CustomWindowPlacementSettings : IWindowPlacementSettings
    {
        private readonly MoniSettings moniSettings;

        public CustomWindowPlacementSettings(MoniSettings settings)
        {
            this.moniSettings = settings;
        }

        public WINDOWPLACEMENT Placement { get; set; }

        public bool UpgradeSettings { get; set; }

        public void Reload()
        {
            this.Placement = this.moniSettings.MainSettings.Placement;
        }

        public void Upgrade()
        {
        }

        public void Save()
        {
            this.moniSettings.MainSettings.Placement = this.Placement;
        }
    }
}