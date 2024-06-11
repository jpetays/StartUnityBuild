namespace StartUnityBuild;

public static class Commands
{
    public static void GitStatus()
    {
        new RunCommand("git", "git", "status", OutputListener, Form1.ExitListener).Execute();
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
}
