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
    /// BuildInfo Filename for updating C# BuildInfoDataPart.cs.
    /// </summary>
    public string BuildInfoFilename { get; set; } = "";

    /// <summary>
    /// Build targets from _auto_build.env.
    /// </summary>
    public List<string> BuildTargets { get; private set; } = [];

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
}
