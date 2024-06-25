using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Prg.Util
{
    /// <summary>
    /// Semantic Version check according to Semantic Versioning spec (https://semver.org).<br />
    /// See: https://stackoverflow.com/questions/49338155/c-sharp-how-can-you-compare-versions
    /// </summary>
    public static class SemVer
    {
        /// <summary>
        /// Compares two Semantic Version strings (of any length).
        /// </summary>
        /// <param name="first">first version string</param>
        /// <param name="second">second version string</param>
        /// <returns>first &gt; second : 1<br />
        /// first == second : 0<br />
        /// first &lt; second : -1
        /// </returns>
        public static int Compare(string first, string second)
        {
            var intVersions = new List<int[]>
            {
                Array.ConvertAll(first.Split('.'), int.Parse),
                Array.ConvertAll(second.Split('.'), int.Parse)
            };
            var cmp = intVersions.First().Length.CompareTo(intVersions.Last().Length);
            if (cmp == 0)
            {
                intVersions = intVersions.Select(v =>
                {
                    Array.Resize(ref v, intVersions.Min(x => x.Length));
                    return v;
                }).ToList();
            }
            var strVersions = intVersions.ConvertAll(v =>
            {
                return string.Join("", Array.ConvertAll(v,
                    i => { return i.ToString($"D{intVersions.Max(x => x.Max().ToString().Length)}"); }));
            });
            var cmpVersions = strVersions.OrderByDescending(i => i).ToList();
            return cmpVersions.First().Equals(cmpVersions.Last())
                ? cmp
                : cmpVersions.First().Equals(strVersions.First())
                    ? 1
                    : -1;
        }

        /// <summary>
        /// Gets patch value from MAJOR.MINOR.PATCH version string.
        /// </summary>
        public static int GetPatch(string version)
        {
            try
            {
                var versionNumbers = Array.ConvertAll(version.Split('.'), int.Parse);
                return versionNumbers.Length is 3
                    ? versionNumbers[^1]
                    : 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static string IncrementPatch(string version)
        {
            try
            {
                var versionNumbers = Array.ConvertAll(version.Split('.'), int.Parse);
                switch (versionNumbers.Length)
                {
                    case 2:
                        return $"{version}.0";
                    case 3:
                        versionNumbers[^1] += 1;
                        return string.Join('.', versionNumbers);
                    default:
                        return version;
                }
            }
            catch (Exception)
            {
                return version;
            }
        }

        /// <summary>
        /// Checks that version string is in MAJOR.MINOR.PATCH format.
        /// </summary>
        public static bool IsSemantic(string version)
        {
            try
            {
                var versionNumbers = Array.ConvertAll(version.Split('.'), int.Parse);
                return versionNumbers.Length is 3;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
