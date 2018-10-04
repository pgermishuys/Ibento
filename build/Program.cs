using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    class Program
    {
        private const string Default = "default";
        private const string GetGitInfo = "getGitInfo";
        private const string VersionAssembly = "versionAssembly";
        private const string RevertVersionFiles = "revertVersionFiles";
        private const string Build = "build";

        static void Main(string[] args)
        {
            var version = "1.0.0";

            string branchName = null;
            string commitHash = null;
            string commitTimeStamp = null;

            var versionInfoFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
                .Where(path => path.Contains("VersionInfo.cs")).ToList();

            Target(GetGitInfo, () =>
            {
                branchName = Read("git", "rev-parse --abbrev-ref HEAD").Trim();
                commitHash = Read("git", "log -n1 --pretty=format:\"%H\" HEAD").Trim();
                commitTimeStamp = Read("git", "log -n1 --pretty=format:\"%aD\" HEAD").Trim();
            });
            Target(VersionAssembly, DependsOn(GetGitInfo), versionInfoFiles, versionInfoFile =>
            {
                var fileContents = File.ReadAllText(versionInfoFile);
                fileContents = Regex.Replace(fileContents, ".*(Version = ).*", $"public static readonly string Version = \"{version}\";");
                fileContents = Regex.Replace(fileContents, ".*(Branch = ).*", $"public static readonly string Branch = \"{branchName}\";");
                fileContents = Regex.Replace(fileContents, ".*(Hashtag = ).*", $"public static readonly string Hashtag = \"{commitHash}\";");
                fileContents = Regex.Replace(fileContents, ".*(Timestamp = ).*", $"public static readonly string Timestamp = \"{commitTimeStamp}\";");
                File.WriteAllText(versionInfoFile, fileContents);
            });
            Target(Build, () => Run("dotnet", "build ./src/Ibento.sln"));
            Target(RevertVersionFiles, versionInfoFiles, versionInfoFile =>
            {
                Run("git", $"checkout {versionInfoFile}");
            });
            Target(Default, DependsOn(VersionAssembly, Build, RevertVersionFiles));
            RunTargets(args);
        }
    }
}
