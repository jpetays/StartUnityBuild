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

        [MenuItem(MenuItem + "Create Build Report", false, 20)]
        private static void CreateBuildReport() => Logged(() =>
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var created = UnityBuildReport.CreateBuildReport(buildTarget, out var assetPath);
            Debug.Log($"Build System version {Info.Version}");
            Debug.Log($"Build Report {(created ? "created" : "FAILED")} {assetPath}");
        });

        [MenuItem(MenuItem + "About Build System", false, 21)]
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
