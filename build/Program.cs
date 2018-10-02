using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    class Program
    {
        private const string Default = "default";
        private const string VersionAssembly = "versionAssembly";
        private const string RevertVersionFiles = "revertVersionFiles";
        private const string Build = "build";
        
        static string StartProcessAndReturnOutput(string processName, string arguments){
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = processName,
                Arguments = arguments,
                RedirectStandardOutput = true,
            });
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd().Trim();
        }

        static void Main(string[] args)
        {
            Target(VersionAssembly, () =>
            {
                var version = "1.0.0";
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
                
                Console.Write(newBranch);
                
                foreach(var versionInfoFile in Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
                    .Where(path => path.Contains("VersionInfo.cs")))
                {
                    var fileContents = File.ReadAllText(versionInfoFile);
                    fileContents = Regex.Replace(fileContents, versionPattern, newVersion);
                    fileContents = Regex.Replace(fileContents, branchPattern, newBranch);
                    fileContents = Regex.Replace(fileContents, commitHashPattern, newCommitHash);
                    fileContents = Regex.Replace(fileContents, timestampPattern, newCommitTimeStamp);
                    Console.WriteLine($"{fileContents}");
                    File.WriteAllText(versionInfoFile, fileContents);
                }
            });
            Target(Build, () => Run("dotnet", "build ./src/Ibento.sln"));
            Target(RevertVersionFiles, () =>
            {
                foreach(var versionInfoFile in Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
                    .Where(path => path.Contains("VersionInfo.cs")))
                {
                    StartProcessAndReturnOutput("git", $"checkout {versionInfoFile}");
                }
            });
            Target(Default, DependsOn(VersionAssembly, Build, RevertVersionFiles));
            RunTargets(args);
        }
    }
}