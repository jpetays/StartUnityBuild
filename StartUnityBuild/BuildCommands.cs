using System.Windows.Forms.VisualStyles;
using NLog;

namespace StartUnityBuild;

public static class BuildCommands
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static string Quoted(string path) => path.Contains(' ') ? $"\"{path}\"" : path;

    /// <summary>
    /// Calls UNITY side "build command" whose output is:<br />
    /// - final binary if successful in output folder
    /// - Win64.build.buildreport (UNITY BuildPipeline.BuildPlayer Build Report asset)
    /// - Win64.build.log.txt (copy of UNITY build log file created during build)
    /// </summary>
    public static void BuildPlayer(BuildSettings settings, Action finished)
    {
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
                var projectPath = settings.WorkingDirectory;
                var prevLogFile = @$".\etc\_local_build_{buildTarget}.prev.log";
                var buildLogFile = @$".\etc\_local_build_{buildTarget}.temp.log";
                var outputLogFile = @$".\Assets\BuildReports\{buildTarget}.build.log.txt";
                var outputLogMetafile = $"{outputLogFile}.meta";
                PrepareFilesForBuild();
                var arguments =
                    $" -buildTarget {buildTarget} -projectPath {Quoted(projectPath)}" +
                    $" -logFile {Quoted(buildLogFile)}" +
                    $" -executeMethod PrgBuild.Build.BuildPlayer -quit -batchmode";
                Form1.AddLine($".{outPrefix}", $"executable: {executable}");
                Form1.AddLine($".{outPrefix}", $"arguments: {arguments}");
                var result = await RunCommand.Execute(outPrefix, executable, arguments,
                    settings.WorkingDirectory, null, Form1.OutputListener, Form1.ExitListener);
                Form1.AddLine($">{outPrefix}", $"return code {result}");
                HandleOutputFiles();
                return;

                #region File handling

                void PrepareFilesForBuild()
                {
                    if (File.Exists(buildLogFile))
                    {
                        Logger.Trace($"File.Move({buildLogFile}, {prevLogFile}, overwrite: true);");
                        File.Move(buildLogFile, prevLogFile, overwrite: true);
                    }
                    if (File.Exists(outputLogFile))
                    {
                        Logger.Trace($"File.Delete({outputLogFile});");
                        File.Delete(outputLogFile);
                        if (File.Exists(outputLogMetafile))
                        {
                            File.Delete(outputLogMetafile);
                        }
                    }
                }

                void HandleOutputFiles()
                {
                    if (!File.Exists(buildLogFile))
                    {
                        return;
                    }
                    Logger.Trace($"File.Move({buildLogFile}, {outputLogFile}, overwrite: true);");
                    try
                    {
                        File.Move(buildLogFile, outputLogFile, overwrite: true);
                        if (!File.Exists(outputLogMetafile))
                        {
                            CreateUnityMetafile(outputLogMetafile);
                        }
                    }
                    catch (Exception x)
                    {
                        Form1.AddLine($">{outPrefix}", $"File.Move failed: {x.GetType().Name} {x.Message}");
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
        var ticks = DateTime.Now.Ticks.ToString()[^6..];
        var randomGuid = $"{ticks}e6a1509ef46ba1d3ba55c8e8ce";
        File.WriteAllText(metaFilename, metaFileContent
            .Replace("142774e6a1509ef46ba1d3ba55c8e8ce", randomGuid)
            .Replace('_', ' ')
            .Replace("\r", ""));
    }
}
