using Prg.Util;
using PrgFrame.Util;

namespace StartUnityBuild;

/// <summary>
/// Collection of settings used by this build system UI.
/// </summary>
public class BuildSettings(string workingDirectory)
{
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
    /// Git options for push command.
    /// </summary>
    public string PushOptions { get; set; } = "";

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
    /// WebGl host name or path to folder for WebGL builds.
    /// </summary>
    public string WebGlHostName { get; set; } = "";

    /// <summary>
    /// Path to folder for WebGL build history.
    /// </summary>
    public string WebGlBuildHistoryJson { get; set; } = "";

    /// <summary>
    /// Gets cached and lazy-initialized WebGl folder name where build output is copied in web server that hosts WebGl build.
    /// </summary>
    public string WebGlFolderName
    {
        get
        {
            // TODO: cache Files.GetCopyDirs for WebGL!
            if (string.IsNullOrEmpty(_webGlFolderName))
            {
                var copyDirs = Files.GetCopyDirs(this, BuildName.WebGL);
                _webGlFolderName = copyDirs.Count == 1 ? copyDirs[0].Item2 : "";
                _webGlFolderName = Files.ExpandDeliveryTrackName(_webGlFolderName, DeliveryTrack);
                _webGlFolderName = Files.ExpandUniqueName(_webGlFolderName, BuildSequenceName);
            }
            return _webGlFolderName;
        }
    }

    public string WebGlBuildDir
    {
        get
        {
            if (string.IsNullOrEmpty(_webGlBuildDir))
            {
                var copyDirs = Files.GetCopyDirs(this, BuildName.WebGL);
                _webGlBuildDir = copyDirs.Count == 1 ? copyDirs[0].Item1 : "";
            }
            return _webGlBuildDir;
        }
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
        if (!HasBuildTarget(buildTarget))
        {
            return false;
        }
        var buildTargetPart = $".{buildTarget}.";
        return CopyDirectoriesAfter.Keys.Any(x => x.Contains(buildTargetPart));
    }

    public bool BuildSucceeded(string buildTarget)
    {
        var index = BuildTargets.FindIndex(x => x == buildTarget);
        return index == -1 || BuildResult[index];
    }

    /// <summary>
    /// Gets cached and lazy-initialized <c>ProductVersion</c> + unique sequence name for this build settings instance
    /// that is suitable to be used for example in file or folder names.
    /// </summary>
    private string BuildSequenceName
    {
        get
        {
            if (string.IsNullOrEmpty(_uniqueBuildSequence))
            {
                _versionForUniqueBuildSequence = ProductVersion;
                _uniqueBuildSequence =
                    $"{PathUtil.SanitizePath(_versionForUniqueBuildSequence).Replace('.', '_')}" +
                    $"_{DateTime.Today.DayOfYear}_{RandomUtil.StringFromTicks(6)}";
            }
            if (ProductVersion != _versionForUniqueBuildSequence)
            {
                throw new InvalidOperationException(
                    $"Product version '{_versionForUniqueBuildSequence}' has been changed to '{ProductVersion}, current BuildSequence is invalid");
            }
            return _uniqueBuildSequence;
        }
    }

    private string _webGlFolderName = "";
    private string _webGlBuildDir = "";
    private string _uniqueBuildSequence = "";
    private string _versionForUniqueBuildSequence = "";

    public override string ToString()
    {
        return $"Product: {ProductName} Version: {ProductVersion} Bundle: {BundleVersion}";
    }
}
