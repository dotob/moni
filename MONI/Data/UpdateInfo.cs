using System;

namespace MONI.Data
{
  public class UpdateInfo
  {
    private Version versionVersion;
    public string Version { get; set; }
    public string[] Changes { get; set; }
    public string DownLoadURL { get; set; }

    public Version VersionAsVersion() {
      if (versionVersion == null && !string.IsNullOrWhiteSpace(Version)) {
        if (!System.Version.TryParse(Version, out versionVersion)) {
          versionVersion = null;
        }
      }
      return versionVersion;
    }

    public override string ToString() {
      return Version;
    }
  }
}