using System.IO;
using System.Linq;
using System.Web;

namespace TES.Web.Core.Extensions
{
    public static class WebExtensions
    {
        /// <summary>
        /// To check format of image
        /// </summary>
        /// <param name="file"></param>
        /// <param name="allowedFormats"></param>
        /// <returns></returns>
        public static bool IsValidFormat(this HttpPostedFileBase file, string allowedFormats)
        {
            if (file == null || file.ContentLength == 0)
                return false;

            var extensions = allowedFormats.Split(',');
            var toFilter = extensions.Where(x => !string.IsNullOrWhiteSpace(x)).Select(ext => ext.ToLowerInvariant().Trim()).ToList();

            var fileExtension = Path.GetExtension(file.FileName.ToLowerInvariant());

            return toFilter.Contains(fileExtension);
        }

        /// <summary>
        /// Checks whether an upload is valid
        /// </summary>
        public static bool IsValidFile(this HttpPostedFileBase file)
        {
            return file != null && file.ContentLength > 0;
        }

        public static byte[] ToByteArray(this HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return null;
            }

            var data = new byte[file.ContentLength];
            file.InputStream.Position = 0;
            file.InputStream.Read(data, 0, file.ContentLength);

            return data;
        }
    }
}