using System;
using System.Text;

namespace MONI.Util
{
    public class EnvironmentInfos
    {
        private EnvironmentHelper environmentHelper;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static EnvironmentInfos()
        {
        }

        private EnvironmentInfos(EnvironmentHelper helper)
        {
            this.environmentHelper = helper;
        }

        public static EnvironmentInfos Instance { get; } = new EnvironmentInfos(new EnvironmentHelper());

        public string MemoryUsage(string applicationName, DateTime startedUtcTime)
        {
            return $"{applicationName} ({this.environmentHelper.ProcessId}) - Memory (Private Memory, WorkingSet, Peak WS): {this.environmentHelper.ProcessPrivateMemorySize}, {this.environmentHelper.ProcessWorkingSet}, {this.environmentHelper.ProcessPeakWorkingSet} - UpTime: {TimeSpanUtil.UptimeString(startedUtcTime)}";
        }

        public StringBuilder PrettyPrintInfos()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("========================================:: Environment infos")
              .AppendLine()
              .AppendLine($"                         Assembly-Version: {this.environmentHelper.AssemblyVersion}")
              .AppendLine($"                    Informational-Version: {this.environmentHelper.AssemblyInformationalVersion}")
              .AppendLine()
              .AppendLine($"                                   Client: {this.environmentHelper.MachineName} ({this.environmentHelper.IpAddresses})")
              .AppendLine($"                                     User: {this.environmentHelper.UserName}")
              .AppendLine()
              .AppendLine($"OS (Version|ServicePack|Platform|Bitness): {this.environmentHelper.OsVersion}|{this.environmentHelper.OsServicePack}|{this.environmentHelper.OsPlatform}|{this.environmentHelper.OsBitness}")
              .AppendLine($"                           Processor Name: {this.environmentHelper.ProcessorName}")
              .AppendLine($"                     Processor Max. Speed: {this.environmentHelper.ProcessorMaxSpeed}")
              .AppendLine($"                    Processor Count|Cores: {this.environmentHelper.ProcessorCount}|{this.environmentHelper.ProcessorCores}")
              .AppendLine($"                          Physical Memory: {this.environmentHelper.PhysicalMemory} MB")
              .AppendLine()
              .AppendLine($"                                Framework: {this.environmentHelper.FrameworkVersion} || {this.environmentHelper.HighestFrameworkVersion}")
              .AppendLine($"                       WPF Rendering Tier: {this.environmentHelper.RenderCapabilityTier}")
              .AppendLine()
              .AppendLine($"                      Application Bitness: {this.environmentHelper.ProcessBitness}")
              .AppendLine($"                              Commandline: {this.environmentHelper.CommandLine}");
            return sb;
        }
    }
}