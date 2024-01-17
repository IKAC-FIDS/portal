namespace TES.Common.Extensions
{
    public static class PersianExtensions
    {
        #region Fields (4)

        public const char ArabicKeChar = (char)1603;
        public const char ArabicYeChar = (char)1610;
        public const char PersianKeChar = (char)1705;
        public const char PersianYeChar = (char)1740;

        #endregion Fields

        /// <summary>
        /// Fixes common writing mistakes caused by using a bad keyboard layout,
        /// such as replacing Arabic Ye with Persian one and so on ...
        /// </summary>
        /// <param name="text">Text to process</param>
        /// <returns>Processed Text</returns>
        public static string ApplyPersianYeKe(this string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : text.Replace(ArabicYeChar, PersianYeChar).Replace(ArabicKeChar, PersianKeChar).Replace('ئ', 'ی').Trim();
        }

        /// <summary>
        /// Get Persian Numbers
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetPersianNumbers(this string s)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : s.Replace("0", "۰").Replace("1", "۱").Replace("2", "۲").Replace("3", "۳").Replace("4", "۴").Replace("5", "۵").Replace("6", "۶").Replace("7", "۷").Replace("8", "۸").Replace("9", "۹");
        }

        public static string RemoveHamzeh(this string text)
        {
            return text.Replace("ء", "");
        }
    }
}
