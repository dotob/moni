using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Media;
using Microsoft.Win32;
using NLog;

namespace MONI.Util
{
  public class EnvironmentInfos
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private string assemblyCompany;
    private string assemblyCopyright;
    private string assemblyDescription;
    private string assemblyProduct;
    private string assemblyTitle;
    private string assemblyVersion;
    private string highestFrameworkVersion;
    private string systemBitInfo;
    private string version;
    private string ipAdresses;

    private static readonly EnvironmentInfos instance = new EnvironmentInfos();

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static EnvironmentInfos() {
    }

    private EnvironmentInfos() {
    }

    public static EnvironmentInfos Instance {
      get { return instance; }
    }

    public Assembly AssemblyToUse {
      get { return Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(); }
    }

    public string Version {
      get { return this.version ?? this.CollectVersion(); }
    }

    public string AssemblyTitle {
      get { return this.assemblyTitle ?? this.CollectAssemblyTitle(); }
    }

    public string AssemblyVersion {
      get { return this.version ?? this.CollectAssemblyVersion(); }
    }

    public string AssemblyDescription {
      get { return this.assemblyDescription ?? this.CollectAssemblyDescription(); }
    }

    public string AssemblyProduct {
      get { return this.assemblyProduct ?? this.CollectAssemblyProduct(); }
    }

    public string AssemblyCopyright {
      get { return this.assemblyCopyright ?? this.CollectAssemblyCopyright(); }
    }

    public string AssemblyCompany {
      get { return this.assemblyCompany ?? this.CollectAssemblyCompany(); }
    }

    public string HighestFrameworkVersion {
      get { return this.highestFrameworkVersion ?? this.CollectHighestFrameWorkVersion(); }
    }

    // RenderCapabilityTier may change during runtime, so don't cache, always collect
    public string RenderCapabilityTier {
      get { return this.CollectRenderCapabilityTier(); }
    }

    public string SystemBitInfo {
      get { return this.systemBitInfo ?? this.CollectSystemBitInfo(); }
    }

    public string OsVersion {
      get {
        var vs = "n/a";
        try {
          vs = Environment.OSVersion.VersionString;
        } catch (Exception ex) {
          logger.Error("exception occured while retrieving Environment.OSVersion.VersionString: {0}", ex);
        }
        return vs;
      }
    }

    public string OsPlatform {
      get {
        var vs = "n/a";
        try {
          vs = Environment.OSVersion.Platform.ToString();
        } catch (Exception ex) {
          logger.Error("exception occured while retrieving Environment.OSVersion.Platform: {0}", ex);
        }
        return vs;
      }
    }

    public string OsServicePack {
      get {
        var vs = "n/a";
        try {
          vs = Environment.OSVersion.ServicePack;
        } catch (Exception ex) {
          logger.Error("exception occured while retrieving Environment.OSVersion.ServicePack: {0}", ex);
        }
        return vs;
      }
    }

    public string ProcessorCount {
      get {
        var vs = "n/a";
        try {
          vs = Environment.ProcessorCount.ToString();
        } catch (Exception ex) {
          logger.Error("exception occured while retrieving Environment.ProcessorCount: {0}", ex);
        }
        return vs;
      }
    }

    public string MachineName {
      get {
        var vs = "n/a";
        try {
          vs = Environment.MachineName;
        } catch (Exception ex) {
          logger.Error("exception occured while retrieving Environment.MachineName: {0}", ex);
        }
        return vs;
      }
    }

    public string FrameworkVersion {
      get {
        var vs = "n/a";
        try {
          vs = Environment.Version.ToString();
        } catch (Exception ex) {
          logger.Error("exception occured while retrieving Environment.Version: {0}", ex);
        }
        return vs;
      }
    }

    public string UserName {
      get {
        var vs = "n/a";
        try {
          vs = Environment.UserName;
        } catch (Exception ex) {
          logger.Error("exception occured while retrieving Environment.UserName: {0}", ex);
        }
        return vs;
      }
    }

    public string ProcessId {
      get { return System.Diagnostics.Process.GetCurrentProcess().Id.ToString(); }
    }

    public string StartTime {
      get { return System.Diagnostics.Process.GetCurrentProcess().StartTime.ToString("g"); }
    }

    public string MemoryUsage {
      get { return System.Diagnostics.Process.GetCurrentProcess().WorkingSet64.ToString(); }
    }

    public string IPAdresses {
      get {
        if (this.ipAdresses == null) {
          var addressList = Dns.GetHostAddresses(Dns.GetHostName());
          this.ipAdresses = string.Join(",", addressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Select(ip => ip.ToString()).ToArray());
        }
        return this.ipAdresses;
      }
    }

    public string CommandLine {
      get {
        var vs = "n/a";
        try {
          vs = Environment.CommandLine;
        } catch (Exception ex) {
          logger.Error("exception occured while retrieving Environment.CommandLine: {0}", ex);
        }
        return vs;
      }
    }

    private string CollectAssemblyVersion() {
      this.assemblyVersion = this.AssemblyToUse.GetName().Version.ToString();

      return this.assemblyVersion;
    }

    private string CollectVersion() {
      this.version = this.AssemblyToUse.GetName().Version.ToString();
      return this.version;
    }

    private string CollectAssemblyTitle() {
      // Get all Title attributes on this assembly
      var attributes = this.AssemblyToUse.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
      // If there is at least one Title attribute
      if (attributes.Length > 0) {
        // Select the first one
        var titleAttribute = (AssemblyTitleAttribute)attributes[0];
        // If it is not an empty string, return it
        if (titleAttribute.Title != string.Empty) {
          this.assemblyTitle = titleAttribute.Title;
        }
      }
      // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
      this.assemblyTitle = Path.GetFileNameWithoutExtension(this.AssemblyToUse.CodeBase);

      return this.assemblyTitle;
    }

    private string CollectAssemblyDescription() {
      // Get all Description attributes on this assembly
      var attributes = this.AssemblyToUse.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
      // If there aren't any Description attributes, return an empty string
      if (attributes.Length == 0) {
        this.assemblyDescription = string.Empty;
        logger.Warn("unable to detect assembly description");
      }
      // If there is a Description attribute, return its value
      this.assemblyDescription = ((AssemblyDescriptionAttribute)attributes[0]).Description;

      return this.assemblyDescription;
    }

    private string CollectAssemblyProduct() {
      // Get all Product attributes on this assembly
      var attributes = this.AssemblyToUse.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
      // If there aren't any Product attributes, return an empty string
      if (attributes.Length == 0) {
        this.assemblyProduct = string.Empty;
        logger.Warn("unable to detect assembly product");
      }
      // If there is a Product attribute, return its value
      this.assemblyProduct = ((AssemblyProductAttribute)attributes[0]).Product;

      return this.assemblyProduct;
    }

    private string CollectAssemblyCopyright() {
      // Get all Copyright attributes on this assembly
      var attributes = this.AssemblyToUse.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
      // If there aren't any Copyright attributes, return an empty string
      if (attributes.Length == 0) {
        this.assemblyCopyright = string.Empty;
        logger.Warn("unable to detect assembly copyright");
      }
      // If there is a Copyright attribute, return its value
      this.assemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

      return this.assemblyCopyright;
    }

    private string CollectAssemblyCompany() {
      // Get all Company attributes on this assembly
      var attributes = this.AssemblyToUse.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
      // If there aren't any Company attributes, return an empty string
      if (attributes.Length == 0) {
        this.assemblyCompany = string.Empty;
        logger.Warn("unable to detect assembly company");
      }
      // If there is a Company attribute, return its value
      this.assemblyCompany = ((AssemblyCompanyAttribute)attributes[0]).Company;

      return this.assemblyCompany;
    }

    public string CollectHighestFrameWorkVersion() {
      var returnString = String.Empty; //vx.y.z

      this.highestFrameworkVersion = String.Empty;

      try {
        const string subkey = "Software\\Microsoft\\NET Framework Setup\\NDP\\";
        var rk = Registry.LocalMachine;
        var rksub = rk.OpenSubKey(subkey, true);
        if (rksub != null) {
          double[] dversion = {0.0, 0.0, 0.0};
          double[] dvercomp = {0.0, 0.0, 0.0};
          foreach (var valueName in rksub.GetSubKeyNames()) {
            var keySp = rksub.OpenSubKey(valueName);
            int intSp;
            if (keySp != null) {
              var o = keySp.GetValue("SP");
              intSp = Convert.ToInt32(o);
            } else {
              intSp = 0;
            }

            var s = valueName.Substring(1); //Format vx.y.z 
            char[] sep = {'.'};
            var parts = s.Split(sep, 3);

            for (var i = 0; i < parts.Length; i++) {
              if (!Double.TryParse(parts[i], out dvercomp[i])) {
                logger.Warn("Could not parse {0} as double, use 0 instead, part-index={1}, complete-string:{2}", parts[i], i, s);
                dvercomp[i] = 0;
              }
            }

            if ((dvercomp[0] > dversion[0]) || (dvercomp[0] == dversion[0] && dvercomp[1] >= dversion[1])) {
              dvercomp.CopyTo(dversion, 0);
              returnString = intSp > 0 ? String.Format("{0} SP{1}", valueName, intSp) : valueName;
            }
          }
          this.highestFrameworkVersion = String.Format(".NET Framework {0}", returnString);
        }
      } catch (Exception ex) {
        logger.Warn("while retrieving .net framework version this exception occured: {0}", ex);
      }

      return this.highestFrameworkVersion;
    }

    public string CollectRenderCapabilityTier() {
      var renderCapabilityTier = "Unknown Rendering Tier";

      var tier = RenderCapability.Tier / 0x10000;

      switch (tier) {
        case 0:
          renderCapabilityTier = "0/2 - No graphics hardware acceleration. The DirectX version level is less than version 7.0.";
          break;
        case 1:
          renderCapabilityTier = "1/2 - Partial graphics hardware acceleration. The DirectX version level is greater than or equal to version 7.0, and lesser than version 9.0.";
          break;
        case 2:
          renderCapabilityTier = "2/2 - Most graphics features use graphics hardware acceleration. The DirectX version level is greater than or equal to version 9.0.";
          break;
      }

      return renderCapabilityTier;
    }

    public string CollectSystemBitInfo() {
      this.systemBitInfo = "unknown bit mode";

      if (IntPtr.Size == 4) {
        this.systemBitInfo = "32-bit";
      }
      if (IntPtr.Size == 8) {
        this.systemBitInfo = "64-bit";
      }

      return this.systemBitInfo;
    }

    public override string ToString() {
      var sb = new StringBuilder();

      sb.AppendLine("=======================================");
      sb.AppendLine("System:");
      sb.AppendLine("");
      sb.AppendLine("* .NET Framework: " + this.FrameworkVersion);
      sb.AppendLine("* Highest .NET Framework: " + this.HighestFrameworkVersion);
      sb.AppendLine("* Render Capability Tier: " + this.RenderCapabilityTier);
      sb.AppendLine("* System Bit Info: " + this.SystemBitInfo);
      sb.AppendLine("* OS Version: " + this.OsVersion);
      sb.AppendLine("* ServicePack: " + this.OsServicePack);
      sb.AppendLine("* Platform: " + this.OsPlatform);
      sb.AppendLine("* Processor Count: " + this.ProcessorCount);
      sb.AppendLine("* Machine Name: " + this.MachineName);
      sb.AppendLine("* Current User Name: " + this.UserName);
      sb.AppendLine("* Application Process Id: " + this.ProcessId);
      sb.AppendLine("* Memory Usage: " + this.MemoryUsage);
      sb.AppendLine("* Application Von Time: " + this.StartTime);
      sb.AppendLine("* Command Line: " + this.CommandLine);
      sb.AppendLine("=======================================");
      sb.AppendLine("");

      return sb.ToString();
    }

    public StringBuilder PrettyPrintInfos(DateTime timeUtc) {
      var sb = new StringBuilder();
      sb.AppendFormat("==================== ::INFOS:: ========================================");
      sb.AppendLine();
      sb.AppendFormat("Version: {0}", this.AssemblyVersion);
      sb.AppendLine();
      sb.AppendFormat("Client: {0}({1})", this.MachineName, this.IPAdresses);
      sb.AppendLine();
      sb.AppendFormat("User: {0}", this.UserName);
      sb.AppendLine();
      sb.AppendFormat("OS (Version|ServicePack|Platform): {0}|{1}|{2}", this.OsVersion, this.OsServicePack, this.OsPlatform);
      sb.AppendLine();
      sb.AppendFormat("Processorcount: {0}", this.ProcessorCount);
      sb.AppendLine();
      sb.AppendFormat("Framework: {0} || {1}", Environment.Version, this.HighestFrameworkVersion);
      sb.AppendLine();
      sb.AppendFormat("WPF Rendering Tier = {0}", this.RenderCapabilityTier);
      sb.AppendLine();
      sb.AppendFormat("System-Bit-Mode is: {0}", this.SystemBitInfo);
      sb.AppendLine();
      sb.AppendFormat("Uptime: {0}", TimeSpanUtil.UptimeString(timeUtc));
      sb.AppendLine();
      return sb;
    }
  }
}