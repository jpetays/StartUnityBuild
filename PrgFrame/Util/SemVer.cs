using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Prg.Util
{
    /// <summary>
    /// Semantic Version check and utilities according to Semantic Versioning spec (https://semver.org).<br />
    /// Version number can be four digit 'date + patch' or three digit MAJOR.MINOR.PATCH.<br />
    /// UNITY BundleVersionCode is used always to store patch value in both cases.
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
        /// Gets version digit value from MAJOR.MINOR.PATCH[.x.y] version string.
        /// </summary>
        public static int GetDigit(string version, int digitPos)
        {
            try
            {
                var versionNumbers = Array.ConvertAll(version.Split('.'), int.Parse);
                return versionNumbers[digitPos];
            }
            catch (Exception x)
            {
                throw new UnityException(
                    $"Unable to get version digit {digitPos} from '{version}': {x.GetType().Name} {x.Message}");
            }
        }

        /// <summary>
        /// Updates version digit value in MAJOR.MINOR.PATCH[.x.y] version string.
        /// </summary>
        public static string SetDigit(string version, int digitPos, int digitValue)
        {
            try
            {
                var versionNumbers = Array.ConvertAll(version.Split('.'), int.Parse);
                versionNumbers[digitPos] = digitValue;
                return string.Join('.', versionNumbers);
            }
            catch (Exception x)
            {
                throw new UnityException(
                    $"Unable to update version digit {digitPos} in '{version}': {x.GetType().Name} {x.Message}");
            }
        }

        /// <summary>
        /// Checks that version string has given number of digits.
        /// </summary>
        public static bool HasDigits(string version, int digitCount)
        {
            try
            {
                var versionNumbers = Array.ConvertAll(version.Split('.'), int.Parse);
                return versionNumbers.Length == digitCount;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks that version string is in dd.mm.yyyy.patch or yyyy.mm.dd.patch format.
        /// </summary>
        public static bool IsVersionDateWithPatch(string version)
        {
            try
            {
                var n = Array.ConvertAll(version.Split('.'), int.Parse);
                if (n.Length != 4)
                {
                    return false;
                }
                var ci = CultureInfo.InvariantCulture;
                const DateTimeStyles styles = DateTimeStyles.None;
                var dateString = $"{n[0]}.{n[1]}.{n[2]}";
                if (DateTime.TryParseExact(dateString, "dd.MM.yyyy", ci, styles, out _)
                    || DateTime.TryParseExact(dateString, "yyyy.MM.dd", ci, styles, out _))
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string CreateVersionDateWithPatch(string version, DateTime date, int patch)
        {
            try
            {
                var n = Array.ConvertAll(version.Split('.'), int.Parse);
                if (n.Length != 4)
                {
                    throw new UnityException(
                        $"Unable to set date+patch in '{version}': length must be 4 digits");
                }
                if (n[0] <= 31)
                {
                    n[0] = date.Day;
                    n[1] = date.Month;
                    n[2] = date.Year;
                }
                else
                {
                    n[0] = date.Year;
                    n[1] = date.Month;
                    n[2] = date.Day;
                }
                n[3] = patch;
                return string.Join('.', n);
            }
            catch (Exception x)
            {
                throw new UnityException(
                    $"Unable to set date+patch in '{version}': {x.GetType().Name} {x.Message}");
            }
        }
    }
}
