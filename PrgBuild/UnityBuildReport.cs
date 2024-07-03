#if PRG_DEBUG
using Debug = Prg.Debug;
#else
using Debug = UnityEngine.Debug;
#endif
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace PrgBuild
{
    /// <summary>
    /// Encapsulates UNITY <c>BuildReport</c> object as used in "Build Report Inspector".<br />
    /// See https://docs.unity3d.com/Packages/com.unity.build-report-inspector@0.1/manual/index.html
    /// </summary>
    /// <remarks>
    /// UNITY build pipeline returns this object but does not allow to save it for later use.<br />
    /// This utility is used to re-create it from file system 'binary file' that contains this information in proprietary format.
    /// <br />
    /// This code is based on UNITY Build Report Inspector<br />
    /// https://docs.unity3d.com/Packages/com.unity.build-report-inspector@0.1/manual/index.html<br />
    /// https://github.com/Unity-Technologies/BuildReportInspector/blob/master/com.unity.build-report-inspector/Editor/BuildReportInspector.cs
    /// </remarks>
    public static class UnityBuildReport
    {
        // UNITY build pipeline creates the during build.
        private const string LastBuildReportFilename = "Library/LastBuild.buildreport";

        // Our folder for collected BuildReport assets.
        private const string BuildReportAssetFolder = "Assets/BuildReports";
        private const string AssetFileExtension = ".buildreport";
        private const string AssetFilter = "t:BuildReport";

        public static string CreateBuildReport(BuildTarget platform)
        {
            if (!File.Exists(LastBuildReportFilename))
            {
                Debug.LogError($"Unable to find UNITY Last Build Report, file NOT FOUND: {LastBuildReportFilename}");
                return null;
            }
            if (!Directory.Exists(BuildReportAssetFolder))
            {
                Directory.CreateDirectory(BuildReportAssetFolder);
            }
            var platformName = BuildPipeline.GetBuildTargetName(platform);
            var assetName = $"{platformName}.build{AssetFileExtension}";
            var buildReportAssetPath = $"{BuildReportAssetFolder}/{assetName}";

            // Copy real last Build Report over our newly create Build Report asset.
            File.Copy(LastBuildReportFilename, buildReportAssetPath, true);
            AssetDatabase.ImportAsset(buildReportAssetPath);
            var buildReport = AssetDatabase.LoadAssetAtPath<BuildReport>(buildReportAssetPath);
            buildReport.name = assetName;
            AssetDatabase.SaveAssets();
            if (buildReport != null)
            {
                Debug.LogError($"Unable to create UNITY Build Report, asset NOT FOUND: {buildReportAssetPath}");
                return null;
            }
            return buildReportAssetPath;
        }

        public static BuildReport LoadBuildReport(BuildTarget platform)
        {
            var platformName = BuildPipeline.GetBuildTargetName(platform);
            var assetName = $"{platformName}.build{AssetFileExtension}";
            var buildReportAssetPath = $"{BuildReportAssetFolder}/{assetName}";
            var buildReport = AssetDatabase.LoadAssetAtPath<BuildReport>(buildReportAssetPath);
            return buildReport;
        }

        #region BuildReport Extension methods

        public static List<PackedAssetInfo> GetPackedAssets(this BuildReport buildReport)
        {
            List<PackedAssetInfo> packedAssets = new List<PackedAssetInfo>();
            foreach (var packedAsset in buildReport.packedAssets)
            {
                var contents = packedAsset.contents;
                foreach (var assetInfo in contents)
                {
                    var sourceAssetGuid = assetInfo.sourceAssetGUID.ToString();
                    if (sourceAssetGuid == "00000000000000000000000000000000" ||
                        sourceAssetGuid == "0000000000000000f000000000000000")
                    {
                        continue;
                    }
                    packedAssets.Add(assetInfo);
                }
            }
            return packedAssets;
        }

        #endregion
    }
}
