using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TES.Common.Extensions
{
    /// <summary>
    /// Utility class for string manipulation.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// If string is number return true
        /// </summary>
        /// <param name="s"></param>
        /// <returns>bool</returns>
        public static bool IsItNumber(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            var isnumber = new Regex("[^0-9]");

            return !isnumber.IsMatch(s);
        }

        /// <summary>
        /// Shortcut for System.String.Format
        /// Example:
        /// string greeting = "Hello {0}, my name is {1}, and I own you."
        /// Console.WriteLine(greeting.Format("Adam", "Microsoft"))
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        /// <param name="additionalArgs"></param>
        /// <returns></returns>
        public static string Format(this string format, object arg, params object[] additionalArgs)
        {
            if (additionalArgs == null || additionalArgs.Length == 0)
            {
                return string.Format(format, arg);
            }

            return string.Format(format, new[] { arg }.Concat(additionalArgs).ToArray());
        }

        /// <summary>
        /// It truncates the given string with the given length.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="length">The length of string you want to have.</param>
        /// <returns>string</returns>
        public static string Truncate(this string input, int length)
        {
            return input.Length > length ? input.Substring(0, length) + " ..." : input;
        }

        /// <summary>
        /// Get long values from comma separated input string. Used for multiselect
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<int> GetCommaSeparatedValues(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<int>();
            }

            var values = input.Split(',');

            return values.Where(x => !string.IsNullOrWhiteSpace(x)).Select(item => Convert.ToInt32(item)).ToList();
        }

        public static bool IsValidPostCode(this string postCode)
        {
            if (string.IsNullOrEmpty(postCode) || postCode.Length != 10 || !postCode.IsItNumber())
            {
                return false;
            }

            var firstFiveDigits = postCode.Substring(0, 5);

            return !firstFiveDigits.Contains('0') &&
                   !firstFiveDigits.Contains('2') &&
                   postCode[4] != '5' &&
                   postCode[5] != '0' &&
                   postCode.Substring(6, 4) != "0000";
        }

        public static string NormalizeMobile(this string mobile)
        {
            return mobile?.Replace("+98", "0");
        }
    }
}