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

        [MenuItem(MenuItem + "Debug/Create Build Report", false, 20)]
        private static void CreateBuildReport() => Logged(() =>
        {
            var timer = new Timer();
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var created = UnityBuildReport.CreateBuildReport(buildTarget, out var assetPath);
            timer.Stop();
            Debug.Log($"Build Report {(created ? "created" : "FAILED")} {assetPath} in {timer.ElapsedTime}");
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
