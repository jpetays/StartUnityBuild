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
            var gitCommand = "status --porcelain=v1";
            Form1.AddLine($">{outPrefix}", $"git {gitCommand}");
            await RunCommand.Execute(outPrefix, "git", gitCommand, workingDirectory,
                null, GitStatusFilter, Form1.ExitListener);
            gitCommand = "log --pretty=oneline origin/main..HEAD";
            Form1.AddLine($">{outPrefix}", $"git {gitCommand}");
            await RunCommand.Execute(outPrefix, "git", gitCommand, workingDirectory,
                null, GitLogFilter, Form1.ExitListener);
            finished();
        });
    }

    private static void GitStatusFilter(string prefix, string? line)
    {
        if (line == null)
        {
            return;
        }
        Form1.OutputListener(prefix, line.StartsWith("   ") ? line : $"-commit: {line}");
    }

    private static void GitLogFilter(string prefix, string? line)
    {
        if (line == null)
        {
            return;
        }
        var tokens = line.Split(' ');
        if (tokens.Length >= 2 && tokens[0].Trim().Length == 40)
        {
            tokens[0] = "push:";
            line = $"-{string.Join(' ', tokens)}";
        }
        Form1.OutputListener(prefix, line);
    }
}
