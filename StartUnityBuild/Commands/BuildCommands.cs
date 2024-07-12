using NLog;
using PrgFrame.Util;

namespace StartUnityBuild.Commands;

/// <summary>
/// Commands to build UNITY player.
/// </summary>
public static class BuildCommands
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Calls UNITY side "build command" whose output is:<br />
    /// - final binary if successful in output folder
    /// - Win64.build.buildreport (UNITY BuildPipeline.BuildPlayer Build Report asset)
    /// - Win64.build.log.txt (copy of UNITY build log file created during build)
    /// </summary>
    public static void BuildPlayer(BuildSettings settings, Action finished)
    {
        // For quick testing use: Editor.Demo.BuildTest.TestBuild
        const string executeMethod = "PrgBuild.Build.BuildPlayer";
        const string outPrefix = "build";
        Task.Run(async () =>
        {
            foreach (var buildTarget in settings.BuildTargets)
            {
                await BuildTarget(buildTarget);
            }
            finished();
            return;

            async Task BuildTarget(string buildTarget)
            {
                Form1.AddLine($">{outPrefix}", $"build {buildTarget}");
                var executable = settings.UnityExecutable;
                var projectPath = Directory.GetCurrentDirectory() == settings.WorkingDirectory
                    ? ".\\"
                    : settings.WorkingDirectory;
                var prevLogFile = @$".\etc\_local_build_{buildTarget}.prev.log";
                var buildLogFile = @$".\etc\_local_build_{buildTarget}.build.log";
                var outputLogFile = @$".\Assets\BuildReports\{buildTarget}.build.log.txt";
                var outputLogMetafile = $"{outputLogFile}.meta";
                PrepareFilesForBuild();
                var arguments =
                    $" -buildTarget {buildTarget} -projectPath {Files.Quoted(projectPath)}" +
                    $" -logFile {Files.Quoted(buildLogFile)}" +
                    $" -executeMethod {executeMethod} -quit -batchmode";
                Form1.AddLine($".{outPrefix}", $"executable: {executable}");
                Form1.AddLine($".{outPrefix}", $"arguments: {arguments}");
                var result = await RunCommand.Execute(outPrefix, executable, arguments,
                    settings.WorkingDirectory, null, Form1.OutputListener, Form1.ExitListener);
                Form1.AddLine($">{outPrefix}", $"return code {result}");
                if (result != 0)
                {
                    Form1.AddLine(outPrefix, $"- return code was not zero!");
                }
                HandleOutputFiles();
                return;

                #region Build Output File handling

                void PrepareFilesForBuild()
                {
                    if (File.Exists(buildLogFile))
                    {
                        Logger.Trace($"File.Move({buildLogFile}, {prevLogFile}, overwrite: true);");
                        File.Move(buildLogFile, prevLogFile, overwrite: true);
                    }
                    Logger.Trace($"File.Truncate({Path.GetFullPath(buildLogFile)});");
                    File.WriteAllText(buildLogFile, "");

                    Logger.Trace($"File.Truncate({Path.GetFullPath(outputLogFile)});");
                    File.WriteAllText(outputLogFile, "");
                    if (!File.Exists(outputLogMetafile))
                    {
                        CreateUnityMetafile(outputLogMetafile);
                    }
                }

                void HandleOutputFiles()
                {
                    if (!File.Exists(buildLogFile))
                    {
                        return;
                    }
                    Logger.Trace($"File.Copy({buildLogFile}, {outputLogFile}, overwrite: true);");
                    try
                    {
                        File.Copy(buildLogFile, outputLogFile, overwrite: true);
                    }
                    catch (Exception x)
                    {
                        Form1.AddLine($">{outPrefix}", $"File.Copy failed: {x.GetType().Name} {x.Message}");
                    }
                }

                #endregion
            }
        });
    }

    private static void CreateUnityMetafile(string metaFilename)
    {
        // Underscore '_' is placeholder for space in the raw string so trailing spaces are not stripped away!
        const string metaFileContent = """
                                       fileFormatVersion: 2
                                       guid: 142774e6a1509ef46ba1d3ba55c8e8ce
                                       TextScriptImporter:
                                         externalObjects: {}
                                         userData:_
                                         assetBundleName:_
                                         assetBundleVariant:_

                                       """;
        var ticks = RandomUtil.StringFromTicks(6);
        var randomGuid = $"e6a1509ef46ba1d3ba55c8e8ce{ticks}";
        File.WriteAllText(metaFilename, metaFileContent
            .Replace("142774e6a1509ef46ba1d3ba55c8e8ce", randomGuid)
            .Replace('_', ' ')
            .Replace("\r", ""));
    }
}
