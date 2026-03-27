using System.Text;

namespace CocoQR.QR_Generator.Encoders
{
    /// <summary>
    /// CRC16-CCITT với UTF-8 (không phải ASCII)
    /// </summary>
    public static class TLVEncoder
    {
        /// <summary>
        /// Encode một TLV field.
        /// </summary>
        /// <param name="tag">Tag ID (2 ký tự, ví dụ "00", "54")</param>
        /// <param name="value">Giá trị — Length được tính bằng UTF-8 byte count</param>
        /// <exception cref="ArgumentException">Khi value vượt quá 99 bytes UTF-8</exception>
        public static string Encode(string tag, string value)
        {
            if (string.IsNullOrEmpty(tag) || tag.Length != 2)
                throw new ArgumentException($"Tag phải đúng 2 ký tự, nhận: '{tag}'");

            var byteLength = Encoding.UTF8.GetByteCount(value);

            if (byteLength > 99)
                throw new ArgumentException(
                    $"Field '{tag}': value quá dài ({byteLength} bytes UTF-8, tối đa 99). " +
                    $"Cắt ngắn nội dung trước khi encode.");

            return $"{tag}{byteLength:00}{value}";
        }

        /// <summary>
        /// Encode nhiều TLV fields liên tiếp và ghép lại thành một string.
        /// </summary>
        public static string EncodeMany(params (string tag, string value)[] fields)
        {
            var sb = new StringBuilder();
            foreach (var (tag, value) in fields)
                sb.Append(Encode(tag, value));
            return sb.ToString();
        }

        /// <summary>
        /// Truncate string sao cho không vượt quá maxBytes UTF-8.
        /// Dùng để cắt description trước khi encode.
        /// </summary>
        public static string TruncateToByteLimit(string value, int maxBytes = 99)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length <= maxBytes) return value;

            // Cắt và decode lại — tránh cắt giữa multi-byte char
            var truncated = new byte[maxBytes];
            Array.Copy(bytes, truncated, maxBytes);
            return Encoding.UTF8.GetString(truncated).TrimEnd('\uFFFD');
        }
    }
}
