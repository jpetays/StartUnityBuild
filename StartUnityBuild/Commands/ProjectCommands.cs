using System.Diagnostics.CodeAnalysis;
using System.Text;
using Newtonsoft.Json;
using NLog;
using Prg.Util;
using PrgBuild;

namespace StartUnityBuild.Commands;

/// <summary>
/// Commands to prepare to build UNITY player.
/// </summary>
public static class ProjectCommands
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static void WriteWebGLBuildHistory(BuildSettings settings, bool isSuccess)
    {
        if (!isSuccess
            || string.IsNullOrWhiteSpace(settings.WebGlBuildHistoryJson)
            || !settings.HasPostProcessingFor(BuildName.WebGL))
        {
            Form1.AddLine("ERROR", "Can not do WebGL build history: conditions are not met");
            return;
        }
        var webGlFolderName = Path.GetFileName(settings.WebGlFolderName);
        if (string.IsNullOrWhiteSpace(webGlFolderName))
        {
            Form1.AddLine("ERROR", "Can not do WebGL build history: WebGL Folder Name empty");
            return;
        }
        var linkLabel = $"{settings.ProductVersion}";
        var linkHref = $"{StripEnd(settings.WebGlHostName)}/{StripStart(webGlFolderName)}";
        var releaseNotes = GetReleaseNotesText();
        var buildLogEntryFile = settings.WebGlBuildHistoryJson;
        WriteBuildLogEntry(DateTime.Now, linkLabel, linkHref, releaseNotes, buildLogEntryFile);
        return;

        string StripStart(string path) => path.StartsWith('/') ? path[1..] : path;

        string StripEnd(string path) => path.EndsWith('/') ? path[..^1] : path;

        string GetReleaseNotesText()
        {
            var path = Files.GetReleaseNotesFileName(settings.WorkingDirectory);
            if (File.Exists(path))
            {
                var builder = new StringBuilder();
                foreach (var line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }
                    if (builder.Length > 0)
                    {
                        builder.Append("\r\n");
                    }
                    builder.Append(line);
                }
                if (builder.Length > 0)
                {
                    return builder.ToString();
                }
            }
            return $"{settings.ProductName} {settings.ProductVersion} built on {DateTime.Today:yyyy-MM-dd}";
        }
    }

    public static void ModifyProject(BuildSettings settings, Action<bool> finished)
    {
        const string outPrefix = "update";
        Task.Run(() =>
        {
            Form1.AddLine($">{outPrefix}", $"update {settings.ProductName} in {settings.WorkingDirectory}");
            var today = DateTime.Now;
            Form1.AddLine($">{outPrefix}", $"today is {today:yyyy-MM-dd HH:mm}");
            int result;
            try
            {
                UpdateProjectSettings();
                UpdateBuildProperties();
                Form1.AddLine($">{outPrefix}", "Update done");
                result = 0;
            }
            catch (Exception x)
            {
                Form1.AddLine("ERROR", $"Update failed: {x.GetType().Name} {x.Message}");
                Logger.Trace(x.StackTrace);
                result = -1;
            }
            var isSuccess = result == 0;
            Form1.AddExitCode(outPrefix, result, isSuccess, showSuccess: true);
            finished(false);
            return;

            void UpdateProjectSettings()
            {
                // Update Project settings: ProductVersion and BundleVersion
                var productVersion = settings.ProductVersion;
                // Always increment bundleVersion.
                var bundleVersion = $"{int.Parse(settings.BundleVersion) + 1}";
                if (SemVer.IsVersionDateWithPatch(productVersion))
                {
                    // Set version as date + patch.
                    productVersion = SemVer.CreateVersionDateWithPatch(productVersion, today, int.Parse(bundleVersion));
                }
                else if (SemVer.HasDigits(productVersion, 3))
                {
                    // Synchronize productVersion with bundleVersion in MAJOR.MINOR.PATCH format.
                    productVersion = SemVer.SetDigit(productVersion, 2, int.Parse(bundleVersion));
                }
                var updateCount = ProjectSettings.UpdateProjectSettingsFile(
                    settings.WorkingDirectory, productVersion, bundleVersion);
                switch (updateCount)
                {
                    case -1:
                        Form1.AddLine("ERROR", $"Could not update ProjectSettingsFile");
                        break;
                    case 0:
                        Form1.AddLine($".{outPrefix}", $"Did not update ProjectSettingsFile, it is same");
                        break;
                    case 1:
                        if (settings.ProductVersion != productVersion)
                        {
                            Form1.AddLine($".{outPrefix}",
                                $"update ProductVersion {settings.ProductVersion} <- {productVersion}");
                            settings.ProductVersion = productVersion;
                        }
                        Form1.AddLine($".{outPrefix}",
                            $"update BundleVersion {settings.BundleVersion} <- {bundleVersion}");
                        settings.BundleVersion = bundleVersion;
                        break;
                }
            }

            void UpdateBuildProperties()
            {
                // Update BuildProperties.cs
                var assetFolder = Files.GetAssetFolder(settings.WorkingDirectory);
                var buildPropertiesPath = BuildInfoUpdater.BuildPropertiesPath(assetFolder);
                if (buildPropertiesPath.Length < settings.WorkingDirectory.Length)
                {
                    Form1.AddLine("ERROR", $"File not found '{buildPropertiesPath}' in {assetFolder}");
                    return;
                }
                var shortName = buildPropertiesPath[(settings.WorkingDirectory.Length + 1)..];
                var isMuteOtherAudioSources = settings.IsMuteOtherAudioSources;
                if (BuildInfoUpdater.UpdateBuildInfo(buildPropertiesPath,
                        today, settings.BundleVersion, isMuteOtherAudioSources))
                {
                    Form1.AddLine($".{outPrefix}", $"update BuildProperties {shortName}");
                }
                else
                {
                    Form1.AddLine($".{outPrefix}", $"Did not update BuildProperties {shortName}, it is same");
                }
            }
        });
    }

    public static bool CopyFiles(BuildSettings settings)
    {
        const string outPrefix = "copy";
        try
        {
            var copyFiles = Files.GetCopyFiles(settings);
            foreach (var tuple in copyFiles)
            {
                Form1.AddLine($">{outPrefix}", $"copy {tuple.Item1} to {tuple.Item2}");
                File.Copy(tuple.Item1, tuple.Item2, overwrite: true);
            }
            return true;
        }
        catch (Exception x)
        {
            Form1.AddLine("ERROR", $"Copy failed: {x.GetType().Name} {x.Message}");
            Logger.Trace(x.StackTrace);
            return false;
        }
    }

    private static void WriteBuildLogEntry(DateTime date, string linkLabel, string linkHref, string releaseNotes,
        string jsonFilename)
    {
        var entries = Serializer.LoadStateJson<BuildLogEntries>(jsonFilename) ?? new BuildLogEntries();
        entries.List.Insert(0, new BuildLogEntry
        {
            Ver = "1",
            Date = $"{date:yyyy-MM-dd HH:mm}",
            Label = linkLabel,
            HRef = linkHref,
            Notes = releaseNotes
        });
        Form1.AddLine($".update", $"Build history log entries #{entries.List.Count} in {jsonFilename}");
        Serializer.SaveStateJson(entries, jsonFilename, Formatting.Indented);
    }

    /// <summary>
    /// JSON serialized build log entry.<br />
    /// This can be used for example to create table of content for all (recent) builds.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class BuildLogEntry
    {
        public string Ver { get; set; } = "";
        public string Date { get; set; } = "";
        public string Label { get; set; } = "";
        public string HRef { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    /// <summary>
    /// JSON serialized container for <c>BuildLogEntry</c> instances.
    /// </summary>
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    private class BuildLogEntries
    {
        public List<BuildLogEntry> List = [];
    }
}
