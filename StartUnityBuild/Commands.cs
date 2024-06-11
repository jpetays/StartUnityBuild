namespace StartUnityBuild;

public static class Commands
{
    public static void GitStatus(string workingDirectory)
    {
        const string outPrefix = "git";
        Form1.AddLine($">{outPrefix}", "status");
        RunCommand.Execute(outPrefix, "git", "status", workingDirectory,
            OutputListener, Form1.ExitListener);
        return;

        void OutputListener(string prefix, string? line)
        {
            if (line == null || line.StartsWith("  (use \"git"))
            {
                return;
            }
            line = line.Replace("\t", "    ");
            Form1.OutputListener(prefix, line);
        }
    }

    public static void UnityBuild(string workingDirectory)
    {
        const string outPrefix = "unity";
        const string batchFile = "__local_Build_Win64.bat";
        workingDirectory = Path.Combine(workingDirectory, "etc", "batchBuild");
        var path = Path.Combine(workingDirectory, batchFile);
        if (!File.Exists(path))
        {
            Form1.AddLine(outPrefix, $"command not found: {path}");
            return;
        }
        Form1.AddLine($">{outPrefix}", $"start {batchFile}");
        RunCommand.Execute(outPrefix, "cmd.exe", $"/C {batchFile}", workingDirectory,
            Form1.OutputListener, Form1.ExitListener);
    }
}
