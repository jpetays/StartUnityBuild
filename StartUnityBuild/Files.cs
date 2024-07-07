using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace StartUnityBuild;

/// <summary>
/// Utilities for project and build system specific files.
/// </summary>
public static class Files
{
    public static readonly Encoding Encoding = new UTF8Encoding(false, false);

    public const string ProjectSettingsFolderName = "ProjectSettings";
    public const string ProjectSettingsFileName = "ProjectSettings.asset";
    public const string ProjectVersionFileName = "ProjectVersion.txt";
    private static readonly string AutoBuildEnvironmentFilePath = Path.Combine("etc", "batchBuild", "_auto_build.env");

    public static string GetAssetFolder(string workingDirectory) => Path.Combine(workingDirectory, "Assets");

    public static bool HasProjectVersionFile(string workingDirectory)
    {
        var path = Path.Combine(workingDirectory, ProjectSettingsFolderName, ProjectVersionFileName);
        return File.Exists(path);
    }

    [SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
    public static void LoadProjectVersionFile(string workingDirectory, out string unityVersion)
    {
        unityVersion = null!;
        var path = Path.Combine(workingDirectory, ProjectSettingsFolderName, ProjectVersionFileName);
        Form1.AddLine(".file", $"{path}");
        var lines = File.ReadAllLines(path, Encoding);
        foreach (var line in lines)
        {
            var tokens = line.Split(':');
            if (tokens[0].Trim() == "m_EditorVersion")
            {
                unityVersion = tokens[1].Trim();
                return;
            }
        }
        if (unityVersion == null)
        {
            throw new InvalidOperationException($"unable to find 'unityVersion' from {path}");
        }
    }

    public static void UpdateAutoBuildSettings(BuildSettings settings)
    {
        var path = Path.Combine(settings.WorkingDirectory, AutoBuildEnvironmentFilePath);
        Form1.AddLine(".file", $"{path}");
        settings.BuildTargets.Clear();
        settings.CopyFiles.Clear();
        settings.RevertFiles.Clear();
        foreach (var line in File.ReadAllLines(path)
                     .Where(x => !(string.IsNullOrEmpty(x) || x.StartsWith('#')) && x.Contains('=')))
        {
            var tokens = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
            var key = tokens[0].Trim();
            var value = tokens[1].Trim();
            if (key == "buildTargets")
            {
                var targets = tokens[1].Split(',');
                foreach (var target in targets)
                {
                    settings.BuildTargets.Add(target.Trim());
                }
                continue;
            }
            if (key.StartsWith("copy.") && (key.EndsWith(".source") || key.EndsWith(".target")))
            {
                settings.CopyFiles.Add(key, value);
                continue;
            }
            if (key.StartsWith("revert.") && key.EndsWith(".file"))
            {
                settings.RevertFiles.Add(value);
            }
        }
    }

    [SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
    public static void LoadAutoBuildTargets(string workingDirectory, out string unityPath, List<string> buildTargets)
    {
        unityPath = null!;
        var path = Path.Combine(workingDirectory, AutoBuildEnvironmentFilePath);
        Form1.AddLine(".file", $"{path}");
        var lines = File.ReadAllLines(path, Encoding);
        foreach (var line in lines)
        {
            var tokens = line.Split('=');
            switch (tokens[0].Trim())
            {
                case "BuildTargets":
                case "buildTargets":
                {
                    var targets = tokens[1].Split(',');
                    foreach (var target in targets)
                    {
                        buildTargets.Add(target.Trim());
                    }
                    break;
                }
                case "UnityPath":
                case "unityPath":
                    unityPath = tokens[1].Trim();
                    break;
            }
        }
        if (unityPath == null)
        {
            throw new InvalidOperationException($"unable to find 'unityPath' from {path}");
        }
    }
}

public static class ProjectSettings
{
    [SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
    public static void LoadProjectSettingsFile(string workingDirectory,
        out string productName, out string productVersion, out string bundleVersion, out bool muteOtherAudioSources)
    {
        productName = null!;
        productVersion = null!;
        bundleVersion = null!;
        muteOtherAudioSources = false;
        var path = Path.Combine(workingDirectory, Files.ProjectSettingsFolderName, Files.ProjectSettingsFileName);
        Form1.AddLine(".file", $"{path}");
        var lines = File.ReadAllLines(path, Files.Encoding);
        foreach (var line in lines)
        {
            if (!line.Contains(':'))
            {
                continue;
            }
            var tokens = line.Split(':');
            switch (tokens[0])
            {
                case "  productName":
                    productName = tokens[1].Trim();
                    break;
                case "  bundleVersion":
                    productVersion = tokens[1].Trim();
                    break;
                case "  AndroidBundleVersionCode":
                    bundleVersion = tokens[1].Trim();
                    break;
                case "  muteOtherAudioSources":
                    muteOtherAudioSources = tokens[1].Trim() != "0";
                    break;
            }
        }
        if (productName == null || productVersion == null || bundleVersion == null)
        {
            throw new InvalidOperationException($"unable to find required field values from {path}");
        }
    }

    public static int UpdateProjectSettingsFile(string workingDirectory, string productVersion, string bundleVersion)
    {
        var path = Path.Combine(workingDirectory, Files.ProjectSettingsFolderName, Files.ProjectSettingsFileName);
        var lines = File.ReadAllLines(path, Files.Encoding);
        var updateCount = 0;
        var skipCount = 0;
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = lines[i];
            var tokens = line.Split(':');
            if (tokens[0] == "  bundleVersion")
            {
                var curProductVersion = tokens[1].Trim();
                if (curProductVersion != productVersion)
                {
                    lines[i] = $"  bundleVersion: {productVersion}";
                    updateCount += 1;
                }
                else
                {
                    skipCount += 1;
                }
            }
            else if (tokens[0] == "  AndroidBundleVersionCode")
            {
                var curBundleVersion = tokens[1].Trim();
                if (curBundleVersion != bundleVersion)
                {
                    lines[i] = $"  AndroidBundleVersionCode: {bundleVersion}";
                    updateCount += 1;
                }
                else
                {
                    skipCount += 1;
                }
            }
        }
        if (updateCount == 0)
        {
            return -1;
        }
        if (skipCount == 2)
        {
            return 0;
        }
        var output = string.Join('\n', lines);
        File.WriteAllText(path, output, Files.Encoding);
        return 1;
    }
}
