using UnityEditor;
using UnityEngine;

namespace Editor.Demo
{
    /// <summary>
    /// For -executeMethod test: Editor.Demo.BuildTest.TestBuild
    /// </summary>
    public static class BuildTest
    {
        public static void TestBuild()
        {
            Debug.Log("HERE");
            EditorApplication.Exit(10);
        }
    }
}
