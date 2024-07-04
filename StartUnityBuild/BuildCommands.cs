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
            var buildTarget = settings.BuildTargets[0];
            Form1.AddLine($">{outPrefix}", $"build {buildTarget}");
            var executable = settings.UnityExecutable;
            var projectPath = Quoted(@".\");
            var prevLogFile = Quoted(@$".\etc\_local_build_{buildTarget}.prev.log");
            var tempLogFile = Quoted(@$".\etc\_local_build_{buildTarget}.temp.log");
            var buildLogFile = Quoted(@$".\Assets\BuildReports\{buildTarget}.build.log.txt");
            if (File.Exists(tempLogFile))
            {
                Logger.Trace($"File.Move({tempLogFile}, {prevLogFile}, overwrite: true);");
                File.Move(tempLogFile, prevLogFile, overwrite: true);
            }
            if (File.Exists(buildLogFile))
            {
                Logger.Trace($"File.Delete({buildLogFile});");
                File.Delete(buildLogFile);
            }
            var arguments =
                $" -buildTarget {buildTarget} -projectPath {projectPath}" +
                $" -logFile {tempLogFile}" +
                $" -executeMethod PrgBuild.Build.BuildPlayer -quit -batchmode";
            Form1.AddLine($".{outPrefix}", $"executable: {executable}");
            Form1.AddLine($".{outPrefix}", $"arguments: {arguments}");
            var result = await RunCommand.Execute(outPrefix, executable, arguments,
                settings.WorkingDirectory, null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddLine($">{outPrefix}", $"return code {result}");
            if (File.Exists(tempLogFile))
            {
                Logger.Trace($"File.Move({tempLogFile}, {buildLogFile}, overwrite: true);");
                File.Move(tempLogFile, buildLogFile, overwrite: true);
            }
            finished();
        });
    }
}
