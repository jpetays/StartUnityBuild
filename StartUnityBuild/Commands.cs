namespace StartUnityBuild;

public static class Commands
{
    public static void GitStatus(string workingDirectory, Action finished)
    {
        const string outPrefix = "git";
        Form1.AddLine($">{outPrefix}", "status");
        Task.Run(async () =>
        {
            await RunCommand.Execute(outPrefix, "git", "status", workingDirectory, null,
                GitOutputFilter, Form1.ExitListener);
            finished();
        });
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
        Form1.OutputListener(prefix, line);
    }

    public static void UnityUpdate(string workingDirectory, string productVersion, string bundleVersion,
        Action<bool, string, string> finished)
    {
        const string outPrefix = "update";
        Form1.AddLine($">{outPrefix}", $"ProjectSettings.asset");
        var isProjectSettingsFound = false;
        var isProjectSettingsClean = false;
        Task.Run(async () =>
        {
            await RunCommand.Execute(outPrefix, "git", "status ProjectSettings", workingDirectory, null,
                MyOutputFilter, Form1.ExitListener);
            if (!isProjectSettingsClean)
            {
                Form1.AddLine("ERROR", $"ProjectSettings folder has changed files in it, can not update project");
                finished(false, string.Empty, string.Empty);
                return;
            }
            var updated = false;
            var prefix = updated ? outPrefix : "ERROR";
            Form1.OutputListener($"{prefix}", $"-Version {productVersion}");
            Form1.OutputListener($"{prefix}", $"-Bundle {bundleVersion}");
            Form1.AddLine($">{outPrefix}", $"done");
            finished(updated, productVersion, bundleVersion);
        });
        return;

        void MyOutputFilter(string prefix, string? line)
        {
            GitOutputFilter(prefix, line);
            if (line == null || isProjectSettingsFound)
            {
                return;
            }
            if (line.Contains("nothing to commit, working tree clean"))
            {
                isProjectSettingsClean = true;
            }
            if (line.Contains("ProjectSettings/ProjectVersion.txt"))
            {
                isProjectSettingsFound = true;
                isProjectSettingsClean = false;
            }
        }
    }

    public static void UnityBuild(string workingDirectory, string unityExecutable,
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
            return;
        }
        Form1.AddLine($">{outPrefix}", $"build targets: {string.Join(", ", buildTargets)}");
        var environmentVariables = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(unityExecutable))
        {
            if (!File.Exists(unityExecutable))
            {
                Form1.AddLine(outPrefix, $"Unity Executable not found: {unityExecutable}");
                return;
            }
            environmentVariables.Add("UNITY_EXE_OVERRIDE", unityExecutable);
        }
        string cachedFilename = null;
        FileInfo cachedFileInfo = null;
        Task.Run(async () =>
        {
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
                    break;
                }
                Thread.Sleep(delayAfterUnityBuild * 000);
            }
            fileSystemWatcher.EnableRaisingEvents = false;
            await RunCommand.Execute(outPrefix, "git", "status", workingDirectory, null,
                GitOutputFilter, Form1.ExitListener);
            finished();
        });
    }
}
