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

        void GitStatusFilter(string prefix, string line)
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

        void GitLogFilter(string prefix, string line)
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
            // Using --tags might be overkill but
            // it should ensure that git push will never fail due to tag conflict with remote.
            const string gitCommand = "pull --rebase=true --tags --no-autostash origin main";
            Form1.AddLine($">{outPrefix}", $"git {gitCommand}");
            var result = await RunCommand.Execute(outPrefix, "git", gitCommand, workingDirectory,
                null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddExitCode(outPrefix, result, result == 0, showSuccess: true);
            finished();
        });
    }

    public static void GitCommitAndPushWithTag(BuildSettings settings, Action finished)
    {
        Task.Run(async () =>
        {
            var message = $"auto update version {settings.ProductVersion}";
            var files = new List<string>
            {
                "ProjectSettings/ProjectSettings.asset",
                BuildInfoUpdater.GetGFitPath(settings.WorkingDirectory, settings.BuildInfoFilename),
            };
            var gitVerb = "commit";
            var gitCommand = $"""{gitVerb} -m "{message}" {string.Join(' ', files)}""";
            Form1.AddLine($">{gitVerb}", $"git {gitCommand}");
            var result = await RunCommand.Execute(gitVerb, "git", gitCommand, settings.WorkingDirectory,
                null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddExitCode(gitVerb, result, result == 0, showSuccess: false);
            if (result != 0)
            {
                Form1.AddLine($"{gitVerb}", $"-Commit failed, can not push anything");
                finished();
                return;
            }

            // Create a lightweight commit tag, --force will 'move' it 'upwards' if it exists already.
            gitVerb = "tag";
            var bundle = settings.HasProductVersionBundle() ? "" : $"_bundle_{settings.BundleVersion}";
            var track = string.IsNullOrWhiteSpace(settings.DeliveryTrack) ? "" : $"_{settings.DeliveryTrack}";
            var tagName = $"auto_build_{DateTime.Today:yyyy-MM-dd}_version_{settings.ProductVersion}{bundle}{track}"
                .ToLowerInvariant();
            gitCommand = $"{gitVerb} --force {tagName}";
            Form1.AddLine($">{gitVerb}", $"git {gitCommand}");
            result = await RunCommand.Execute(gitVerb, "git", gitCommand, settings.WorkingDirectory,
                null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddExitCode(gitVerb, result, result == 0, showSuccess: false);

            // First push actual changes to make sure they get pushes ok.
            gitVerb = "push";
            var options = Args.Instance.IsTesting ? "--dry-run" : "";
            gitCommand = $"{gitVerb} {options} origin main";
            Form1.AddLine($">{gitVerb}", $"git {gitCommand}");
            result = await RunCommand.Execute(gitVerb, "git", gitCommand, settings.WorkingDirectory,
                null, Form1.OutputListener, Form1.ExitListener);
            if (result == 0)
            {
                // Push tag if git push is ok.
                gitCommand = $"{gitVerb} {options} --tags origin main";
                Form1.AddLine($">{gitVerb}", $"git {gitCommand}");
                var result2 = await RunCommand.Execute(gitVerb, "git", gitCommand, settings.WorkingDirectory,
                    null, Form1.OutputListener, Form1.ExitListener);
                if (result2 != 0)
                {
                    Form1.AddLine($"{gitVerb}", $"-Failed to push build tag, everything else should be ok");
                }
            }
            Form1.AddExitCode(gitVerb, result, result == 0, showSuccess: true);
            if (Args.Instance.IsTesting)
            {
                Form1.AddLine($"{gitVerb}", $"-You should reset git 'working tree' to state before changes:");
                Form1.AddLine($"{gitVerb}", $"git reset --hard HEAD~1");
                Form1.AddLine($"{gitVerb}", $"-You should also manually delete build tag before it gets pushed:");
                Form1.AddLine($"{gitVerb}", $"git tag -d {tagName}");
            }
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
