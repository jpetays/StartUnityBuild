using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Editor.Prg.Build
{
    /// <summary>
    /// Entrypoint to UNITY commandline batch build system.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    internal static class BatchBuild
    {
        private const string LogPrefix = "[BatchBuild]";

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal static void TestBuild()
        {
            Debug.Log($"{LogPrefix} *");
            Debug.Log($"{LogPrefix} * TestBuild");
            Debug.Log($"{LogPrefix} *");
        }
    }
}
