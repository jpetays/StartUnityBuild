using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PrgFrame.Util;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PrgBuild
{
    public static class Build
    {
        /// <summary>
        /// Starts build for: -executeMethod PrgBuild.Build.BuildPlayer.<br />
        /// See: https://docs.unity3d.com/Manual/EditorCommandLineArguments.html
        /// Following arguments are required:<br />
        /// -executeMethod namespace.class.method -quit -batchmode<br />
        /// -buildTarget<br />
        /// -projectPath<br />
        /// -logFile<br />
        /// [-android androidFilename]
        /// </summary>
        /// <returns>0 on success<br />
        /// 1 on build failed<br />
        /// 2 on invalid arguments or error starting build
        /// </returns>
        public static void BuildPlayer()
        {
            Util.Trace($"unityVersion: {Application.unityVersion}");
            Util.Trace($"productName: {Application.productName}");
            Util.Trace($"version: {Application.version}");
            Util.Trace($"bundleVersionCode: {PlayerSettings.Android.bundleVersionCode}");
            var options = LoadOptions();
            if (!Util.VerifyUnityVersion(out var editorVersion))
            {
                Util.Trace($"UNITY version {Application.unityVersion} does not match" +
                           $" Editor version {editorVersion} in ProjectSettings/ProjectVersion.txt");
                EditorApplication.Exit(2);
                return;
            }
            try
            {
                var timer = new Timer();
                var buildReport = BuildPLayer(options);
                var buildResult = buildReport.summary.result;
                if (buildResult == BuildResult.Succeeded)
                {
                    var assetPath = UnityBuildReport.CreateBuildReport(buildReport.summary.platform);
                    Util.Trace($"UNITY Build Report saved: {assetPath}");
                }
                timer.Stop();
                Util.Trace($"Build result {buildResult}, build took {timer.ElapsedTime}");
                EditorApplication.Exit(buildResult == BuildResult.Succeeded ? 0 : 1);
            }
            catch (Exception x)
            {
                Util.Trace($"Unhandled exception {x.GetType().Name}: {x.Message}");
                EditorApplication.Exit(2);
            }
        }

        private static BuildReport BuildPLayer(BuildConfig config)
        {
            var scenes = EditorBuildSettings.scenes
                .Where(x => x.enabled && !string.IsNullOrEmpty(x.path))
                .Select(x => x.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new UnityException("NO eligible SCENES FOUND for build in EditorBuildSettings");
            }
            Util.Trace($"scenes {scenes.Length}: {string.Join(',', scenes)}");

            var buildPlayerOptions = new BuildPlayerOptions
            {
                locationPathName = config.OutputPathName,
                options = BuildOptions.StrictMode | BuildOptions.DetailedBuildReport,
                scenes = scenes,
                target = config.BuildTarget,
                targetGroup = config.BuildTargetGroup,
            };
            if (EditorUserBuildSettings.development)
            {
                buildPlayerOptions.options |= BuildOptions.Development;
            }
            if (Directory.Exists(config.OutputFolderName))
            {
                Directory.Delete(config.OutputFolderName, recursive: true);
            }
            Directory.CreateDirectory(config.OutputFolderName);
            ConfigurePlayerSettings(config);
            var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            return buildReport;
        }

        private static void ConfigurePlayerSettings(BuildConfig config)
        {
            // General settings we enforce for any build.
            PlayerSettings.insecureHttpOption = InsecureHttpOption.NotAllowed;
            Util.Trace($"insecureHttpOption: {PlayerSettings.insecureHttpOption}");
            switch (config.BuildTarget)
            {
                case BuildTarget.Android:
                {
                    // Android settings we enforce.
                    PlayerSettings.Android.minifyRelease = true;
                    PlayerSettings.Android.useCustomKeystore = true;
                    Util.Trace($"Android.minifyRelease: {PlayerSettings.Android.minifyRelease}");
                    Util.Trace($"Android.useCustomKeystore: {PlayerSettings.Android.useCustomKeystore}");
                    if (PlayerSettings.Android.useCustomKeystore)
                    {
                        if (config.Android == null)
                        {
                            throw new UnityException($"Android settings not set in '{config.AndroidFile}'");
                        }
                        // Project keystore:
                        PlayerSettings.Android.keystoreName = config.Android.KeystoreName;
                        PlayerSettings.keystorePass = config.Android.KeystorePassword;
                        // Project key:
                        PlayerSettings.Android.keyaliasName = config.Android.KeyaliasName;
                        PlayerSettings.keyaliasPass = config.Android.AliasPassword;

                        Util.Trace($"Android.keystoreName: {PlayerSettings.Android.keystoreName}");
                        Util.Trace($"PlayerSettings.keystorePass: {Util.PasswordToLog(PlayerSettings.keystorePass)}");
                        Util.Trace($"Android.keyaliasName: {PlayerSettings.Android.keyaliasName}");
                        Util.Trace($"PlayerSettings.keyaliasPass: {Util.PasswordToLog(PlayerSettings.keyaliasPass)}");
                    }
                    break;
                }
                case BuildTarget.WebGL:
                    PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                    Util.Trace($"WebGL.compressionFormat: {PlayerSettings.WebGL.compressionFormat}");
                    // No use to show stack trace in browser.
                    Util.Trace($"SetStackTraceLogType: {StackTraceLogType.None}");
                    PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
                    PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
                    PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
                    PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                    PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.None);
                    break;
            }
        }

        private static BuildConfig LoadOptions()
        {
            var args = Environment.GetCommandLineArgs().ToList();
            args.RemoveAt(0);
            Util.Trace($"args: {string.Join(' ', args)}");
            var options = new BuildConfig(args);
            Util.Trace($"buildTarget: {options.BuildTargetName}");
            Util.Trace($"projectPath: {options.ProjectPath}");
            Util.Trace($"logFile: {options.LogFile}");
            var defs = PlayerSettings.GetScriptingDefineSymbols(options.NamedBuildTarget)
                .Split(';')
                .ToList();
            defs.Sort();
            Util.Trace($"defines: {string.Join(' ', defs)}");
            return options;
        }

        private static class Util
        {
            public static bool VerifyUnityVersion(out string editorVersion)
            {
                editorVersion = File
                    .ReadAllLines(Path.Combine("ProjectSettings", "ProjectVersion.txt"))[0]
                    .Split(" ")[1];
                return Application.unityVersion == editorVersion;
            }

            public static string PasswordToLog(string password)
            {
                if (string.IsNullOrWhiteSpace(password) || password.Length <= 8)
                {
                    return "********";
                }
                return $"{password[..2]}********{password[^2..]}";
            }

            public static void Trace(string message)
            {
                Debug.Log($"Prg_Build: {message}");
            }
        }

        private class AndroidOptions
        {
            // PlayerSettings.Android.keyaliasName.
            public readonly string KeyaliasName;

            // This is path to android keystore file.
            public readonly string KeystoreName;

            // Two passwords required for the build as in PlayerSettings.
            public readonly string KeystorePassword;
            public readonly string AliasPassword;

            public AndroidOptions(string keyaliasName, string keystoreName, string keystorePassword,
                string aliasPassword)
            {
                KeyaliasName = keyaliasName;
                KeystoreName = keystoreName;
                KeystorePassword = keystorePassword;
                AliasPassword = aliasPassword;
            }
        }

        private class BuildConfig
        {
            public readonly string BuildTargetName;
            public readonly string ProjectPath = "";
            public readonly string LogFile = "";
            public readonly string AndroidFile = "";
            public readonly BuildTarget BuildTarget;
            public readonly BuildTargetGroup BuildTargetGroup;
            public readonly NamedBuildTarget NamedBuildTarget;
            public readonly string OutputFolderName;
            public readonly string OutputPathName;
            public readonly AndroidOptions? Android;

            public BuildConfig(List<string> args)
            {
                var buildTarget = "";
                using var enumerator = args.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var value = enumerator.Current;
                    switch (value)
                    {
                        case "-buildTarget":
                            enumerator.MoveNext();
                            buildTarget = enumerator.Current;
                            break;
                        case "-projectPath":
                            enumerator.MoveNext();
                            ProjectPath = enumerator.Current ?? "";
                            break;
                        case "-logFile":
                            enumerator.MoveNext();
                            LogFile = enumerator.Current ?? "";
                            break;
                        case "-android":
                            enumerator.MoveNext();
                            AndroidFile = enumerator.Current ?? "";
                            break;
                    }
                }
                switch (buildTarget)
                {
                    case "Android":
                        BuildTarget = BuildTarget.Android;
                        BuildTargetGroup = BuildTargetGroup.Android;
                        NamedBuildTarget = NamedBuildTarget.Android;
                        Android = GetAndroidOptions();
                        break;
                    case "WebGL":
                        BuildTarget = BuildTarget.WebGL;
                        BuildTargetGroup = BuildTargetGroup.WebGL;
                        NamedBuildTarget = NamedBuildTarget.WebGL;
                        break;
                    case "Win64":
                        BuildTarget = BuildTarget.StandaloneWindows64;
                        BuildTargetGroup = BuildTargetGroup.Standalone;
                        NamedBuildTarget = NamedBuildTarget.Standalone;
                        break;
                    default:
                        throw new UnityException($"Build target '{buildTarget}' is not supported or invalid");
                }
                BuildTargetName = BuildPipeline.GetBuildTargetName(BuildTarget);
                OutputFolderName = Path.Combine(ProjectPath, $"build{BuildPipeline.GetBuildTargetName(BuildTarget)}");
                OutputPathName = GetBuildOutput(OutputFolderName);
            }

            private string GetBuildOutput(string folder)
            {
                if (BuildTarget == BuildTarget.WebGL)
                {
                    return folder;
                }
                var appName = PathUtil.SanitizePath($"{Application.productName}_{Application.version}");
                var appExtension = BuildTarget == BuildTarget.Android ? "aab" : "exe";
                return Path.Combine(folder, $"{appName}.{appExtension}");
            }

            private AndroidOptions GetAndroidOptions()
            {
                if (!File.Exists(AndroidFile))
                {
                    throw new UnityException($"AndroidFile not found: {AndroidFile}");
                }
                var keyaliasName = "";
                var keystoreName = "";
                var keystorePassword = "";
                var aliasPassword = "";
                foreach (var line in File.ReadAllLines(AndroidFile)
                             .Where(x => !(string.IsNullOrEmpty(x) || x.StartsWith("#")) && x.Contains('=')))
                {
                    var tokens = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    var key = tokens[0].Trim();
                    var value = tokens[1].Trim();
                    switch (key)
                    {
                        case "keyaliasName":
                            keyaliasName = value;
                            break;
                        case "keystoreName":
                            keystoreName = value;
                            if (!File.Exists(keystoreName))
                            {
                                throw new UnityException($"Android keystore not found: {keystoreName}");
                            }
                            break;
                        case "keystorePassword":
                            keystorePassword = value;
                            break;
                        case "aliasPassword":
                            aliasPassword = value;
                            break;
                    }
                }
                if (string.IsNullOrWhiteSpace(keyaliasName)
                    || string.IsNullOrWhiteSpace(keystoreName)
                    || string.IsNullOrWhiteSpace(keystorePassword)
                    || string.IsNullOrWhiteSpace(aliasPassword))
                {
                    throw new UnityException($"Invalid AndroidFile content: {AndroidFile}");
                }
                return new AndroidOptions(keyaliasName, keystoreName, keystorePassword, aliasPassword);
            }
        }
    }
}
