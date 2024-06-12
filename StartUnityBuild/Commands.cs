namespace StartUnityBuild;

public static class Commands
{
    public static void GitStatus(string workingDirectory, Action finished)
    {
        const string outPrefix = "git";
        Form1.AddLine($">{outPrefix}", "status");
        RunCommand.Execute(outPrefix, "git", "status", workingDirectory,
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

    public static async void UnityBuild(string workingDirectory, List<string> buildTargets, Action finished)
    {
        const string outPrefix = "unity";
        const string batchFile = "_unity_build_driver_auto.bat";
        workingDirectory = Path.Combine(workingDirectory, "etc", "batchBuild");
        var path = Path.Combine(workingDirectory, batchFile);
        if (!File.Exists(path))
        {
            Form1.AddLine(outPrefix, $"command not found: {path}");
            return;
        }
        Form1.AddLine($">{outPrefix}", $"start {batchFile} {string.Join(", ", buildTargets)}");
        foreach (var buildTarget in buildTargets)
        {
            var arguments = $"/C {batchFile} {buildTarget}";
            var result = await RunCommand.ExecuteAsync(outPrefix, "cmd.exe", arguments, workingDirectory,
                Form1.OutputListener, Form1.ExitListener);
            if (result != 0)
            {
                Form1.AddLine("ERROR", $"{buildTarget}: unexpected return code: {result}");
                break;
            }
        }
        finished();
    }
}
