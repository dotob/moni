using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Media;
using Microsoft.Win32;
using NLog;

namespace MONI.Util
{
    public class EnvironmentHelper
    {
        private const string Netfx10RegKeyName = @"Software\Microsoft\.NETFramework\Policy\v1.0";
        private const string Netfx10RegKeyValue = "3705";

        private const string Netfx11RegKeyName = @"Software\Microsoft\NET Framework Setup\NDP\v1.1.4322";

        private const string Netfx20RegKeyName = @"Software\Microsoft\NET Framework Setup\NDP\v2.0.50727";

        private const string Netfx30RegKeyName = @"Software\Microsoft\NET Framework Setup\NDP\v3.0\Setup";
        private const string Netfx30RegValueName = "InstallSuccess";
        private const string Netfx30SpRegKeyName = @"Software\Microsoft\NET Framework Setup\NDP\v3.0";

        private const string Netfx35RegKeyName = @"Software\Microsoft\NET Framework Setup\NDP\v3.5";

        private const string Netfx40ClientRegKeyName = @"Software\Microsoft\NET Framework Setup\NDP\v4\Client";
        private const string Netfx40FullRegKeyName = @"Software\Microsoft\NET Framework Setup\NDP\v4\Full";
        private const string Netfx40SPxRegValueName = "Servicing";
        private const int Netfx451ReleaseVersion = 378675;
        private const int Netfx452ReleaseVersion = 379893;

        private const string Netfx45RegKeyName = @"Software\Microsoft\NET Framework Setup\NDP\v4\Full";
        private const string Netfx45RegValueName = "Release";

        private const int Netfx45ReleaseVersion = 378389;
        private const int Netfx461ReleaseVersion = 394254;
        private const int Netfx462ReleaseVersion = 394802;
        private const int Netfx46ReleaseVersion = 393295;
        private const int Netfx471ReleaseVersion = 461308;
        private const int Netfx472ReleaseVersion = 461808;
        private const int Netfx47ReleaseVersion = 460798;
        private const int Netfx48ReleaseVersion = 528040;

        private const string NetfxStandardRegValueName = "Install";
        private const string NetfxStandardSPxRegValueName = "SP";

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private string assemblyCompany;
        private string assemblyCopyright;
        private string assemblyDescription;
        private string assemblyInformationalVersion;
        private string assemblyProduct;
        private string assemblyTitle;
        private string assemblyVersion;
        private string highestFrameworkVersion;
        private string ipAddresses;
        private string version;

        #region IEnvironment Members

        public string Version
        {
            get => this.version ?? (this.version = CollectAssemblyVersion());
            set => this.version = value;
        }

        public string AssemblyTitle => this.assemblyTitle ?? (this.assemblyTitle = CollectAssemblyTitle());

        public string AssemblyVersion => this.assemblyVersion ?? (this.assemblyVersion = CollectAssemblyVersion());

        public string AssemblyDescription =>
            this.assemblyDescription ?? (this.assemblyDescription = CollectAssemblyDescription());

        public string AssemblyInformationalVersion => this.assemblyInformationalVersion ??
                                                      (this.assemblyInformationalVersion =
                                                          CollectAssemblyInformationalVersion());

        public string AssemblyProduct => this.assemblyProduct ?? (this.assemblyProduct = CollectAssemblyProduct());

        public string AssemblyCopyright =>
            this.assemblyCopyright ?? (this.assemblyCopyright = CollectAssemblyCopyright());

        public string AssemblyCompany => this.assemblyCompany ?? (this.assemblyCompany = CollectAssemblyCompany());

        public string HighestFrameworkVersion => this.highestFrameworkVersion ??
                                                 (this.highestFrameworkVersion = CollectHighestFrameWorkVersion());

        /// <summary>
        /// RenderCapabilityTier may change during runtime, so don't cache, always collect.
        /// </summary>
        public string RenderCapabilityTier => CollectRenderCapabilityTier();

        public string OsBitness
        {
            get
            {
                try
                {
                    return CollectBitness(Environment.Is64BitOperatingSystem);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Environment.Is64BitOperatingSystem");
                }

                return "n/a";
            }
        }

        public string OsVersion
        {
            get
            {
                try
                {
                    return Environment.OSVersion.VersionString;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Environment.OSVersion.VersionString");
                }

                return "n/a";
            }
        }

        public string ProcessBitness
        {
            get
            {
                try
                {
                    return CollectBitness(Environment.Is64BitProcess);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Environment.Is64BitProcess");
                }

                return "n/a";
            }
        }

        public string OsPlatform
        {
            get
            {
                try
                {
                    return Environment.OSVersion.Platform.ToString();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Environment.OSVersion.Platform");
                }

                return "n/a";
            }
        }

        public string OsServicePack
        {
            get
            {
                try
                {
                    return Environment.OSVersion.ServicePack;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Environment.OSVersion.ServicePack");
                }

                return "n/a";
            }
        }

        public float PhysicalMemory
        {
            get
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory"))
                    {
                        return searcher.Get()
                                       .Cast<ManagementBaseObject>()
                                       .Select(obj => Convert.ToInt64(obj["Capacity"]))
                                       .Sum()
                                       .ToMB();
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Capacity from Win32_PhysicalMemory via WMI");
                }

                return -1;
            }
        }

        public string ProcessorCores
        {
            get
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT NumberOfCores FROM Win32_Processor"))
                    {
                        return string.Join("; ",
                                           searcher.Get().Cast<ManagementBaseObject>().Select(obj => obj["NumberOfCores"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving NumberOfCores from Win32_Processor via WMI");
                }

                return "n/a";
            }
        }

        public string ProcessorCount
        {
            get
            {
                try
                {
                    return Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Environment.ProcessorCount");
                }

                return "n/a";
            }
        }

        public string ProcessorMaxSpeed
        {
            get
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT MaxClockSpeed FROM Win32_Processor"))
                    {
                        return string.Join("; ",
                                           searcher.Get().Cast<ManagementBaseObject>().Select(obj => obj["MaxClockSpeed"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving MaxClockSpeed from Win32_Processor via WMI");
                }

                return "n/a";
            }
        }

        public string ProcessorName
        {
            get
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                    {
                        return string.Join("; ",
                                           searcher.Get().Cast<ManagementBaseObject>().Select(obj => obj["Name"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Name from Win32_Processor via WMI");
                }

                return "n/a";
            }
        }

        public string MachineName
        {
            get
            {
                try
                {
                    return Environment.MachineName;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Environment.MachineName");
                }

                return "n/a";
            }
        }

        public string FrameworkVersion
        {
            get
            {
                try
                {
                    return Environment.Version.ToString();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Environment.Version");
                }

                return "n/a";
            }
        }

        public string UserName
        {
            get
            {
                try
                {
                    return Environment.UserName;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Environment.UserName");
                }

                return "n/a";
            }
        }

        public string ProcessId => Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);

        public DateTime StartTimeUtc => Process.GetCurrentProcess().StartTime.ToUniversalTime();

        public string ProcessWorkingSet => $"{Process.GetCurrentProcess().WorkingSet64.ToMB():0} MB";

        public string ProcessPeakWorkingSet => $"{Process.GetCurrentProcess().PeakWorkingSet64.ToMB():0} MB";

        public string ProcessPrivateMemorySize => $"{Process.GetCurrentProcess().PrivateMemorySize64.ToMB():0} MB";

        public string IpAddresses => this.ipAddresses ?? (this.ipAddresses = CalcIpAddresses());

        public string CommandLine
        {
            get
            {
                try
                {
                    return Environment.CommandLine;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Exception occurred while retrieving Environment.CommandLine");
                }

                return "n/a";
            }
        }

        #endregion

        private static string CalcIpAddresses()
        {
            IPAddress[] addressList = Dns.GetHostAddresses(Dns.GetHostName());
            return string.Join(",",
                               addressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Select(ip => ip.ToString())
                                          .ToArray());
        }

        /// <summary>
        /// Uses the detection method recommended at http://msdn.microsoft.com/library/ms994349.aspx to determine whether the .NET
        /// Framework 1.0 is installed on the machine.
        /// </summary>
        private static bool IsNetfx10Installed()
        {
            return RegistryGetValue<string>(Registry.LocalMachine, Netfx10RegKeyName, Netfx10RegKeyValue) != null;
        }

        /// <summary>
        /// Uses the detection method recommended at http://msdn.microsoft.com/library/ms994339.aspx to determine whether the .NET
        /// Framework 1.1 is installed on the machine.
        /// </summary>
        private static bool IsNetfx11Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx11RegKeyName, NetfxStandardRegValueName) == 1;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/previous-versions/dotnet/articles/aa480243(v=msdn.10)#detecting-installed-net-framework-20
        /// to determine whether the .NET Framework 2.0 is installed on the machine.
        /// </summary>
        private static bool IsNetfx20Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx20RegKeyName, NetfxStandardRegValueName) == 1;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-3.0/aa964978(v=vs.85)#detecting-the-microsoft-net-framework-30
        /// to determine whether the .NET Framework 3.0 is installed on the machine.
        /// </summary>
        private static bool IsNetfx30Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx30RegKeyName, Netfx30RegValueName) == 1;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2008/cc160716(v=vs.90)#detecting-the-net-framework-35
        /// to determine whether the .NET Framework 3.5 is installed on the machine.
        /// </summary>
        private static bool IsNetfx35Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx35RegKeyName, NetfxStandardRegValueName) == 1;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/ee942965(v=vs.100)#detecting-the-net-framework-4
        /// to determine whether the .NET Framework 4 Client is installed on the machine.
        /// </summary>
        private static bool IsNetfx40ClientInstalled()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx40ClientRegKeyName, NetfxStandardRegValueName) ==
                   1;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/ee942965(v=vs.100)#detecting-the-net-framework-4
        /// to determine whether the .NET Framework 4 Full is installed on the machine.
        /// </summary>
        private static bool IsNetfx40FullInstalled()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx40FullRegKeyName, NetfxStandardRegValueName) == 1;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#detecting-the-net-framework
        /// to determine whether the .NET Framework 4.5 is installed on the machine.
        /// </summary>
        private static bool IsNetfx45Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx45RegKeyName, Netfx45RegValueName) >=
                   Netfx45ReleaseVersion;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#detecting-the-net-framework
        /// to determine whether the .NET Framework 4.5.1 is installed on the machine.
        /// </summary>
        private static bool IsNetfx451Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx45RegKeyName, Netfx45RegValueName) >=
                   Netfx451ReleaseVersion;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#detecting-the-net-framework
        /// to determine whether the .NET Framework 4.5.2 is installed on the machine.
        /// </summary>
        private static bool IsNetfx452Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx45RegKeyName, Netfx45RegValueName) >=
                   Netfx452ReleaseVersion;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#detecting-the-net-framework
        /// to determine whether the .NET Framework 4.6 is installed on the machine.
        /// </summary>
        private static bool IsNetfx46Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx45RegKeyName, Netfx45RegValueName) >=
                   Netfx46ReleaseVersion;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#detecting-the-net-framework
        /// to determine whether the .NET Framework 4.6.1 is installed on the machine.
        /// </summary>
        private static bool IsNetfx461Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx45RegKeyName, Netfx45RegValueName) >=
                   Netfx461ReleaseVersion;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#detecting-the-net-framework
        /// to determine whether the .NET Framework 4.6.2 is installed on the machine.
        /// </summary>
        private static bool IsNetfx462Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx45RegKeyName, Netfx45RegValueName) >=
                   Netfx462ReleaseVersion;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#detecting-the-net-framework
        /// to determine whether the .NET Framework 4.7 is installed on the machine.
        /// </summary>
        private static bool IsNetfx47Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx45RegKeyName, Netfx45RegValueName) >=
                   Netfx47ReleaseVersion;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#detecting-the-net-framework
        /// to determine whether the .NET Framework 4.7.1 is installed on the machine.
        /// </summary>
        private static bool IsNetfx471Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx45RegKeyName, Netfx45RegValueName) >=
                   Netfx471ReleaseVersion;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#detecting-the-net-framework
        /// to determine whether the .NET Framework 4.7.2 is installed on the machine.
        /// </summary>
        private static bool IsNetfx472Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx45RegKeyName, Netfx45RegValueName) >=
                   Netfx472ReleaseVersion;
        }

        /// <summary>
        /// Uses the detection method recommended at
        /// https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers#detecting-the-net-framework
        /// to determine whether the .NET Framework 4.8 is installed on the machine.
        /// </summary>
        private static bool IsNetfx48Installed()
        {
            return RegistryGetValue<int>(Registry.LocalMachine, Netfx45RegKeyName, Netfx45RegValueName) >=
                   Netfx48ReleaseVersion;
        }

        /// <summary>
        /// Determine what service pack is installed for a version of the .NET Framework using registry based detection methods
        /// documented in the .NET Framework deployment guides.
        /// </summary>
        private static int GetNetfxSPLevel(string subKeyName, string valueName)
        {
            return RegistryGetValue<int>(Registry.LocalMachine, subKeyName, valueName);
        }

        private static T RegistryGetValue<T>(RegistryKey rootKey, string subKeyName, string valueName)
        {
            using (var rksub = rootKey.OpenSubKey(subKeyName))
            {
                if (rksub != null)
                {
                    return rksub.GetValue(valueName) is T
                        ? (T)rksub.GetValue(valueName)
                        : default;
                }
            }

            return default;
        }

        private static string CollectAssemblyVersion()
        {
            return AssemblyToUse.GetName().Version.ToString();
        }

        private static string CollectAssemblyInformationalVersion()
        {
            return AssemblyToUse.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        }

        private static string CollectAssemblyTitle()
        {
            var titleAttribute = AssemblyToUse.GetCustomAttributes<AssemblyTitleAttribute>().FirstOrDefault();

            // If the title is not an empty string, return it.
            if (!string.IsNullOrEmpty(titleAttribute?.Title))
            {
                return titleAttribute.Title;
            }

            // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
            return Path.GetFileNameWithoutExtension(AssemblyToUse.CodeBase);
        }

        private static string CollectAssemblyDescription()
        {
            var descriptionAttribute =
                AssemblyToUse.GetCustomAttributes<AssemblyDescriptionAttribute>().FirstOrDefault();

            // If the description is not an empty string, return it.
            if (!string.IsNullOrEmpty(descriptionAttribute?.Description))
            {
                return descriptionAttribute.Description;
            }

            // If there aren't any Description attributes, return an empty string.
            Log.Error("unable to detect assembly description");
            return string.Empty;
        }

        private static string CollectAssemblyProduct()
        {
            var productAttribute = AssemblyToUse.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault();

            // If the description is not an empty string, return it.
            if (!string.IsNullOrEmpty(productAttribute?.Product))
            {
                return productAttribute.Product;
            }

            // If there aren't any Description attributes, return an empty string.
            Log.Error("unable to detect assembly product");
            return string.Empty;
        }

        private static string CollectAssemblyCopyright()
        {
            var copyrightAttribute = AssemblyToUse.GetCustomAttributes<AssemblyCopyrightAttribute>().FirstOrDefault();

            // If the description is not an empty string, return it.
            if (!string.IsNullOrEmpty(copyrightAttribute?.Copyright))
            {
                return copyrightAttribute.Copyright;
            }

            // If there aren't any Description attributes, return an empty string.
            Log.Error("unable to detect assembly copyright");
            return string.Empty;
        }

        private static string CollectAssemblyCompany()
        {
            var companyAttribute = AssemblyToUse.GetCustomAttributes<AssemblyCompanyAttribute>().FirstOrDefault();

            // If the description is not an empty string, return it.
            if (!string.IsNullOrEmpty(companyAttribute?.Company))
            {
                return companyAttribute.Company;
            }

            // If there aren't any Description attributes, return an empty string.
            Log.Error("unable to detect assembly company");
            return string.Empty;
        }

        private static string CollectHighestFrameWorkVersion()
        {
            var retValue = string.Empty;
            try
            {
                // Determine whether or not the .NET Framework 1.0, 1.1, 2.0, 3.0, 3.5, 4, 4.5, 4.5.1, 4.5.2, 4.6, or 4.6.1 are installed.
                bool netfx10Installed = IsNetfx10Installed();
                bool netfx11Installed = IsNetfx11Installed();

                bool netfx20Installed = IsNetfx20Installed();
                int netfx20SPLevel = GetNetfxSPLevel(Netfx20RegKeyName, NetfxStandardSPxRegValueName);

                // The .NET Framework 3.0 is an add-in that installs on top of the .NET Framework 2.0.
                // For this version check, validate that both 2.0 and 3.0 are installed.
                bool netfx30Installed = IsNetfx20Installed() && IsNetfx30Installed();
                int netfx30SPLevel = GetNetfxSPLevel(Netfx30SpRegKeyName, NetfxStandardSPxRegValueName);

                // The .NET Framework 3.5 is an add-in that installs on top of the .NET Framework 2.0 and 3.0.
                // For this version check, validate that 2.0, 3.0 and 3.5 are installed.
                bool netfx35Installed = IsNetfx20Installed() && IsNetfx30Installed() && IsNetfx35Installed();
                int netfx35SPLevel = GetNetfxSPLevel(Netfx35RegKeyName, NetfxStandardSPxRegValueName);

                bool netfx40ClientInstalled = IsNetfx40ClientInstalled();
                int netfx40ClientSPLevel = GetNetfxSPLevel(Netfx40ClientRegKeyName, Netfx40SPxRegValueName);

                bool netfx40FullInstalled = IsNetfx40FullInstalled();
                int netfx40FullSPLevel = GetNetfxSPLevel(Netfx40FullRegKeyName, Netfx40SPxRegValueName);

                bool netfx45Installed = IsNetfx45Installed();
                bool netfx451Installed = IsNetfx451Installed();
                bool netfx452Installed = IsNetfx452Installed();
                bool netfx46Installed = IsNetfx46Installed();
                bool netfx461Installed = IsNetfx461Installed();
                bool netfx462Installed = IsNetfx462Installed();
                bool netfx47Installed = IsNetfx47Installed();
                bool netfx471Installed = IsNetfx471Installed();
                bool netfx472Installed = IsNetfx472Installed();
                bool netfx48Installed = IsNetfx48Installed();

                // Since 4.5.x/4.6.x/4.7.x are inplace upgrades, they all share the SP-level.
                int netfx45SPLevel = GetNetfxSPLevel(Netfx45RegKeyName, Netfx40SPxRegValueName);

                if (netfx48Installed)
                {
                    retValue = FormatFrameworkVersionNameString("4.8", netfx45SPLevel);
                }
                else if (netfx472Installed)
                {
                    retValue = FormatFrameworkVersionNameString("4.7.2", netfx45SPLevel);
                }
                else if (netfx471Installed)
                {
                    retValue = FormatFrameworkVersionNameString("4.7.1", netfx45SPLevel);
                }
                else if (netfx47Installed)
                {
                    retValue = FormatFrameworkVersionNameString("4.7", netfx45SPLevel);
                }
                else if (netfx462Installed)
                {
                    retValue = FormatFrameworkVersionNameString("4.6.2", netfx45SPLevel);
                }
                else if (netfx461Installed)
                {
                    retValue = FormatFrameworkVersionNameString("4.6.1", netfx45SPLevel);
                }
                else if (netfx46Installed)
                {
                    retValue = FormatFrameworkVersionNameString("4.6", netfx45SPLevel);
                }
                else if (netfx452Installed)
                {
                    retValue = FormatFrameworkVersionNameString("4.5.2", netfx45SPLevel);
                }
                else if (netfx451Installed)
                {
                    retValue = FormatFrameworkVersionNameString("4.5.1", netfx45SPLevel);
                }
                else if (netfx45Installed)
                {
                    retValue = FormatFrameworkVersionNameString("4.5", netfx45SPLevel);
                }
                else if (netfx40FullInstalled)
                {
                    retValue = FormatFrameworkVersionNameString("4 Full", netfx40FullSPLevel);
                }
                else if (netfx40ClientInstalled)
                {
                    retValue = FormatFrameworkVersionNameString("4 Client", netfx40ClientSPLevel);
                }
                else if (netfx35Installed)
                {
                    retValue = FormatFrameworkVersionNameString("3.5", netfx35SPLevel);
                }
                else if (netfx30Installed)
                {
                    retValue = FormatFrameworkVersionNameString("3.0", netfx30SPLevel);
                }
                else if (netfx20Installed)
                {
                    retValue = FormatFrameworkVersionNameString("2.0", netfx20SPLevel);
                }
                else if (netfx11Installed)
                {
                    // Ignore the SP-level, since it cannot be installed on modern Windows anyway.
                    retValue = FormatFrameworkVersionNameString("1.1", 0);
                }
                else if (netfx10Installed)
                {
                    // Ignore the SP-level, since it cannot be installed on modern Windows anyway.
                    retValue = FormatFrameworkVersionNameString("1.0", 0);
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Exception while retrieving .NET Framework version");
            }

            return retValue;
        }

        private static string FormatFrameworkVersionNameString(string versionName, int spLevel)
        {
            return spLevel > 0
                ? string.Format(CultureInfo.InvariantCulture, ".NET Framework {0} SP{1}", versionName, spLevel)
                : string.Format(CultureInfo.InvariantCulture, ".NET Framework {0}", versionName);
        }

        private static string CollectRenderCapabilityTier()
        {
            try
            {
                int tier = RenderCapability.Tier / 0x10000;

                switch (tier)
                {
                    case 0:
                        return
                            "0/2 - No graphics hardware acceleration. The DirectX version level is less than version 7.0.";
                    case 1:
                        return
                            "1/2 - Partial graphics hardware acceleration. The DirectX version level is greater than or equal to version 7.0, and lesser than version 9.0.";
                    case 2:
                        return
                            "2/2 - Most graphics features use graphics hardware acceleration. The DirectX version level is greater than or equal to version 9.0.";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not get RenderCapability.Tier");
            }

            return "Unknown Rendering Tier";
        }

        private static string CollectBitness(bool is64Bit)
        {
            return is64Bit
                ? "64-bit"
                : "32-bit";
        }

        private static Assembly AssemblyToUse => Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
    }
}