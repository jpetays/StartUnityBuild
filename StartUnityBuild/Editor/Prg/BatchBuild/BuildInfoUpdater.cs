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

        private const string BuildInfoFilename = "BuildInfoDataPart.cs";

        private static string Timestamp(DateTime dateTime) =>
            ((FormattableString)$"{dateTime:yyyy-MM-dd HH:mm}").ToString(CultureInfo.InvariantCulture);

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

        public static int GetPatchValue(string fromFolder)
        {
            var patchValue = 0;
            var path = FindSourceFile(fromFolder);
            foreach (var line in File.ReadAllLines(path, Encoding))
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

        public static void UpdateBuildInfo(string fromFolder, int bundleVersionCode, int patchValue,
            bool isMuteOtherAudioSourcesValue)
        {
            var path = FindSourceFile(fromFolder);
            if (path == null)
            {
                throw new InvalidOperationException($"unable to find {BuildInfoFilename} from {fromFolder}");
            }
            var namespaceName = FindNamespace(path);
            if (namespaceName == null)
            {
                throw new InvalidOperationException($"unable to C# namespace declaration from {path}");
            }
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
            File.WriteAllText(path, machineGeneratedBuildInfo, Encoding);
        }

        private static string FindNamespace(string filename)
        {
            try
            {
                var line = File.ReadAllLines(filename, Encoding)[0];
                var tokens = line.Split(' ');
                return tokens[1].Trim();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string FindSourceFile(string fromFolder)
        {
            // Try to find 'build info' file in the project by its filename.
            string filePath = null;
            var files = Directory.GetFiles(fromFolder, BuildInfoFilename,
                SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.EndsWith(BuildInfoFilename))
                {
                    filePath = ConvertToWindowsPath(file);
                    break;
                }
            }
            return filePath;

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