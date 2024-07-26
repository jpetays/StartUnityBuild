using Prg.Util;
using PrgFrame.Util;

namespace StartUnityBuild;

/// <summary>
/// Collection of settings used by this build system UI.
/// </summary>
public class BuildSettings(string workingDirectory)
{
    private const string UnityVersionName = "$UNITY_VERSION$";
    private const string BuildTargetName = "$BUILD_TARGET$";
    private const string UniqueNameName = "$UNIQUE_NAME$";
    private const string DeliveryTrackName = "$TRACK_NAME$";
    private const string HostNameName = "$HOST_NAME$";

    public string WorkingDirectory { get; } = workingDirectory;

    /// <summary>
    /// ProductName from ProjectSettings.asset.
    /// </summary>
    public string ProductName { get; set; } = "";

    /// <summary>
    /// ProductVersion from ProjectSettings.asset.
    /// </summary>
    public string ProductVersion { get; set; } = "";

    /// <summary>
    /// BundleVersion from ProjectSettings.asset.
    /// </summary>
    public string BundleVersion { get; set; } = "";

    /// <summary>
    /// IsMuteOtherAudioSources from ProjectSettings.asset.
    /// </summary>
    public bool IsMuteOtherAudioSources { get; set; }

    /// <summary>
    /// BuildInfo Filename for updating C# BuildInfoDataPart.cs.
    /// </summary>
    public string BuildInfoFilename { get; set; } = "";

    /// <summary>
    /// Build targets from _auto_build.env.
    /// </summary>
    public List<string> BuildTargets { get; private set; } = [];

    /// <summary>
    /// Build results from build command.
    /// </summary>
    public List<bool> BuildResult { get; private set; } = [];

    /// <summary>
    /// Optional Delivery Track name for file system etc. operations.
    /// </summary>
    public string DeliveryTrack { get; set; } = "";

    /// <summary>
    /// Copy files before build (source, target).
    /// </summary>
    public Dictionary<string, string> CopyFilesBefore { get; private set; } = [];

    /// <summary>
    /// Revert files before build.
    /// </summary>
    public List<string> RevertFilesAfter { get; private set; } = [];

    /// <summary>
    /// Copy directories after build (source, target).
    /// </summary>
    public Dictionary<string, string> CopyDirectoriesAfter { get; private set; } = [];

    /// <summary>
    /// UnityEditorVersion from ProjectVersion.txt.
    /// </summary>
    public string UnityEditorVersion { get; set; } = "";

    /// <summary>
    /// UnityPath 'replacement string' from _auto_build.env.
    /// </summary>
    public string UnityPath { get; set; } = "";

    /// <summary>
    /// Path to UNITY executable file.
    /// </summary>
    public string UnityExecutable { get; set; } = "";

    /// <summary>
    /// Android settings filename for Android builds.
    /// </summary>
    public string AndroidSettingsFileName { get; set; } = "";

    /// <summary>
    /// Optional WebGl host name or path to folder for WebGL builds.
    /// </summary>
    public string WebGlHostName { get; set; } = "";

    /// <summary>
    /// WebGL build history URL for this build.
    /// </summary>
    public string WebGlBuildHistoryUrl
    {
        get => ExpandUrl(_webGlBuildHistoryUrl);
        set => _webGlBuildHistoryUrl = value;
    }

    /// <summary>
    /// WebGL build history book-keeping json filename.
    /// </summary>
    public string WebGlBuildHistoryJson { get; set; } = "";

    /// <summary>
    /// WebGL build history book-keeping html filename.
    /// </summary>
    public string WebGlBuildHistoryHtml { get; set; } = "";

    /// <summary>
    /// WebGl distribution folder name where build output is copied in web server that hosts WebGl builds.
    /// </summary>
    public string WebGlDistFolderName
    {
        get => ExpandPath(_webGlDistFolderName);
        set => _webGlDistFolderName = value;
    }

    /// <summary>
    /// WebGl build output folder name.
    /// </summary>
    public string WebGlBuildDirName
    {
        get => ExpandPath(_webGlBuildDirName);
        set => _webGlBuildDirName = value;
    }

    public bool HasProductVersionBundle()
    {
        return SemVer.GetVersionType(ProductVersion) switch
        {
            SemVer.SemVerType.VersionDateWithPatch => true,
            SemVer.SemVerType.MajorMinorPatch => true,
            _ => false
        };
    }

    /// <summary>
    /// Checks that settings has given build target included.
    /// </summary>
    public bool HasBuildTarget(string buildTarget) => BuildTargets.Contains(buildTarget);

    /// <summary>
    /// Checks that given build target contains post processing instructions.
    /// </summary>
    public bool HasPostProcessingFor(string buildTarget)
    {
        if (!HasBuildTarget(buildTarget) || buildTarget != BuildName.WebGL)
        {
            return false;
        }
        // Only WebGL has post processing options.
        var hasJson = !string.IsNullOrEmpty(WebGlDistFolderName) &&
                      !string.IsNullOrEmpty(WebGlBuildHistoryJson) &&
                      !string.IsNullOrEmpty(WebGlBuildHistoryHtml);
        var hasFolders = !string.IsNullOrEmpty(WebGlDistFolderName) && !string.IsNullOrEmpty(WebGlBuildDirName);
        return hasJson || hasFolders;
    }

    public bool BuildSucceeded(string buildTarget)
    {
        var index = BuildTargets.FindIndex(x => x == buildTarget);
        return index == -1 || BuildResult[index];
    }

    public static string ExpandUnityPath(string path, string unityVersion) =>
        path.Replace(UnityVersionName, unityVersion);

    public static string ExpandBuildTarget(string path, string buildTarget) =>
        path.Replace(BuildTargetName, buildTarget);

    private string ExpandPath(string path)
    {
        path = path.Replace(DeliveryTrackName, DeliveryTrack);
        path = path.Replace(UniqueNameName, BuildSequenceName);
        return path;
    }

    private string ExpandUrl(string path)
    {
        if (!string.IsNullOrEmpty(WebGlHostName))
        {
            path = path.Replace(HostNameName, $"https://{WebGlHostName}");
        }
        path = path.Replace(DeliveryTrackName, DeliveryTrack);
        path = path.Replace(UniqueNameName, BuildSequenceName);
        return path;
    }

    /// <summary>
    /// Gets cached and lazy-initialized <c>ProductVersion</c> + unique sequence name for this build settings instance
    /// that is suitable to be used for example in file or folder names.
    /// </summary>
    private string BuildSequenceName
    {
        get
        {
            if (string.IsNullOrEmpty(_uniqueBuildSequence) || ProductVersion != _versionForUniqueBuildSequence)
            {
                _versionForUniqueBuildSequence = ProductVersion;
                _uniqueBuildSequence =
                    $"{PathUtil.SanitizePath(_versionForUniqueBuildSequence).Replace('.', '_')}" +
                    $"_{DateTime.Today.DayOfYear}_{RandomUtil.StringFromTicks(6)}";
            }
            return _uniqueBuildSequence;
        }
    }

    private string _webGlBuildHistoryUrl = "";
    private string _webGlDistFolderName = "";
    private string _webGlBuildDirName = "";
    private string _uniqueBuildSequence = "";
    private string _versionForUniqueBuildSequence = "";

    public override string ToString()
    {
        return $"Product: {ProductName} Version: {ProductVersion} Bundle: {BundleVersion}";
    }
}
