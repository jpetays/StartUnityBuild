namespace StartUnityBuild;

/// <summary>
/// Collection of settings used by build system.
/// </summary>
public class BuildSettings(string workingDirectory)
{
    public string WorkingDirectory { get; } = workingDirectory;

    public string ProductName { get; set; } = "";
    public string ProductVersion { get; set; } = "";
    public string BundleVersion { get; set; } = "";
    public List<string> BuildTargets { get; private set; } = [];
    public string UnityVersion { get; set; } = "";
    public string UnityPath { get; set; } = "";
    public string UnityExecutable { get; set; } = "";
}
