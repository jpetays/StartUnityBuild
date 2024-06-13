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
                OutputListener, Form1.ExitListener);
            finished();
        });
        return;

        void OutputListener(string prefix, string? line)
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
    }

    public static void UnityBuild(string workingDirectory, string unityExecutable,
        List<string> buildTargets, Action finished)
    {
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
                var arguments = $"""
                                 /C {batchBuildCommand} .\etc\batchBuild\_build_{buildTarget}.env
                                 """.Trim();
                Form1.AddLine($">{outPrefix}", $"run {batchFile} {buildTarget}");
                var result = await RunCommand.Execute(outPrefix, "cmd.exe", arguments,
                    workingDirectory, environmentVariables, Form1.OutputListener, Form1.ExitListener);
                if (result != 0)
                {
                    Form1.AddLine("ERROR", $"{buildTarget}: unexpected return code: {result}");
                    break;
                }
                if (!Directory.Exists(buildOutputFolder))
                {
                    Form1.AddLine("ERROR", $"build output not found: {buildOutputFolder}");
                    break;
                }
                if (i < buildTargets.Count - 1)
                {
                    const int delay = 5;
                    Form1.AddLine($".{outPrefix}", $"wait build shutdown for {delay} sec");
                    Thread.Sleep(delay * 000);
                }
            }
            finished();
        });
    }
}
