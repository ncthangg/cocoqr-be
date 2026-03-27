using System.Text;

namespace CocoQR.QR_Generator.Encoders
{
    /// <summary>
    /// CRC16-CCITT (polynomial 0x1021, init 0xFFFF) theo chuẩn EMVCo.
    ///
    /// ⚠️ Phải dùng UTF-8 encoding, KHÔNG dùng ASCII.
    ///    ASCII sẽ corrupt byte khi payload có tiếng Việt → CRC sai → QR không hợp lệ.
    ///
    /// Cách dùng:
    ///   payload = "000201...6304"   ← append "6304" nhưng CHƯA có CRC value
    ///   crc = CRC16Service.Compute(payload)
    ///   finalPayload = payload + crc  → "000201...6304F0A2"
    /// </summary>
    public static class CRC16Service
    {
        private const ushort Polynomial = 0x1021;
        private const ushort InitValue = 0xFFFF;

        /// <summary>
        /// Tính CRC16-CCITT của payload string.
        /// </summary>
        /// <param name="payload">Payload đã bao gồm "6304" ở cuối, CHƯA có 4 ký tự CRC</param>
        /// <returns>4 ký tự HEX uppercase, ví dụ "F0A2"</returns>
        public static string Compute(string payload)
        {
            var bytes = Encoding.UTF8.GetBytes(payload);
            var crc = InitValue;

            foreach (var b in bytes)
            {
                crc ^= (ushort)(b << 8);

                for (var i = 0; i < 8; i++)
                {
                    crc = (crc & 0x8000) != 0
                        ? (ushort)((crc << 1) ^ Polynomial)
                        : (ushort)(crc << 1);
                }
            }

            return crc.ToString("X4");
        }

        /// <summary>
        /// Verify CRC của một payload hoàn chỉnh (bao gồm 4 ký tự CRC ở cuối).
        /// </summary>
        /// <param name="fullPayload">Payload hoàn chỉnh kể cả 4 ký tự CRC</param>
        public static bool Verify(string fullPayload)
        {
            if (fullPayload.Length < 8) return false;

            // CRC nằm trong field "63" — luôn là 4 ký tự cuối
            var crcIndex = fullPayload.LastIndexOf("6304", StringComparison.Ordinal);
            if (crcIndex < 0) return false;

            var withoutCrc = fullPayload[..(crcIndex + 4)]; // bao gồm "6304"
            var rawCrc = fullPayload[(crcIndex + 4)..];
            var computedCrc = Compute(withoutCrc);

            return string.Equals(rawCrc, computedCrc, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Trích xuất CRC value từ payload hoàn chỉnh.
        /// </summary>
        public static string? Extract(string fullPayload)
        {
            var crcIndex = fullPayload.LastIndexOf("6304", StringComparison.Ordinal);
            if (crcIndex < 0 || crcIndex + 8 > fullPayload.Length) return null;
            return fullPayload[(crcIndex + 4)..];
        }
    }
}
