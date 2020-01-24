///////////////////////////////////////////////////////////////////////////////
// TOOLS / ADDINS
///////////////////////////////////////////////////////////////////////////////

#module nuget:?package=Cake.DotNetTool.Module
#tool "dotnet:?package=AzureSignTool&version=2.0.17"

#tool GitVersion.CommandLine&version=5.0.1
#tool gitreleasemanager
#tool xunit.runner.console
#tool vswhere
#addin Cake.Figlet

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosity = Argument("verbosity", Verbosity.Minimal);

///////////////////////////////////////////////////////////////////////////////
// PREPARATION
///////////////////////////////////////////////////////////////////////////////

var repoName = "MONI";
var isLocal = BuildSystem.IsLocalBuild;

// Set build version
if (isLocal == false || verbosity == Verbosity.Verbose)
{
    GitVersion(new GitVersionSettings { OutputType = GitVersionOutput.BuildServer });
}
GitVersion gitVersion = GitVersion(new GitVersionSettings { OutputType = GitVersionOutput.Json });

var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var branchName = gitVersion.BranchName;
var isDevelopBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", branchName);
var isReleaseBranch = StringComparer.OrdinalIgnoreCase.Equals("master", branchName);
var isTagged = AppVeyor.Environment.Repository.Tag.IsTag;

var latestInstallationPath = VSWhereLatest(new VSWhereLatestSettings { IncludePrerelease = true });
var msBuildPath = latestInstallationPath.Combine("./MSBuild/Current/Bin");
var msBuildPathExe = msBuildPath.CombineWithFilePath("./MSBuild.exe");

if (FileExists(msBuildPathExe) == false)
{
    throw new NotImplementedException("You need at least Visual Studio 2019 to build this project.");
}

// Directories and Paths
var solution = "MONI.sln";
var publishDir = "./Publish";
var testResultsDir = Directory("./TestResults");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    if (!IsRunningOnWindows())
    {
        throw new NotImplementedException($"{repoName} will only build on Windows because it's not possible to target WPF and Windows Forms from UNIX.");
    }

    Information(Figlet(repoName));

    Information("Informational   Version: {0}", gitVersion.InformationalVersion);
    Information("SemVer          Version: {0}", gitVersion.SemVer);
    Information("AssemblySemVer  Version: {0}", gitVersion.AssemblySemVer);
    Information("MajorMinorPatch Version: {0}", gitVersion.MajorMinorPatch);
    Information("NuGet           Version: {0}", gitVersion.NuGetVersion);
    Information("IsLocalBuild           : {0}", isLocal);
    Information("Branch                 : {0}", branchName);
    Information("Configuration          : {0}", configuration);
    Information("MSBuildPath            : {0}", msBuildPath);
});

Teardown(ctx =>
{
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .ContinueOnError()
    .Does(() =>
{
    var directoriesToDelete = GetDirectories("./**/obj")
        .Concat(GetDirectories("./**/bin"))
        .Concat(GetDirectories("./**/Publish"))
        .Concat(GetDirectories("./**/TestResults"))
        ;
    DeleteDirectories(directoriesToDelete, new DeleteDirectorySettings { Recursive = true, Force = true });
});

Task("Restore")
    .Does(() =>
{
    NuGetRestore(solution, new NuGetRestoreSettings { MSBuildPath = msBuildPath.ToString() });
});

Task("Build")
    .Does(() =>
{
    var msBuildSettings = new MSBuildSettings {
        Verbosity = verbosity
        , ToolPath = msBuildPathExe
        , Configuration = configuration
        , ArgumentCustomization = args => args.Append("/m").Append("/nr:false") // The /nr switch tells msbuild to quite once it�s done
        , BinaryLogger = new MSBuildBinaryLogSettings() { Enabled = isLocal }
    };
    MSBuild(solution, msBuildSettings
            .SetMaxCpuCount(0)
            .WithProperty("Version", isReleaseBranch ? gitVersion.MajorMinorPatch : gitVersion.NuGetVersion)
            .WithProperty("AssemblyVersion", gitVersion.AssemblySemVer)
            .WithProperty("FileVersion", gitVersion.AssemblySemFileVer)
            .WithProperty("InformationalVersion", gitVersion.InformationalVersion)
            );
});

void SignFiles(IEnumerable<FilePath> files, string description)
{
    var vurl = EnvironmentVariable("azure-key-vault-url");
    if(string.IsNullOrWhiteSpace(vurl)) {
        Error("Could not resolve signing url.");
        return;
    }

    var vcid = EnvironmentVariable("azure-key-vault-client-id");
    if(string.IsNullOrWhiteSpace(vcid)) {
        Error("Could not resolve signing client id.");
        return;
    }

    var vcs = EnvironmentVariable("azure-key-vault-client-secret");
    if(string.IsNullOrWhiteSpace(vcs)) {
        Error("Could not resolve signing client secret.");
        return;
    }

    var vc = EnvironmentVariable("azure-key-vault-certificate");
    if(string.IsNullOrWhiteSpace(vc)) {
        Error("Could not resolve signing certificate.");
        return;
    }

    foreach(var file in files)
    {
        Information($"Sign file: {file}");
        var processSettings = new ProcessSettings {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Arguments = new ProcessArgumentBuilder()
                .Append("sign")
                .Append(MakeAbsolute(file).FullPath)
                .AppendSwitchQuoted("--file-digest", "sha256")
                .AppendSwitchQuoted("--description", description)
                .AppendSwitchQuoted("--description-url", "https://github.com/dotob/moni")
                .Append("--no-page-hashing")
                .AppendSwitchQuoted("--timestamp-rfc3161", "http://timestamp.digicert.com")
                .AppendSwitchQuoted("--timestamp-digest", "sha256")
                .AppendSwitchQuoted("--azure-key-vault-url", vurl)
                .AppendSwitchQuotedSecret("--azure-key-vault-client-id", vcid)
                .AppendSwitchQuotedSecret("--azure-key-vault-client-secret", vcs)
                .AppendSwitchQuotedSecret("--azure-key-vault-certificate", vc)
        };

        using(var process = StartAndReturnProcess("tools/AzureSignTool", processSettings))
        {
            process.WaitForExit();

            if (process.GetStandardOutput().Any())
            {
                Information($"Output:{Environment.NewLine}{string.Join(Environment.NewLine, process.GetStandardOutput())}");
            }

            if (process.GetStandardError().Any())
            {
                Information($"Errors occurred:{Environment.NewLine}{string.Join(Environment.NewLine, process.GetStandardError())}");
            }

            // This should output 0 as valid arguments supplied
            Information("Exit code: {0}", process.GetExitCode());
        }
    }
}

Task("Sign")
    .WithCriteria(() => !isPullRequest)
    .ContinueOnError()
    .Does(() =>
{
    var files = GetFiles("./MONI/**/bin/**/*.exe");
    SignFiles(files, "MONI, the better way to enter monlist data.");
});

Task("Zip")
    .Does(() =>
{
    EnsureDirectoryExists(Directory(publishDir));

    Zip($"./MONI/bin/{configuration}/net462/", publishDir + "/MONI-v" + gitVersion.NuGetVersion + ".zip");
});

Task("Tests")
    // .ContinueOnError()
    .Does(() =>
{
    var settings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
            NoRestore = true,
            Logger = "trx",
            ResultsDirectory = testResultsDir,
            Verbosity = DotNetCoreVerbosity.Normal
        };

    DotNetCoreTest("./MONI.Tests/MONI.Tests.csproj", settings);
});

Task("CreateRelease")
    .WithCriteria(() => !isTagged)
    .WithCriteria(() => !isPullRequest)
    .Does(() =>
{
    var username = EnvironmentVariable("GITHUB_USERNAME");
    if (string.IsNullOrEmpty(username))
    {
        throw new Exception("The GITHUB_USERNAME environment variable is not defined.");
    }

    var token = EnvironmentVariable("GITHUB_TOKEN");
    if (string.IsNullOrEmpty(token))
    {
        throw new Exception("The GITHUB_TOKEN environment variable is not defined.");
    }

    GitReleaseManagerCreate(username, token, "dotob", repoName, new GitReleaseManagerCreateSettings {
        Milestone         = gitVersion.MajorMinorPatch,
        Name              = gitVersion.AssemblySemFileVer,
        Prerelease        = isDevelopBranch,
        TargetCommitish   = branchName,
        WorkingDirectory  = "."
    });
});

///////////////////////////////////////////////////////////////////////////////
// TASK TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Tests");

Task("CI")
    .IsDependentOn("Default")
    .IsDependentOn("Sign")
    .IsDependentOn("Zip")
    ;

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);