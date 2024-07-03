using NLog;

namespace StartUnityBuild;

public static class BuildCommands
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static string Quoted(string path) => path.Contains(' ') ? $"\"{path}\"" : path;

    public static void BuildPlayer(BuildSettings settings, Action finished)
    {
        const string outPrefix = "build";
        Task.Run(async () =>
        {
            var buildTarget = settings.BuildTargets[0];
            Form1.AddLine($">{outPrefix}", $"build {buildTarget}");
            var executable = settings.UnityExecutable;
            var projectPath = Quoted(@".\");
            var logFile = Quoted(@$".\etc\_local_AutoBuild_{buildTarget}.log");
            var arguments =
                $" -buildTarget {buildTarget} -projectPath {projectPath}" +
                $" -logFile {logFile} -timestamps" +
                $" -executeMethod PrgBuild.Build.BuildPlayer -quit -batchmode";
            Form1.AddLine($".{outPrefix}", $"executable: {executable}");
            Form1.AddLine($".{outPrefix}", $"arguments: {arguments}");
            var result = await RunCommand.Execute(outPrefix, executable, arguments,
                settings.WorkingDirectory, null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddLine($">{outPrefix}", $"return code {result}");
            finished();
        });
    }
}
