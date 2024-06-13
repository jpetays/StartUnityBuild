namespace StartUnityBuild;

public static class Commands
{
    public static void GitStatus(string workingDirectory, Action finished)
    {
        const string outPrefix = "git";
        Form1.AddLine($">{outPrefix}", "status");
        RunCommand.ExecuteAsync(outPrefix, "git", "status", workingDirectory,
            OutputListener, Form1.ExitListener, finished);
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
        foreach (var buildTarget in buildTargets)
        {
            var arguments = $"""
                             /C {batchBuildCommand} .\etc\batchBuild\_build_{buildTarget}.env
                             """.Trim();
            Form1.AddLine($">{outPrefix}", $"start {arguments}");
            var result = RunCommand.ExecuteBlocking(outPrefix, "cmd.exe", arguments,
                workingDirectory, environmentVariables, Form1.OutputListener, Form1.ExitListener);
            if (result != 0)
            {
                Form1.AddLine("ERROR", $"{buildTarget}: unexpected return code: {result}");
                break;
            }
        }
        finished();
    }
}
