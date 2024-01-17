using System.IO;
using System.Text.RegularExpressions;

namespace TES.Common.Extensions
{
    public static class FileExtensions
    {
        public static string ToValidFileName(this string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var invalidString = Regex.Escape(new string(invalidChars));

            return Regex.Replace(fileName, "[" + invalidString + "]", "");
        }
    }
}
