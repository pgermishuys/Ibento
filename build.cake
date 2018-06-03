#addin "Cake.FileHelpers"

var target          = Argument("target", "Default");
var version         = Argument("version", "1.0.0");
var output          = Argument("output", "../build");
var configuration   = Argument("configuration", "Release");
var srcDirectory    = Directory("src");
var outputDirectory = Directory(output);

Task("Clean")
    .Does(() =>
{
    CleanDirectory(outputDirectory);
    CreateDirectory(outputDirectory);
});

Task("BuildClr")
    .IsDependentOn("Clean")
    .IsDependentOn("VersionAssembly")
    .IsDependentOn("RestoreNuget")
    .Does(() =>
{
    DotNetCoreBuild(srcDirectory, new DotNetCoreBuildSettings {
        Configuration = configuration,
        EnvironmentVariables = DotNetEnvironment,
        NoRestore = true,
        ArgumentCustomization = args => args.Append("/p:Version=" + version),
    });
});

Task("RestoreNuGet")
    .Does(() =>
{
    DotNetCoreRestore(srcDirectory, new DotNetCoreRestoreSettings {
        NoCache = true,
        EnvironmentVariables = DotNetEnvironment
    });
});

Task("RevertVersionFiles")
.Does(() => {
    var files = GetFiles("./**/VersionInfo.cs");
    foreach(var file in files){
        StartProcessAndReturnOutput("git", $"checkout {file}");
    }
});

Task("VersionAssembly")
.Does(() => {
    var branchName = StartProcessAndReturnOutput("git", "rev-parse --abbrev-ref HEAD");
    var commitHash = StartProcessAndReturnOutput("git", "log -n1 --pretty=format:\"%H\" HEAD");
    var commitTimeStamp = StartProcessAndReturnOutput("git", "log -n1 --pretty=format:\"%aD\" HEAD");

    var versionPattern = ".*(Version = ).*";
    var branchPattern = ".*(Branch = ).*";
    var commitHashPattern = ".*(Hashtag = ).*";
    var timestampPattern = ".*(Timestamp = ).*";

    var newVersion=$"public static readonly string Version = \"{version}\";";
    var newBranch=$"public static readonly string Branch = \"{branchName}\";";
    var newCommitHash=$"public static readonly string Hashtag = \"{commitHash}\";";
    var newCommitTimeStamp=$"public static readonly string Timestamp = \"{commitTimeStamp}\";";

    ReplaceRegexInFiles("./**/VersionInfo.cs", versionPattern, newVersion);
    ReplaceRegexInFiles("./**/VersionInfo.cs", branchPattern, newBranch);
    ReplaceRegexInFiles("./**/VersionInfo.cs", commitHashPattern, newCommitHash);
    ReplaceRegexInFiles("./**/VersionInfo.cs", timestampPattern, newCommitTimeStamp);
});

Dictionary<string, string> DotNetEnvironment => new Dictionary<string, string> {
    {"DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1"},
    {"DOTNET_CLI_TELEMETRY_OPTOUT", "1"}
};

string StartProcessAndReturnOutput(string processName, string arguments){
    IEnumerable<string> stdout;
    StartProcess (processName, new ProcessSettings {
            Arguments = arguments,
            RedirectStandardOutput = true,
        }, out stdout);
    return string.Join("", stdout);
}

Task("Default")
  .IsDependentOn("BuildClr")
  .IsDependentOn("RevertVersionFiles");


RunTarget(target);