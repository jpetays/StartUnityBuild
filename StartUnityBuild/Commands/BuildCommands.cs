using NLog;
using PrgFrame.Util;

namespace StartUnityBuild.Commands;

/// <summary>
/// Commands to build UNITY player.
/// </summary>
public static class BuildCommands
{
    /// <summary>
    /// Calls UNITY side "build command" whose output is:<br />
    /// - final binary if successful in output folder
    /// - Win64.build.buildreport (UNITY BuildPipeline.BuildPlayer Build Report asset)
    /// - Win64.build.log.txt (copy of UNITY build log file created during build)
    /// </summary>
    public static void BuildPlayer(BuildSettings settings, Action<bool> finished)
    {
        // For quick testing use: Editor.Demo.BuildTest.TestBuild
        const string executeMethod = "PrgBuild.Build.BuildPlayer";
        const int successReturn = 0;
        const int buildFailureReturn = 1;
        const int startFailureReturn = 2;
        const int testSuccessReturn = 10;
        const string outPrefix = "build";
        Task.Run(async () =>
        {
            var success = false;
            for (var i = 0; i < settings.BuildTargets.Count; ++i)
            {
                if (i > 0)
                {
                    // Give some time for all background processes to shutdown in UNITY build infra.
                    Thread.Sleep(3000);
                }
                var buildTarget = settings.BuildTargets[i];
                success = await BuildTarget(buildTarget);
                settings.BuildResult[i] = success;
                if (!success)
                {
                    break;
                }
            }
            finished(success);
            return;

            async Task<bool> BuildTarget(string buildTarget)
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
                    $" -executeMethod {executeMethod} -quit -batchmode -semVer {PrgBuild.Info.SemVer}";
                if (buildTarget == BuildName.Android)
                {
                    arguments = $"{arguments} -android {settings.AndroidSettingsFileName}";
                }
                Form1.AddLine($".{outPrefix}", $"executable: {executable}");
                Form1.AddLine($".{outPrefix}", $"arguments: {arguments}");
                var result = await RunCommand.Execute(outPrefix, executable, arguments,
                    settings.WorkingDirectory, null, Form1.OutputListener, Form1.ExitListener);
                var isSuccess = result is successReturn or testSuccessReturn;
                if (!isSuccess)
                {
                    switch (result)
                    {
                        case buildFailureReturn:
                            Form1.AddLine(outPrefix, $"-Build system reported: build failed");
                            break;
                        case startFailureReturn:
                            Form1.AddLine(outPrefix,
                                $"-Build system reported: invalid arguments or error starting build");
                            break;
                    }
                }
                Form1.AddExitCode(outPrefix, result, isSuccess, showSuccess: true);
                HandleOutputFiles();
                return isSuccess;

                #region Build Output File handling

                void PrepareFilesForBuild()
                {
                    if (File.Exists(buildLogFile))
                    {
                        Form1.AddLine($".file", $"Move from {buildLogFile} to {prevLogFile}");
                        File.Move(buildLogFile, prevLogFile, overwrite: true);
                    }
                    Form1.AddLine($".file", $"Truncate {Path.GetFullPath(buildLogFile)}");
                    File.WriteAllText(buildLogFile, "");

                    Form1.AddLine($".file", $"Truncate {Path.GetFullPath(outputLogFile)}");
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
                    try
                    {
                        Form1.AddLine($".file", $"Copy from {buildLogFile} to {outputLogFile}");
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
        var content = metaFileContent
            .Replace("142774e6a1509ef46ba1d3ba55c8e8ce", randomGuid)
            .Replace('_', ' ')
            .Replace("\r", "");
        File.WriteAllText(metaFilename, content, Files.Encoding);
    }
}
