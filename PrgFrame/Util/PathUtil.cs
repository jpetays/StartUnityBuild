using System.IO;
using System.Linq;

namespace PrgFrame.Util
{
    public static class PathUtil
    {
        private static readonly bool IsWindows =
#if UNITY_EDITOR
            Application.platform == RuntimePlatform.WindowsEditor
            || Application.platform == RuntimePlatform.WindowsPlayer
            || Application.platform == RuntimePlatform.WindowsServer
#else
                true
#endif
            ;

        public static string WindowsPath(string path)
        {
            return IsWindows
                ? path.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString())
                : path;
        }

        public static string SanitizePath(string path)
        {
            // https://www.mtu.edu/umc/services/websites/writing/characters-avoid/
            var illegalCharacters = new[]
            {
                '#', '<', '$', '+',
                '%', '>', '!', '`',
                '&', '*', '\'', '|',
                '{', '?', '"', '=',
                '}', '/', ':', '@',
                '\\', ' '
            };
            for (var i = 0; i < path.Length; ++i)
            {
                var c = path[i];
                if (illegalCharacters.Contains(c))
                {
                    path = path.Replace(c, '_');
                }
            }
            return path;
        }
    }
}
