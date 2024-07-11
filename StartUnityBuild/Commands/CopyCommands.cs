namespace StartUnityBuild.Commands;

/// <summary>
/// Commands for file copy operations.
/// </summary>
public static class CopyCommands
{
    public static void CopyDirectories(string sourceDir, string targetDir, Action finished)
    {
        if (!Directory.Exists(sourceDir))
        {
            Form1.AddLine("ERROR", $"source directory not found: {sourceDir}");
            finished();
            return;
        }
        const string outPrefix = "copy";
        Task.Run(async () =>
        {
            const string workingDirectory = ".";
            const string copyCommand = "robocopy";
            var copyOptions = $"{Files.Quoted(sourceDir)} {Files.Quoted(targetDir)} *.* /S /E /V /PURGE";
            if (Args.Instance.IsTesting)
            {
                copyOptions = $"{copyOptions} /L";
            }
            Form1.AddLine($">{outPrefix}", $"{copyCommand} {copyOptions}");
            await RunCommand.Execute(outPrefix, $"{copyCommand}.exe", copyOptions, workingDirectory,
                null, Form1.OutputListener, Form1.ExitListener);
            finished();
        });
    }
}
