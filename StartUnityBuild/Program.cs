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
        var initialDirectory = Directory.GetCurrentDirectory();
        var appPropertiesName = $"{Application.ProductName!}.properties";
        var appPropertiesFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName!);
        var appPropertiesFile = Path.Combine(appPropertiesFolder, appPropertiesName);
        var currentDirectory = File.Exists(appPropertiesFile)
            ? File.ReadAllText(appPropertiesFile, Files.Encoding)
            : initialDirectory;
        if (!Files.HasProjectVersionFile(currentDirectory))
        {
            if (GetProjectSettingsFolderName(initialDirectory, out currentDirectory))
            {
                if (!Directory.Exists(appPropertiesFolder))
                {
                    Directory.CreateDirectory(appPropertiesFolder);
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
