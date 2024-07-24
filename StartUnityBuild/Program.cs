using System.Diagnostics.CodeAnalysis;
using NLog;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
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
        var projectDirectory = Args.ParseArgs();
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

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public class Args
{
    public bool IsTesting { get; private set; }

    public static Args Instance;

    public static string ParseArgs()
    {
        var isTesting = "--isTesting".ToLowerInvariant();

        var argsList = Environment.GetCommandLineArgs().ToList();
        argsList.RemoveAt(0);
        var currentDirectory = Directory.GetCurrentDirectory();
        Instance = new Args();
        using var enumerator = argsList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var value = enumerator.Current?.ToLowerInvariant();
            if (value == isTesting)
            {
                Instance.IsTesting = true;
            }
        }
        return currentDirectory;
    }
}
