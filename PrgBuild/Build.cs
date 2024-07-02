using UnityEngine;

namespace PrgBuild
{
    public static class Build
    {
        public static void BuildPlayer()
        {
            var unityVersion = Application.unityVersion;
            Trace($"start BUILD in UNITY {unityVersion}");
        }

        private static void Trace(string message)
        {
            Debug.Log($"Prg_Build: {message}");
        }
    }
}
