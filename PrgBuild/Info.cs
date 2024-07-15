using System;
using System.Diagnostics;
using System.Reflection;

namespace PrgBuild
{
    public static class Info
    {
        public static string SemVer
        {
            get
            {
                if (string.IsNullOrEmpty(_semVer))
                {
                    _semVer = GetVersion();
                }
                return _semVer;
            }
        }

        public static string Version => $"{nameof(PrgBuild)} {SemVer}";

        private static string _semVer = "";

        private static string GetVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersionInfo.ProductVersion.Split('+')[0];
            }
            catch (Exception)
            {
                return "0.0.0";
            }
        }
    }
}
