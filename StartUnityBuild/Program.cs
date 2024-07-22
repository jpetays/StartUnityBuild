using System.Diagnostics.CodeAnalysis;
using NLog;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
[SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
internal static class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        try
        {
            SetWorkingDirectory();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to set working directory");
            Logger.Trace(e.StackTrace);
        }
        Application.Run(new Form1());
    }

    private static void SetWorkingDirectory()
    {
        var projectDirectory = Args.ParseArgs(out var args);
        if (Directory.Exists(projectDirectory) && Files.HasProjectVersionFile(projectDirectory))
        {
            Directory.SetCurrentDirectory(projectDirectory);
            AppSettings.SetUnityProjectFolder(projectDirectory);
        }
        else
        {
            projectDirectory = AppSettings.Get().UnityProjectFolder;
            if (Directory.Exists(projectDirectory) && Files.HasProjectVersionFile(projectDirectory))
            {
                Directory.SetCurrentDirectory(projectDirectory);
            }
        }
    }
}

public class Args
{
    public bool IsTesting { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static Args Instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private Args()
    {
        Instance = this;
    }

    public static string ParseArgs(out Args args)
    {
        var isTesting = "--isTesting".ToLower();

        var argsList = Environment.GetCommandLineArgs().ToList();
        argsList.RemoveAt(0);
        var currentDirectory = Directory.GetCurrentDirectory();
        args = new Args();
        using var enumerator = argsList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var value = enumerator.Current.ToLower();
            if (value == isTesting)
            {
                args.IsTesting = true;
            }
        }
        return currentDirectory;
    }
}
