using System.Text;

namespace ITnnovative.EncryptionTool.API
{
    public static class Utility
    {
        /// <summary>
        /// Converts data to human-readable string depending on it's context
        /// </summary>
        public static string GetDataString(this byte[] array)
        {
            foreach (var b in array)
            {
                if (b < 32 || b > 126) return ToHexString(array);
            }

            return ToASCIIString(array);
        }
        
        /// <summary>
        /// Return hex by space spaced
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] array)
        {
            var result = new StringBuilder();
            foreach (var b in array)
            {
                result.Append(b.ToString("X")+ " ");
            }

            return result.ToString();
        }

        /// <summary>
        /// Get byte array as ASCII string
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string ToASCIIString(this byte[] array)
        {
            return Encoding.ASCII.GetString(array);
        }
    }
}