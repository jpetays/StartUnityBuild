using PrgBuild;

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
            var result = await RunCommand.Execute(outPrefix, "git", gitCommand, workingDirectory,
                null, GitStatusFilter, Form1.ExitListener);
            Form1.AddExitCode(outPrefix, result, result == 0, showSuccess: false);
            gitCommand = "log --pretty=oneline origin/main..HEAD";
            Form1.AddLine($">{outPrefix}", $"git {gitCommand}");
            result = await RunCommand.Execute(outPrefix, "git", gitCommand, workingDirectory,
                null, GitLogFilter, Form1.ExitListener);
            Form1.AddExitCode(outPrefix, result, result == 0, showSuccess: false);
            finished();
        });
        return;

        void GitStatusFilter(string prefix, string? line)
        {
            if (line == null)
            {
                return;
            }
            if (line.StartsWith("   "))
            {
                Form1.OutputListener(prefix, line);
                return;
            }
            if (line.StartsWith(" M "))
            {
                Form1.OutputListener(prefix, $"-commit: {line}");
                return;
            }
            if (line.StartsWith("?? "))
            {
                Form1.OutputListener(prefix, $"-untracked: {line}");
                return;
            }
            Form1.OutputListener(prefix, $"-status: {line}");
        }

        void GitLogFilter(string prefix, string? line)
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

    public static void GitPull(string workingDirectory, Action finished)
    {
        const string outPrefix = "pull";
        Task.Run(async () =>
        {
            var gitCommand = "pull --rebase=true --no-autostash origin main";
            Form1.AddLine($">{outPrefix}", $"git {gitCommand}");
            var result = await RunCommand.Execute(outPrefix, "git", gitCommand, workingDirectory,
                null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddExitCode(outPrefix, result, result == 0, showSuccess: true);
            finished();
        });
    }

    public static void GitCommitAnPushWithLabel(BuildSettings settings, string options, Action finished)
    {
        Task.Run(async () =>
        {
            var message = $"auto update version {settings.ProductVersion}";
            var tagName = $"auto_build_{DateTime.Today:yyyy-MM-dd}_version_{settings.ProductVersion}";
            var files = new List<string>
            {
                "ProjectSettings/ProjectSettings.asset",
                BuildInfoUpdater.GetGFitPath(settings.WorkingDirectory, settings.BuildInfoFilename),
            };
            var getVerb = "commit";
            var gitCommand = $"""{getVerb} -m "{message}" {string.Join(' ', files)}""";
            Form1.AddLine($">{getVerb}", $"git {gitCommand}");
            var result = await RunCommand.Execute(getVerb, "git", gitCommand, settings.WorkingDirectory,
                null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddExitCode(getVerb, result, result == 0, showSuccess: false);

            // Create a lightweight commit tag.
            getVerb = "tag";
            gitCommand = $"{getVerb} -f {tagName}";
            Form1.AddLine($">{getVerb}", $"git {gitCommand}");
            result = await RunCommand.Execute(getVerb, "git", gitCommand, settings.WorkingDirectory,
                null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddExitCode(getVerb, result, result == 0, showSuccess: false);

            getVerb = "push";
            gitCommand = $"{getVerb} {options} origin main";
            Form1.AddLine($">{getVerb}", $"git {gitCommand}");
            result = await RunCommand.Execute(getVerb, "git", gitCommand, settings.WorkingDirectory,
                null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddExitCode(getVerb, result, result == 0, showSuccess: true);
            finished();
        });
    }

    public static void GitRevert(string workingDirectory, List<string> files, Action finished)
    {
        if (files.Count == 0)
        {
            finished();
            return;
        }
        const string outPrefix = "revert";
        Task.Run(async () =>
        {
            var gitCommand = $"checkout --force -- {string.Join(' ', files)}";
            Form1.AddLine($">{outPrefix}", $"git {gitCommand}");
            var result = await RunCommand.Execute(outPrefix, "git", gitCommand, workingDirectory,
                null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddExitCode(outPrefix, result, result == 0, showSuccess: true);
            finished();
        });
    }
}
