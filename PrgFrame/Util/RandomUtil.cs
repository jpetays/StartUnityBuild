using System;
using System.Linq;

namespace PrgFrame.Util
{
    /// <summary>
    /// Collection of random generator helpers.
    /// </summary>
    public static class RandomUtil
    {
        public static string StringFromTicks(int len)
        {
            // Ticks string len is 18.
            if (len < 2)
            {
                len = 2;
            }
            else if (len > 16)
            {
                len = 16;
            }
            return string.Join("", DateTime.Now.Ticks.ToString()[^len..].ToCharArray().Reverse());
        }

        public static string StringFromGuid(int len)
        {
            // Guid max len is 32 (format 'N').
            if (len < 2)
            {
                len = 2;
            }
            else if (len > 32)
            {
                len = 32;
            }
            var guid = Guid.NewGuid().ToString("N");
            var left = len / 2;
            var right = len / 2 + len % 2;
            return $"{guid[..left]}{guid[^right..]}";
        }
    }
}
