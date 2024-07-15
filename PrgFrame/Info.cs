namespace PrgFrame
{
    public static class Info
    {
        // When you change SemVer remember to edit PrgFrame.csproj to have same data.
        // For compatibility reasons we do not want to read it from the assembly using
        // System.Diagnostics and System.Reflection.
        public static string SemVer => "1.0.1";
        public static string Version => $"{nameof(PrgFrame)} {SemVer}";
    }
}
