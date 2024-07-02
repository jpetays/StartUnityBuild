using NLog;

namespace StartUnityBuild;

public class BuildCommands
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static void BuildPlayer(BuildSettings settings, Action finished)
    {
        const string outPrefix = "build";
        Task.Run(async () =>
        {
            var buildTarget = settings.BuildTargets[0];
            var logFile = @$".\etc\_local_AutoBuild_{buildTarget}.log";
            Form1.AddLine($">{outPrefix}", $"build {buildTarget}");
            var executable = settings.UnityExecutable;
            var arguments =
                $"-executeMethod PrgBuild.Build.BuildPlayer -quit -batchmode" +
                $"-projectPath .\\ -buildTarget {buildTarget} -logFile \"{logFile}\"";
            Form1.AddLine($".{outPrefix}", $"executable: {executable}");
            Form1.AddLine($".{outPrefix}", $"arguments: {arguments}");
            var result = await RunCommand.Execute(outPrefix, executable, arguments,
                settings.WorkingDirectory, null, Form1.OutputListener, Form1.ExitListener);
            Form1.AddLine($">{outPrefix}", $"return code {result}");
            finished();
        });
    }
}
