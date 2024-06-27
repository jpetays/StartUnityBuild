using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Prg;
using Prg.Util;
using UnityEditor;
using UnityEngine;
#if PRG_DEBUG
using Debug = Prg.Debug;
#else
using Debug = UnityEngine.Debug;
#endif

/// <summary>
/// Convenience class for platform detection to access platform specific features.<br />
/// Note that we have distinct separation of <c>IsEditor</c> and <c>IsDevelopmentBuild</c>, UNITY considers that they are "same".
/// </summary>
/// <remarks>
/// Most of these are getters because
/// static code analysis will otherwise complain about using compile time constants that are always <c>true</c> or <c>false</c>.
/// </remarks>
[SuppressMessage("ReSharper", "CheckNamespace")]
public static class AppPlatform
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");

    /// <summary>
    /// Alias for UNITY <c>Application.isEditor</c>.
    /// </summary>
    public static bool IsEditor
    {
        get
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// Replacement for UNITY <c>Debug.isDebugBuild</c> that returns <c>true</c> when running outside UNITY Editor
    /// and check box called "Development Build" is checked.
    /// </summary>
    /// <remarks>
    /// See differences from https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Debug-isDebugBuild.html
    /// </remarks>
    public static bool IsDevelopmentBuild
    {
        get
        {
#if DEVELOPMENT_BUILD
            return true;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// Check if we are running a device simulator mode inside UNITY Editor.
    /// </summary>
    public static bool IsSimulator => IsEditor && DeviceUtil.IsSimulator;

    /// <summary>
    /// Gets simple platform name.
    /// </summary>
    public static string Name => Application.platform
        .ToString()
        .Replace("Player", "")
        .Replace("Editor", "");

    /// <summary>
    ///  Mobile platform (for consistency).
    /// </summary>
    public static bool IsMobile => Application.isMobilePlatform;

    /// <summary>
    /// Desktop platforms.
    /// </summary>
    public static bool IsDesktop => Application.platform is
        RuntimePlatform.WindowsPlayer or RuntimePlatform.LinuxPlayer or RuntimePlatform.OSXPlayer;

    /// <summary>
    /// WebGL platform.
    /// </summary>
    public static bool IsWebGL => Application.platform is RuntimePlatform.WebGLPlayer;

    /// <summary>
    /// Windows platform can be editor, player or server.
    /// </summary>
    public static bool IsWindows { get; } = Application.platform is
        RuntimePlatform.WindowsEditor or RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsServer;

    /// <summary>
    /// Converts (UNITY) path separators to windows style (only on windows platform where we can have two directory separators).
    /// </summary>
    public static string ConvertToWindowsPath(string path) =>
        IsWindows
            ? path.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString())
            : path;

    /// <summary>
    /// Gets <c>Screen</c> info with current window size (if not full screen).
    /// </summary>
    /// <returns></returns>
    public static string Resolution()
    {
        var screen = $"{Screen.currentResolution.width}x{Screen.currentResolution.height}";
        if (Screen.currentResolution.width != Screen.width || Screen.currentResolution.height != Screen.height)
        {
            screen += $" ({Screen.width}x{Screen.height})";
        }
        var refreshRate = Screen.currentResolution.refreshRateRatio.value;
        if (double.IsNaN(refreshRate))
        {
            // Fix Simulator etc.
            refreshRate = 0;
        }
        return $"{screen} {refreshRate:0}Hz";
    }

    public static bool CanExit => !(
        // NOP - There is no API provided for gracefully terminating an iOS application.
        Application.platform == RuntimePlatform.IPhonePlayer ||
        // NOP - no can do in browser
        Application.platform == RuntimePlatform.WebGLPlayer
    );

    public static void ExitGracefully()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
#if UNITY_EDITOR
            Debug.Log(RichText.Yellow("stop playing"));
            EditorApplication.isPlaying = false;
#endif
            return;
        }
        if (CanExit)
        {
            // Android, desktop, etc. goes here
            Application.Quit(0);
        }
    }
}
