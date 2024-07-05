using System.Diagnostics.CodeAnalysis;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
[SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
static class Program
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
        var projectDirectory = ParseArgs();
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
        Application.Run(new Form1());
    }

    private static string ParseArgs()
    {
        var verDate = "--verDate".ToLower();
        var semVer = "--semVer".ToLower();
        var dryRun = "--dryRun".ToLower();
        var projectFolder = "--projectFolder".ToLower();

        var args = Environment.GetCommandLineArgs().ToList();
        args.RemoveAt(0);
        var currentDirectory = Directory.GetCurrentDirectory();
        using var enumerator = args.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var value = enumerator.Current.ToLower();
            if (value == verDate)
            {
                continue;
            }
            if (value == semVer)
            {
                continue;
            }
            if (value == dryRun)
            {
                Commands.IsDryRun = true;
                continue;
            }
            if (value == projectFolder)
            {
                if (!enumerator.MoveNext())
                {
                    throw new ApplicationException($"required parameter after '{projectFolder}' is missing");
                }
                value = enumerator.Current;
                if (!Directory.Exists(value))
                {
                    throw new ApplicationException($"'{projectFolder}' directory not found: {value}");
                }
                currentDirectory = value;
            }
        }
        return currentDirectory;
    }
}
