using UnityEditor;
using UnityEngine;

namespace PrgBuild
{
    /// <summary>
    /// Shows (and optionally creates) a HTML build report for given target.<ber />
    /// If UNITY Build Report is newer or does not exist, a new HTML Build Report is created.
    /// </summary>
    public static class HtmlBuildReport
    {
        public static string ShowHtmlBuildReport(BuildTarget platform)
        {
            var targetName = BuildPipeline.GetBuildTargetName(platform) ?? platform.ToString();
            var buildReport = UnityBuildReport.LoadBuildReport(platform);
            if (buildReport == null)
            {
                Debug.LogError($"UNITY Build Report NOT FOUND for: {targetName}");
                return "";
            }
            Debug.Log($"UNITY Last Report {buildReport.name} for {targetName}");
            return buildReport.name;
        }
    }
}
