namespace StartUnityBuild;

/// <summary>
/// Utilities for project and build system specific files.
/// </summary>
public static class Files
{
    public const string ProjectSettingsFolderName = "ProjectSettings";
    public const string ProjectSettingsFileName = "ProjectSettings.asset";
    private const string ProjectVersionFileName = "ProjectVersion.txt";
    private static readonly string AutoBuildEnvironmentFilePath = Path.Combine("etc", "batchBuild", "_auto_build.env");

    public static void LoadProjectVersionFile(string workingDirectory, ref string unityVersion)
    {
        var path = Path.Combine(workingDirectory, ProjectSettingsFolderName, ProjectVersionFileName);
        Form1.AddLine(".file", $"{path}");
        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            var tokens = line.Split(':');
            if (tokens[0].Trim() == "m_EditorVersion")
            {
                unityVersion = tokens[1].Trim();
                return;
            }
        }
    }

    public static void LoadAutoBuildTargets(string workingDirectory, ref string unityPath, List<string> buildTargets)
    {
        var path = Path.Combine(workingDirectory, AutoBuildEnvironmentFilePath);
        Form1.AddLine(".file", $"{path}");
        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            var tokens = line.Split('=');
            switch (tokens[0].Trim())
            {
                case "buildTargets":
                {
                    var targets = tokens[1].Split(',');
                    foreach (var target in targets)
                    {
                        buildTargets.Add(target.Trim());
                    }
                    break;
                }
                case "unityPath":
                    unityPath = tokens[1].Trim();
                    break;
            }
        }
    }
}

public static class ProjectSettings
{
    public static void LoadProjectSettingsFile(string workingDirectory,
        ref string productName, ref string productVersion, ref string bundleVersion)
    {
        var path = Path.Combine(workingDirectory, Files.ProjectSettingsFolderName, Files.ProjectSettingsFileName);
        Form1.AddLine(".file", $"{path}");
        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            var tokens = line.Split(':');
            if (tokens[0] == "  productName")
            {
                productName = tokens[1].Trim();
            }
            else if (tokens[0] == "  bundleVersion")
            {
                productVersion = tokens[1].Trim();
            }
            else if (tokens[0] == "  AndroidBundleVersionCode")
            {
                bundleVersion = tokens[1].Trim();
            }
        }
    }

    public static bool UpdateProjectSettingsFile(string workingDirectory,
        ref string productVersion, ref string bundleVersion, bool versionIsDate = true)

    {
        var path = Path.Combine(workingDirectory, Files.ProjectSettingsFolderName, Files.ProjectSettingsFileName);
        var lines = File.ReadAllLines(path);
        var curProductVersion = "";
        var curBundleVersion = "";
        foreach (var line in lines)
        {
            var tokens = line.Split(':');
            if (tokens[0] == "  bundleVersion")
            {
                curProductVersion = tokens[1].Trim();
            }
            else if (tokens[0] == "  AndroidBundleVersionCode")
            {
                curBundleVersion = tokens[1].Trim();
            }
        }
        if (curProductVersion == "" || curBundleVersion == "")
        {
            Form1.AddLine("ERROR", $"Could not find 'version' or 'bundle' from {path}");
            return false;
        }
        if (curProductVersion != productVersion || curBundleVersion != bundleVersion)
        {
            Form1.AddLine("ERROR",
                $"ProjectSettings.asset does not have 'version' {productVersion} or 'bundle' {bundleVersion}");
            Form1.AddLine(".ERROR", $"Current values are 'version' {curProductVersion} or 'bundle' {curBundleVersion}");
            return false;
        }
        if (versionIsDate)
        {
            curProductVersion = $"{DateTime.Today:dd.MM.yyyy}";
        }
        var bundleVersionValue = int.Parse(curBundleVersion) + 1;

        productVersion = curProductVersion;
        bundleVersion = bundleVersionValue.ToString();
        var updateCount = 0;
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = lines[i];
            var tokens = line.Split(':');
            if (tokens[0] == "  bundleVersion")
            {
                lines[i] = $"  bundleVersion: {productVersion}";
                updateCount += 1;
            }
            else if (tokens[0] == "  AndroidBundleVersionCode")
            {
                lines[i] = $"  AndroidBundleVersionCode: {bundleVersion}";
                updateCount += 1;
            }
        }
        if (updateCount != 2)
        {
            Form1.AddLine("ERROR", $"Unable to update 'version' or 'bundle' in {path}");
            return false;
        }
        var output = string.Join('\n', lines);
        File.WriteAllText(path, output);
        return true;
    }
}
