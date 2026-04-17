using System;
using System.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Feature.Services.Protocol;
using Rider.Plugins.CodeGenJetbrains.Model;

namespace Rider.Plugins.CodeGenJetbrains.Extensions;

public static class GitExtensions
{
    [Obsolete("Use TryAddToGit() instead")]
    public static IProjectFile TryAddToGitOld(this IProjectFile projectFile)
    {
        var fullPath = projectFile.Location.FullPath;
        var folderDir = projectFile.Location.Directory.FullPath;

        if (IsGitRepository(folderDir))
            RunGitCommand(folderDir, $"add \"{fullPath}\"");

        return projectFile;
    }

    public static IProjectFile TryAddToGit(this IProjectFile projectFile)
    {
        var solution = projectFile.GetSolution();
        var model = solution.GetProtocolSolution().GetRdCodeGenJetbrainsModel();
        var path = projectFile.Location.FullPath;
        ((RdCall<string, bool>)model.AddToVcs).Start(path);
        return projectFile;
    }

    public static bool IsGitRepository(string workingDir)
    {
        var result = RunGitCommand(workingDir, "rev-parse --is-inside-work-tree");
        return result.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    public static string RunGitCommand(string workingDir, string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return string.Empty;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(1000);

            return output;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
