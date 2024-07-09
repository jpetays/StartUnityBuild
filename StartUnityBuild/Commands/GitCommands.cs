namespace StartUnityBuild.Commands;

/// <summary>
/// Commands for git operations.
/// </summary>
public static class GitCommands
{
    public static void GitStatus(string workingDirectory, Action finished)
    {
        const string outPrefix = "status";
        Task.Run(async () =>
        {
            const string gitCommand = "status";
            Form1.AddLine($">{outPrefix}", $"git {gitCommand}");
            await RunCommand.Execute(outPrefix, "git", gitCommand, workingDirectory,
                null, GitOutputFilter, Form1.ExitListener);
            finished();
        });
    }

    private static void GitOutputFilter(string prefix, string? line)
    {
        if (line == null || line.StartsWith("  (use \"git"))
        {
            return;
        }
        Form1.OutputListener(prefix, line);
    }
}
