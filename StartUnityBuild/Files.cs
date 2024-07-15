using System.Diagnostics.CodeAnalysis;
using System.Text;
using PrgFrame.Util;

namespace StartUnityBuild;

/// <summary>
/// Well known build names.
/// </summary>
public static class BuildName
{
    public const string Android = nameof(Android);
    public const string WebGL = nameof(WebGL);
}

/// <summary>
/// Utilities and constants for project and build system specific files.
/// </summary>
public static class Files
{
    public static readonly Encoding Encoding = new UTF8Encoding(false, false);

    private const string ProjectSettingsFolderName = "ProjectSettings";
    private const string AssetsFolderName = "Assets";
    private const string ResourcesFolderName = "Resources";
    private static readonly string AutoBuildFolderName = Path.Combine("etc", "batchBuild");
    private static readonly string SecretKeysFolderName = Path.Combine("etc", "secretKeys");

    private const string ProjectSettingsFileName = "ProjectSettings.asset";
    private const string ProjectVersionFileName = "ProjectVersion.txt";
    private const string ReleaseNotesFileName = "releasenotes.txt";
    private const string AndroidSettingsFileName = "AndroidOptions.txt";
    private const string AutoBuildFileName = "_auto_build.env";

    private const string UnityVersionName = "$UNITY_VERSION$";
    private const string BuildTargetName = "$BUILD_TARGET$";
    private const string UniqueNameName = "$UNIQUE_NAME$";

    public static string Quoted(string path) => path.Contains(' ') ? $"\"{path}\"" : path;

    public static string GetAutoBuildFileName(string workingDirectory) =>
        Path.Combine(workingDirectory, AutoBuildFolderName, AutoBuildFileName);

    public static string GetProjectSettingsFileName(string workingDirectory) =>
        Path.Combine(workingDirectory, ProjectSettingsFolderName, ProjectSettingsFileName);

    private static string GetProjectVersionFile(string workingDirectory) =>
        Path.Combine(workingDirectory, ProjectSettingsFolderName, ProjectVersionFileName);

    public static string GetReleaseNotesFileName(string workingDirectory) =>
        PathUtil.FindFile(GetAssetFolder(workingDirectory), Path.Combine(ResourcesFolderName, ReleaseNotesFileName));

    public static string GetAndroidSettingsFileName(string workingDirectory) =>
        Path.Combine(workingDirectory, SecretKeysFolderName, AndroidSettingsFileName);

    public static string GetAssetFolder(string workingDirectory) => Path.Combine(workingDirectory, AssetsFolderName);

    public static bool HasProjectVersionFile(string workingDirectory) =>
        File.Exists(GetProjectVersionFile(workingDirectory));

    public static string ExpandUnityPath(string path, string unityVersion) =>
        path.Replace(UnityVersionName, unityVersion);

    public static string ExpandUniqueName(string path, string uniqueName) =>
        path.Replace(UniqueNameName, uniqueName);

    [SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
    public static void LoadProjectVersionFile(string workingDirectory, out string unityVersion)
    {
        unityVersion = null!;
        var path = GetProjectVersionFile(workingDirectory);
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

    public static void LoadAutoBuildSettings(BuildSettings settings)
    {
        var path = GetAutoBuildFileName(settings.WorkingDirectory);
        Form1.AddLine(".file", $"{path}");
        settings.BuildTargets.Clear();
        settings.CopyFilesBefore.Clear();
        settings.RevertFilesAfter.Clear();
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
                    settings.BuildResult.Add(false);
                }
                continue;
            }
            if (key == "unityPath")
            {
                settings.UnityPath = tokens[1].Trim();
                continue;
            }
            if (key == "webgl.build.history.json")
            {
                settings.WebGlBuildHistoryJson = tokens[1].Trim();
                continue;
            }
            if (key.StartsWith("before.copy.") && (key.EndsWith(".source") || key.EndsWith(".target")))
            {
                settings.CopyFilesBefore.Add(key, value);
                continue;
            }
            if (key.StartsWith("after.revert.") && key.EndsWith(".file"))
            {
                settings.RevertFilesAfter.Add(value);
                continue;
            }
            if (key.StartsWith("after.copy.") && (key.EndsWith(".sourceDir") || key.EndsWith(".targetDir")))
            {
                settings.CopyDirectoriesAfter.Add(key, value);
            }
        }
    }

    public static List<Tuple<string, string>> GetCopyFiles(BuildSettings settings)
    {
        const int nPos = 2;
        var keys = settings.CopyFilesBefore.Keys.ToList();
        keys.Sort();
        var result = new List<Tuple<string, string>>();
        for (var i = 0; i < keys.Count; ++i)
        {
            var sourceKey = keys[i];
            if (!sourceKey.EndsWith(".source"))
            {
                throw new InvalidOperationException($"invalid format for copy.N.source key: {sourceKey}");
            }
            i += 1;
            var targetKey = keys[i];
            if (!targetKey.EndsWith(".target"))
            {
                throw new InvalidOperationException($"invalid format for copy.N.target key: {targetKey}");
            }
            var n1 = sourceKey.Split('.')[nPos];
            var n2 = targetKey.Split('.')[nPos];
            if (n1 != n2)
            {
                throw new InvalidOperationException($"invalid copy line keys: {sourceKey} vs {targetKey}");
            }
            var source = settings.CopyFilesBefore[sourceKey];
            var target = settings.CopyFilesBefore[targetKey];
            result.Add(new Tuple<string, string>(source, target));
        }
        return result;
    }

    public static List<Tuple<string, string>> GetCopyDirs(BuildSettings settings, string buildTarget)
    {
        const int nPos = 2;
        var buildTargetPart = $".{buildTarget}.";
        var keys = settings.CopyDirectoriesAfter.Keys.ToList();
        keys.Sort();
        var result = new List<Tuple<string, string>>();
        for (var i = 0; i < keys.Count; ++i)
        {
            var sourceKey = keys[i];
            if (!sourceKey.EndsWith(".sourceDir"))
            {
                throw new InvalidOperationException($"invalid format for copy.N.source key: {sourceKey}");
            }
            if (!sourceKey.Contains(buildTargetPart))
            {
                continue;
            }
            i += 1;
            var targetKey = keys[i];
            if (!targetKey.EndsWith(".targetDir"))
            {
                throw new InvalidOperationException($"invalid format for copy.N.target key: {targetKey}");
            }
            if (!targetKey.Contains(buildTargetPart))
            {
                continue;
            }
            var n1 = sourceKey.Split('.')[nPos];
            var n2 = targetKey.Split('.')[nPos];
            if (n1 != n2)
            {
                throw new InvalidOperationException($"invalid copy line keys: {sourceKey} vs {targetKey}");
            }
            var source = settings.CopyDirectoriesAfter[sourceKey].Replace(BuildTargetName, buildTarget);
            var target = settings.CopyDirectoriesAfter[targetKey].Replace(BuildTargetName, buildTarget);
            result.Add(new Tuple<string, string>(source, target));
        }
        return result;
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
        var path = Files.GetProjectSettingsFileName(workingDirectory);
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
        var path = Files.GetProjectSettingsFileName(workingDirectory);
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
