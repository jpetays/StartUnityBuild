using System.Diagnostics.CodeAnalysis;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
[SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        var projectDirectory = ParseArgs(out var args);
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
        Application.Run(new Form1(args.isTesting));
    }

    private class Args
    {
        public bool isTesting;
    }

    private static string ParseArgs(out Args args)
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
                args.isTesting = true;
            }
        }
        return currentDirectory;
    }
}
