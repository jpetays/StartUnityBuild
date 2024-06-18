using System.Diagnostics.CodeAnalysis;

namespace StartUnityBuild;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class Commands
{
    /*
        https://git-scm.com/docs/git-push
        --quiet
            Suppress all output, including the listing of updated refs, unless an error occurs.
            Progress is not reported to the standard error stream.
        --dry-run
            Do everything except actually send the updates.
        --porcelain
            Produce machine-readable output.
            The output status line for each ref will be tab-separated and sent to stdout instead of stderr.
            The full symbolic names of the refs will be given.
*/
    public static bool IsDryRun;
    private static bool _gitBranchUpToDate;

    private static string PushOptions => $"--tags --quiet {(IsDryRun ? "--dry-run" : "")}";

    public static void GitStatus(string workingDirectory, Action finished)
    {
        const string outPrefix = "git";
        Form1.AddLine($">{outPrefix}", "git status");
        Task.Run(async () =>
        {
            await RunCommand.Execute(outPrefix, "git", "status", workingDirectory, null,
                GitOutputFilter, Form1.ExitListener);
            if (!_gitBranchUpToDate)
            {
                Form1.AddLine(outPrefix, "-");
                Form1.AddLine(outPrefix, "-You have changes that should/could be pushed to git!");
                Form1.AddLine(outPrefix, "-");
            }
            finished();
        });
    }

    public static void UnityUpdate(string workingDirectory, string productVersion, string bundleVersion,
        Action<bool, string, string> finished)
    {
        const string outPrefix = "update";
        Form1.AddLine($">{outPrefix}", $"ProjectSettings.asset");
        Form1.AddLine($">{outPrefix}", $"BuildInfo.cs");
        var isProjectSettingsDirty = false;
        var isBuildInfoDirty = false;
        Task.Run(async () =>
        {
            Form1.AddLine($".{outPrefix}", $"git status");
            await RunCommand.Execute(outPrefix, "git", "status", workingDirectory, null,
                MyOutputFilter, Form1.ExitListener);
            if (isProjectSettingsDirty || isBuildInfoDirty)
            {
                Form1.AddLine("ERROR", $"Project folder has changed files in it, can not update project");
                if (isProjectSettingsDirty)
                {
                    Form1.AddLine($"-{outPrefix}", $"ProjectSettings.asset");
                }
                if (isBuildInfoDirty)
                {
                    Form1.AddLine($"-{outPrefix}", $"BuildInfo.cs");
                }
                finished(false, string.Empty, string.Empty);
                return;
            }
            var updated = Form1.UpdateProjectSettingsFile(workingDirectory, ref productVersion, ref bundleVersion);
            if (updated)
            {
                Thread.Yield();
                Form1.AddLine($".{outPrefix}", $"git status");
                await RunCommand.Execute(outPrefix, "git", "status", workingDirectory,
                    null, MyOutputFilter, Form1.ExitListener);
                var gitChanges = isProjectSettingsDirty;
                if (!gitChanges)
                {
                    Form1.AddLine("ERROR", $"ProjectSettings.asset was not changed, can not update project");
                    finished(false, string.Empty, string.Empty);
                    return;
                }
                // Commit and push changes (in ProjectSettings.asset file).
                var message = $"auto update build version and bundle {bundleVersion}";
                Form1.AddLine($".{outPrefix}", $"git commit: {message}");
                await RunCommand.Execute(outPrefix, "git",
                    $"""commit -m "{message}" ProjectSettings/ProjectSettings.asset""",
                    workingDirectory, null,
                    GitOutputFilter, Form1.ExitListener);
                Form1.AddLine($".{outPrefix}", $"git push {PushOptions}");
                var result = await RunCommand.Execute(outPrefix, "git", $"push {PushOptions}",
                    workingDirectory, null,
                    GitOutputFilter, Form1.ExitListener);
                var gitPushPrefix = result == 0 ? $".{outPrefix}" : "ERROR";
                Form1.AddLine(gitPushPrefix, $"git push returns {result}");
                if (IsDryRun)
                {
                    Form1.AddLine($"-{outPrefix}", $"this was --dry-run");
                }
                // Final git status For Your Information only!
                Form1.AddLine($".{outPrefix}", $"git status");
                await RunCommand.Execute(outPrefix, "git", "status", workingDirectory,
                    null, GitOutputFilter, Form1.ExitListener);
                if (!_gitBranchUpToDate)
                {
                    Form1.AddLine(outPrefix, "-");
                    Form1.AddLine(outPrefix, "-You have changes that should/could be pushed to git!");
                    Form1.AddLine(outPrefix, "-");
                }
                var prefix = updated ? outPrefix : "ERROR";
                Form1.OutputListener($"{prefix}", $"-Version {productVersion}");
                Form1.OutputListener($"{prefix}", $"-Bundle {bundleVersion}");
                Form1.AddLine($">{outPrefix}", $"done");
            }
            finished(updated, productVersion, bundleVersion);
        });
        return;

        void MyOutputFilter(string prefix, string? line)
        {
            GitOutputFilter(prefix, line);
            if (line == null)
            {
                return;
            }
            if (line.Contains("modified:") && line.Contains("ProjectSettings/ProjectSettings.asset"))
            {
                isProjectSettingsDirty = true;
            }
            // BuildInfo can be located anywhere!
            if (line.Contains("modified:") && line.Contains("BuildInfo.cs"))
            {
                isBuildInfoDirty = true;
            }
        }
    }

    public static void UnityBuild(string workingDirectory, string unityExecutable, string bundleVersion,
        List<string> buildTargets, Action finished, FileSystemWatcher fileSystemWatcher, Action<long> fileSizeProgress)
    {
        const int delayAfterUnityBuild = 5;

        const string outPrefix = "unity";
        const string batchFile = "unityBatchBuild.bat";
        var batchBuildFolder = Path.Combine(".", "etc", "batchBuild");
        var batchBuildCommand = Path.Combine(batchBuildFolder, batchFile);
        if (!File.Exists(batchBuildCommand))
        {
            Form1.AddLine(outPrefix, $"build command not found: {batchBuildCommand}");
            finished();
            return;
        }
        Form1.AddLine($">{outPrefix}", $"build targets: {string.Join(", ", buildTargets)}");
        var environmentVariables = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(unityExecutable))
        {
            if (!File.Exists(unityExecutable))
            {
                Form1.AddLine(outPrefix, $"Unity Executable not found: {unityExecutable}");
                finished();
                return;
            }
            environmentVariables.Add("UNITY_EXE_OVERRIDE", unityExecutable);
        }
        string? cachedFilename = null;
        FileInfo? cachedFileInfo = null;
        var isBuildInfoModified = false;
        var buildInfoPath = "";
        Task.Run(async () =>
        {
            var abortBuild = false;
            for (var i = 0; i < buildTargets.Count; ++i)
            {
                var buildTarget = buildTargets[i];
                var buildOutputFolder = $"build{buildTarget}";
                if (Directory.Exists(buildOutputFolder))
                {
                    Form1.AddLine($".{outPrefix}", $"delete build output: {buildOutputFolder}");
                    Directory.Delete(buildOutputFolder, true);
                }
                fileSystemWatcher.Path = Path.Combine(".", "etc");
                fileSystemWatcher.Filter = $"_local_Build_{buildTarget}.log";
                fileSystemWatcher.NotifyFilter = NotifyFilters.Size;
                fileSystemWatcher.Changed += (_, e) =>
                {
                    try
                    {
                        if (cachedFilename != e.FullPath)
                        {
                            cachedFilename = e.FullPath;
                            cachedFileInfo = new FileInfo(cachedFilename);
                        }
                        else
                        {
                            cachedFileInfo!.Refresh();
                        }
                        fileSizeProgress(cachedFileInfo.Length);
                    }
                    catch (Exception)
                    {
                        // Just swallow
                    }
                };
                var arguments = $"""
                                 /C {batchBuildCommand} .\etc\batchBuild\_build_{buildTarget}.env
                                 """.Trim();
                Form1.AddLine($">{outPrefix}", $"run {batchFile} {buildTarget}");
                fileSystemWatcher.EnableRaisingEvents = true;
                var startTime = DateTime.Now;
                var result = await RunCommand.Execute(outPrefix, "cmd.exe", arguments,
                    workingDirectory, environmentVariables, Form1.OutputListener, Form1.ExitListener);
                var duration = DateTime.Now - startTime;
                fileSystemWatcher.EnableRaisingEvents = false;
                Form1.AddLine($".{outPrefix}", $"build {i}/{buildTargets.Count} took {duration:mm':'ss}");
                if (result != 0)
                {
                    Form1.AddLine("ERROR", $"{buildTarget}: unexpected return code: {result}");
                    break;
                }
                if (!Directory.Exists(buildOutputFolder))
                {
                    Form1.AddLine("ERROR", $"build output folder not found: {buildOutputFolder}");
                    abortBuild = true;
                    break;
                }
                Thread.Sleep(delayAfterUnityBuild * 000);
            }
            fileSystemWatcher.EnableRaisingEvents = false;
            if (!abortBuild)
            {
                // Check that BuildInfo was updated.
                Form1.AddLine($".{outPrefix}", $"git status Assets");
                await RunCommand.Execute(outPrefix, "git", "status Assets", workingDirectory, null,
                    MyOutputFilter, Form1.ExitListener);
                if (isBuildInfoModified)
                {
                    // Create a lightweight commit tag.
                    var tagName = $"build_{bundleVersion}_{DateTime.Today:yyyy-MM-dd}";
                    Form1.AddLine($".{outPrefix}", $"git tag {tagName}");
                    await RunCommand.Execute(outPrefix, "git", $"tag {tagName}",
                        workingDirectory, null, GitOutputFilter, Form1.ExitListener);

                    // Commit and push changes (in ProjectSettings.asset file).
                    var message = $"auto update BuildInfo.cs";
                    Form1.AddLine($".{outPrefix}", $"git commit: {message}");
                    await RunCommand.Execute(outPrefix, "git",
                        $"""commit -m "{message}" {buildInfoPath}""",
                        workingDirectory, null, GitOutputFilter, Form1.ExitListener);
                    Form1.AddLine($".{outPrefix}", $"git push {PushOptions}");
                    var result = await RunCommand.Execute(outPrefix, "git", $"push {PushOptions}",
                        workingDirectory, null, GitOutputFilter, Form1.ExitListener);
                    var gitPushPrefix = result == 0 ? $".{outPrefix}" : "ERROR";
                    Form1.AddLine(gitPushPrefix, $"git push returns {result}");
                    if (IsDryRun)
                    {
                        Form1.AddLine($"-{outPrefix}", $"this was --dry-run");
                    }
                }
            }
            // Final git status For Your Information only!
            Form1.AddLine($".{outPrefix}", $"git status");
            await RunCommand.Execute(outPrefix, "git", "status", workingDirectory, null,
                GitOutputFilter, Form1.ExitListener);
            if (!_gitBranchUpToDate)
            {
                Form1.AddLine(outPrefix, "-");
                Form1.AddLine(outPrefix, "-You have changes that should/could be pushed to git!");
                Form1.AddLine(outPrefix, "-");
            }
            if (abortBuild)
            {
                Form1.AddLine("ERROR", "*");
                Form1.AddLine("ERROR", "* Build was aborted!");
                Form1.AddLine("ERROR", "*");
            }
            finished();
        });
        return;

        void MyOutputFilter(string prefix, string? line)
        {
            GitOutputFilter(prefix, line);
            if (line == null)
            {
                return;
            }
            if (line.Contains("modified:") && line.Contains("BuildInfo.cs"))
            {
                isBuildInfoModified = true;
                buildInfoPath =
                    line.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1];
            }
        }
    }

    private static void GitOutputFilter(string prefix, string? line)
    {
        if (line == null || line.StartsWith("  (use \"git"))
        {
            return;
        }
        line = line.Replace("\t", "    ");
        if (line.Contains(" new file: "))
        {
            line = $"--> {line}";
        }
        else if (line.Contains(" modified: "))
        {
            line = $"--> {line}";
        }
        else if (line.Contains(" deleted: "))
        {
            line = $"--> {line}";
        }
        if (line.Contains("Your branch is up to date with 'origin/main'"))
        {
            _gitBranchUpToDate = true;
            Form1.OutputListener(prefix, $"+{line}");
            return;
        }
        if (line.Contains("Your branch is ahead of 'origin/main'"))
        {
            _gitBranchUpToDate = false;
            Form1.OutputListener(prefix, $"-{line}");
            return;
        }
        Form1.OutputListener(prefix, line);
    }
}
