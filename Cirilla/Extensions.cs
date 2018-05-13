using System.IO;
using System.Linq;
using System.Text;

namespace Cirilla
{
    public static class Extensions
    {
        /// <summary>
        ///     Converts this string to a valid filename
        /// </summary>
        /// <returns>The valid filename</returns>
        public static string ToFileName(this string value)
        {
            var builder = new StringBuilder(value);
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                builder.Replace(c, '_');
            }

            return builder.Length > 255 ? builder.ToString(0, 254) : builder.ToString();
        }

        public static string Capitalize(this string value) => value.First().ToString().ToUpper() + value.Substring(1);
    }
}