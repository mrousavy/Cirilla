using System.Text;

namespace Cirilla {
    public static class Extensions {
        /// <summary>
        /// Converts this string to a valid filename
        /// </summary>
        /// <returns>The valid filename</returns>
        public static string ToFileName(this string value) {
            StringBuilder builder = new StringBuilder(value);
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) {
                builder.Replace(c, '_');
            }
            return builder.Length > 255 ? builder.ToString(0, 254) : builder.ToString();
        }
    }
}