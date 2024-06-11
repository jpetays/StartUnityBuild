namespace StartUnityBuild;

public static class Commands
{
    public static void GitStatus(string workingDirectory)
    {
        const string outPrefix = "git";
        Form1.AddLine($">{outPrefix}", "status");
        RunCommand.Execute(outPrefix, "git", "status", workingDirectory, OutputListener, Form1.ExitListener);
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
        Form1.AddLine($">{outPrefix}", "build");
        RunCommand.Execute(outPrefix, "cmd", "/C dir", workingDirectory, Form1.OutputListener, Form1.ExitListener);
    }
}
