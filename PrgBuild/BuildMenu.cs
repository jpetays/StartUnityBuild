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

        [MenuItem(MenuItem + "MenuTest", false, 10)]
        private static void MenuTest() => Logged(() =>
        {
            Debug.Log("here");
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
