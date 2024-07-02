using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace PrgBuild
{
    public static class Build
    {
        /// <summary>
        /// Starts build for: -executeMethod PrgBuild.Build.BuildPlayer.
        /// </summary>
        public static void BuildPlayer()
        {
            var args = Environment.GetCommandLineArgs().ToList();
            args.RemoveAt(0);
            foreach (var arg in args)
            {
                Trace($"arg: {arg}");
            }
            Trace($"unityVersion: {Application.unityVersion}");
            Trace($"productName: {Application.productName}");
            Trace($"version: {Application.version}");
            Trace($"bundleVersionCode: {PlayerSettings.Android.bundleVersionCode}");
            var options = new BuildOptions(args);
            Trace($"buildTarget: {options.BuildTarget}");
            Trace($"projectPath: {options.ProjectPath}");
            Trace($"logFile: {options.LogFile}");
            foreach (var def in PlayerSettings.GetScriptingDefineSymbols(options.NamedBuildTarget)
                         .Split(';'))
            {
                Trace($"def: {def}");
            }
        }

        private static void Trace(string message)
        {
            Debug.Log($"Prg_Build: {message}");
        }

        private class BuildOptions
        {
            public readonly string BuildTargetName;
            public readonly string ProjectPath = "";
            public readonly string LogFile = "";
            public readonly BuildTarget BuildTarget;
            public readonly NamedBuildTarget NamedBuildTarget;

            public BuildOptions(List<string> args)
            {
                var buildTarget = "";
                using var enumerator = args.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var value = enumerator.Current.ToLower();
                    switch (value)
                    {
                        case "-buildTarget":
                            enumerator.MoveNext();
                            buildTarget = enumerator.Current;
                            break;
                        case "-projectPath":
                            enumerator.MoveNext();
                            ProjectPath = enumerator.Current;
                            break;
                        case "-logFile":
                            enumerator.MoveNext();
                            LogFile = enumerator.Current;
                            break;
                    }
                }
                switch (buildTarget)
                {
                    case "Android":
                        BuildTarget = BuildTarget.Android;
                        NamedBuildTarget = NamedBuildTarget.Android;
                        break;
                    case "WebGL":
                        BuildTarget = BuildTarget.WebGL;
                        NamedBuildTarget = NamedBuildTarget.WebGL;
                        break;
                    case "Win64":
                        BuildTarget = BuildTarget.StandaloneWindows64;
                        NamedBuildTarget = NamedBuildTarget.Standalone;
                        break;
                    default:
                        throw new UnityException($"Build target '{buildTarget}' is not supported or invalid");
                }
                BuildTargetName = BuildPipeline.GetBuildTargetName(BuildTarget);
            }
        }
    }
}
