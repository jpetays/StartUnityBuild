using System;
using PrgFrame.Util;
using Debug = UnityEngine.Debug;
using UnityEditor;

namespace PrgBuild
{
    internal static class BuildMenu
    {
        private const string MenuRoot = "Prg/";
        private const string MenuItem = MenuRoot + "Build/";

        [MenuItem(MenuItem + "Show Html Build Report", false, 10)]
        private static void ShowHtmlBuildReport() => Logged(() =>
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var path = HtmlBuildReport.ShowHtmlBuildReport(buildTarget);
            var created = !string.IsNullOrWhiteSpace(path);
            Debug.Log($"HTML Build Report {(created ? "created" : "FAILED")} {path}");
        });

        [MenuItem(MenuItem + "Create Build Report", false, 11)]
        private static void CreateBuildReport() => Logged(() =>
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var created = UnityBuildReport.CreateBuildReport(buildTarget, out var assetPath);
            Debug.Log($"UNITY Build Report {(created ? "created" : "FAILED")} {assetPath}");
        });

        [MenuItem(MenuItem + "About Build System", false, 12)]
        private static void AboutBuildSystem() => Logged(() =>
        {
            Debug.Log($"Build System: {Info.Version} with {PrgFrame.Info.Version}");
        });

        private static void Logged(Action action)
        {
            Debug.Log("*");
            var timer = new Timer();
            action();
            timer.Stop();
            Debug.Log($"Command took {timer.ElapsedTime}");
        }
    }
}
