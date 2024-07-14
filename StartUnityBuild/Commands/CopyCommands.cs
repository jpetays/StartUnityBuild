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
            Form1.AddLine("ERROR", $"can not copy, source directory not found or is empty: {sourceDir}");
            finished();
            return;
        }
        const string outPrefix = "copy";
        Task.Run(async () =>
        {
            const string workingDirectory = ".";
            const string copyCommand = "robocopy";
            var copyOptions = $"{Files.Quoted(sourceDir)} {Files.Quoted(targetDir)} *.* /S /E /V /PURGE /NP";
            if (Args.Instance.IsTesting)
            {
                copyOptions = $"{copyOptions} /L";
            }
            Form1.AddLine($">{outPrefix}", $"{copyCommand} {copyOptions}");
            var result = await RunCommand.Execute(outPrefix, $"{copyCommand}.exe", copyOptions, workingDirectory,
                null, OutputListenerFilter, Form1.ExitListener);
            var isSuccess = result is >= 0 and <= 1;
            if (!isSuccess)
            {
                Form1.AddLine(outPrefix,"-Robocopy reported that copy was not perfect, check the output for possible problems");
            }
            Form1.AddExitCode(outPrefix, result, isSuccess, showSuccess: true);
            finished();
        });
        return;

        void OutputListenerFilter(string prefix, string line)
        {
            var c1 = line[0];
            if (c1 == 255)
            {
                return;
            }
            Form1.OutputListener(prefix, line.Replace('\t', ' '));
        }
    }
}
