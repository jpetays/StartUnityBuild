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
        var appPropertiesFile = GetAppPropertiesFilename();
        var cmdLineDirectory = ParseArgs();
        var initialDirectory = Directory.GetCurrentDirectory();
        var currentDirectory =
            !string.IsNullOrWhiteSpace(cmdLineDirectory) && File.Exists(cmdLineDirectory) ? cmdLineDirectory
            : File.Exists(appPropertiesFile) ? File.ReadAllText(appPropertiesFile, Files.Encoding)
            : initialDirectory;
        if (!Files.HasProjectVersionFile(currentDirectory))
        {
            if (GetProjectSettingsFolderName(initialDirectory, out currentDirectory))
            {
                var appPropertiesFolder = Path.GetDirectoryName(appPropertiesFile);
                if (!Directory.Exists(appPropertiesFolder))
                {
                    Directory.CreateDirectory(appPropertiesFolder!);
                }
                File.WriteAllText(appPropertiesFile, currentDirectory, Files.Encoding);
            }
        }
        if (initialDirectory != currentDirectory)
        {
            Directory.SetCurrentDirectory(currentDirectory);
        }
        Application.Run(new Form1());
    }

    private static string ParseArgs()
    {
        var args = Environment.GetCommandLineArgs().ToList();
        args.RemoveAt(0);
        var currentDirectory = Directory.GetCurrentDirectory();
        using var enumerator = args.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var value = enumerator.Current;
            switch (value)
            {
                case "--dryRun":
                    Commands.IsDryRun = true;
                    continue;
                case "--project":
                {
                    if (enumerator.MoveNext())
                    {
                        value = enumerator.Current;
                        if (File.Exists(value))
                        {
                            currentDirectory = value;
                        }
                    }
                    break;
                }
            }
        }
        return currentDirectory;
    }

    private static string GetAppPropertiesFilename()
    {
        var appPropertiesName = $"{Application.ProductName!}.properties";
        var appPropertiesFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName!);
        var appPropertiesFile = Path.Combine(appPropertiesFolder, appPropertiesName);
        return appPropertiesFile;
    }

    public static void DeleteAppPropertiesFile()
    {
        var appPropertiesFile = GetAppPropertiesFilename();
        if (File.Exists(appPropertiesFile))
        {
            File.Delete(appPropertiesFile);
        }
    }

    private static bool GetProjectSettingsFolderName(string initialDirectory, out string folderName)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.InitialDirectory = initialDirectory;
            openFileDialog.Filter =
                $"{Files.ProjectVersionFileName}|{Files.ProjectVersionFileName}|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var directoryName = Path.GetDirectoryName(openFileDialog.FileName);
                if (Directory.Exists(directoryName))
                {
                    var parentDirectory = Directory.GetParent(directoryName);
                    if (parentDirectory != null)
                    {
                        folderName = parentDirectory.FullName;
                        return true;
                    }
                }
            }
        }
        folderName = "";
        return false;
    }
}
