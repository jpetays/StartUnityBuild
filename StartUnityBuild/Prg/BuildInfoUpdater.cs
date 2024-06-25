using System.Globalization;
using System.Text;
#if UNITY_EDITOR
using System;
using System.IO;
#endif

// ReSharper disable once CheckNamespace
namespace Editor.Prg.BatchBuild
{
    /// <summary>
    /// Utility to read/write <b>BuildInfoDataPart.cs</b> C# source file somewhere in the UNITY project and
    /// create corresponding 'build.properties' file content.
    /// </summary>
    /// <remarks>
    /// This source code is shared between UNITY and non-UNITy projects!
    /// </remarks>
    public static class BuildInfoUpdater
    {
        private static readonly Encoding Encoding = new UTF8Encoding(false, false);

        private const string BuildInfoFilenameName = "BuildInfoDataPart.cs";

        private static string Timestamp(DateTime dateTime) =>
            ((FormattableString)$"{dateTime:yyyy-MM-dd HH:mm}").ToString(CultureInfo.InvariantCulture);

        public static string GetGFitPath(string workingDirectory, string filename)
        {
            const string assetFolderName = "Assets";
            var assetFolderNameLen = assetFolderName.Length;

            workingDirectory = workingDirectory.Replace('\\', '/');
            if (workingDirectory.EndsWith('/'))
            {
                workingDirectory = workingDirectory[..^1];
            }
            if (workingDirectory.EndsWith(assetFolderName))
            {
                workingDirectory = workingDirectory[..^assetFolderNameLen];
            }
            if (workingDirectory.EndsWith('/'))
            {
                workingDirectory = workingDirectory[..^1];
            }
            filename = filename[workingDirectory.Length..].Replace('\\', '/');
            if (filename.StartsWith('/'))
            {
                filename = filename[1..];
            }
            if (!filename.StartsWith(assetFolderName))
            {
                throw new InvalidOperationException($"Unable to get git path from: {filename}");
            }
            return filename;
        }

        /// <summary>
        /// Utility to create build.properties file content for different batch files used in build system.
        /// </summary>
        public static string CreateLocalProperties(string appVersion, int bundleVersion, int patchValue,
            string buildTarget)
        {
            // ReSharper disable InconsistentNaming
            var APP_VERSION = appVersion;
            var BUNDLE_VERSION_CODE = bundleVersion.ToString();
            var PATCH_VALUE = patchValue.ToString();
            var UNIQUE_BUILD_VALUE = GetRandomGuidString();
            var BUILD_TARGET = buildTarget ?? "?";
            // ReSharper restore InconsistentNaming

            return new StringBuilder(
                    @$"#
# Local Build properties created by batch build on {Timestamp(DateTime.Now)}
#")
                .AppendLine()
                .Append("APP_VERSION=").AppendLine(APP_VERSION)
                .Append("BUNDLE_VERSION_CODE=").AppendLine(BUNDLE_VERSION_CODE)
                .Append("PATCH_VALUE=").AppendLine(PATCH_VALUE)
                .Append("UNIQUE_BUILD_VALUE=").AppendLine(UNIQUE_BUILD_VALUE)
                .Append("BUILD_TARGET=").Append(BUILD_TARGET)
                .ToString();

            string GetRandomGuidString() => Guid.NewGuid().ToString("N")[..12];
        }

        public static int GetPatchValue(string buildInfoFilename)
        {
            if (!File.Exists(buildInfoFilename))
            {
                throw new InvalidOperationException($"BuildInfoFile not found {buildInfoFilename}");
            }
            var patchValue = 0;
            foreach (var line in File.ReadAllLines(buildInfoFilename, Encoding))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                if (!line.Contains("internal const int PatchValue"))
                {
                    continue;
                }
                var tokens = line.Split(new[] { '=', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var lastToken = tokens[^1].Trim();
                if (!int.TryParse(lastToken, out patchValue))
                {
                    patchValue = -1;
                }
            }
            return patchValue;
        }

        public static bool UpdateBuildInfo(string buildInfoFilename, int bundleVersionCode, int patchValue,
            bool isMuteOtherAudioSourcesValue)
        {
            if (!File.Exists(buildInfoFilename))
            {
                throw new InvalidOperationException($"BuildInfoFile not found {buildInfoFilename}");
            }
            var namespaceName = GetNamespace();
            var machineGeneratedBuildInfo = @"namespace $namespace$
{
    internal static class MachineGeneratedBuildInfo
    {
        internal const string CompiledOnDateValue = ""$CompiledOnDateValue$"";
        internal const int BundleVersionCodeValue = $BundleVersionCodeValue$;
        internal const int PatchValue = $PatchValue$;
        internal const bool IsMuteOtherAudioSourcesValue = $IsMuteOtherAudioSourcesValue$;
    }
}"
                    .Replace("$namespace$", namespaceName)
                    .Replace("$CompiledOnDateValue$", Timestamp(DateTime.Now))
                    .Replace("$BundleVersionCodeValue$", bundleVersionCode.ToString())
                    .Replace("$PatchValue$", patchValue.ToString())
                    .Replace("$IsMuteOtherAudioSourcesValue$", isMuteOtherAudioSourcesValue.ToString().ToLower())
                ;
            var original = File.ReadAllText(buildInfoFilename, Encoding);
            if (original == machineGeneratedBuildInfo)
            {
                return false;
            }
            File.WriteAllText(buildInfoFilename, machineGeneratedBuildInfo, Encoding);
            return true;

            string GetNamespace()
            {
                try
                {
                    var line = File.ReadAllLines(buildInfoFilename, Encoding)[0];
                    var tokens = line.Split(' ');
                    return tokens[1].Trim();
                }
                catch (Exception)
                {
                    throw new InvalidOperationException($"unable to C# namespace declaration from {buildInfoFilename}");
                }
            }
        }

        public static string BuildInfoFilename(string fromFolder)
        {
            // Try to find 'build info' file in the project by its filename.
            var files = Directory.GetFiles(fromFolder, BuildInfoFilenameName,
                SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.EndsWith(BuildInfoFilenameName))
                {
                    return ConvertToWindowsPath(file);
                }
            }
            // This will be error but we set filename so caller can know what is was looking for!
            return BuildInfoFilenameName;

            string ConvertToWindowsPath(string path)
            {
#if UNITY_EDITOR
                // Running in UNITY build.
                return AppPlatform.IsWindows
                    ? path.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString())
                    : path;
#else
                // Running outside UNITY build.
                return path.Contains(Path.AltDirectorySeparatorChar)
                    ? path.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString())
                    : path;
#endif
            }
        }
    }
}
