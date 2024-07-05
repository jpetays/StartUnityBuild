using Prg.Util;
using PrgBuild;

namespace StartUnityBuild;

public static class ProjectCommands
{
    public static void ModifyProject(BuildSettings settings, Action<bool> finished)
    {
        const string outPrefix = "update";
        Task.Run(() =>
        {
            Form1.AddLine($">{outPrefix}", $"update {settings.ProductName} in {settings.WorkingDirectory}");
            var today = DateTime.Now;
            Form1.AddLine($">{outPrefix}", $"today is {today:yyyy-MM-dd HH:mm}");
            try
            {
                DoUpdate();
                finished(true);
            }
            catch (Exception x)
            {
                Form1.AddLine("ERROR", $"Update failed: {x.GetType().Name} {x.Message}");
                finished(false);
            }
            return;

            void DoUpdate()
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
                    productVersion = SemVer.SetDigit(productVersion, 3, int.Parse(bundleVersion));
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
                // Update BuildProperties.cs
                var buildPropertiesPath = BuildInfoUpdater.BuildPropertiesPath(settings.WorkingDirectory);
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
}
