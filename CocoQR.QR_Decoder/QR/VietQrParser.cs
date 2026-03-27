using CocoQR.QR_Decoder.QR.Dto;
using System.Text;

namespace CocoQR.QR_Decoder.QR
{
    public static class VietQrParser
    {
        public static VietQrInfo Parse(string payload)
        {
            var info = new VietQrInfo
            {
                RawPayload = payload
            };

            var rootTags = ParseTLV(payload);

            ParseMerchantTemplates(rootTags, info);

            if (rootTags.TryGetValue("54", out var amount))
                info.Amount = amount;

            if (rootTags.TryGetValue("59", out var name))
                info.AccountName = name;

            if (rootTags.TryGetValue("62", out var additional))
                ParseAdditional(additional, info);

            if (rootTags.TryGetValue("63", out var crc))
                info.CRCValid = ValidateCRC(payload, crc);

            return info;
        }

        private static void ParseMerchantTemplates(Dictionary<string, string> root, VietQrInfo info)
        {
            foreach (var tag in root)
            {
                int id = int.Parse(tag.Key);

                if (id < 26 || id > 51)
                    continue;

                ParseMerchantAccount(tag.Value, info);
            }
        }

        private static void ParseMerchantAccount(string value, VietQrInfo info)
        {
            var merchantFields = ParseTLV(value);

            if (merchantFields.TryGetValue("00", out var napasRid))
                info.NapasRid = napasRid;

            if (!merchantFields.TryGetValue("01", out var accountBlock))
                return;

            var accountFields = ParseTLV(accountBlock);

            if (accountFields.TryGetValue("00", out var bin))
            {
                info.BankBin = bin;
                (info.BankCode, info.BankName) = BankLookup.GetBank(bin);
            }

            if (accountFields.TryGetValue("01", out var acc))
                info.AccountNumber = acc;

            if (accountFields.TryGetValue("02", out var service))
                info.TransferType = MapTransferType(service);

            if (merchantFields.TryGetValue("02", out var service2))
                info.TransferType = MapTransferType(service2);
        }

        private static void ParseAdditional(string additional, VietQrInfo info)
        {
            var tags = ParseTLV(additional);

            if (tags.TryGetValue("08", out var note))
                info.Note = note;
        }

        public static Dictionary<string, string> ParseTLV(string payload)
        {
            var result = new Dictionary<string, string>();

            int index = 0;

            while (index < payload.Length)
            {
                if (index + 4 > payload.Length)
                    break;

                string tag = payload.Substring(index, 2);

                int length = int.Parse(payload.Substring(index + 2, 2));

                string value = payload.Substring(index + 4, length);

                result[tag] = value;

                index += 4 + length;
            }

            return result;
        }

        private static bool ValidateCRC(string payload, string crcValue)
        {
            var data = payload.Substring(0, payload.Length - 4);

            var crc = CRC16(data + "6304");

            return crc.Equals(crcValue, StringComparison.OrdinalIgnoreCase);
        }

        private static string CRC16(string input)
        {
            ushort crc = 0xFFFF;

            foreach (byte b in Encoding.ASCII.GetBytes(input))
            {
                crc ^= (ushort)(b << 8);

                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }

            return crc.ToString("X4");
        }

        private static string MapTransferType(string code)
        {
            return code switch
            {
                "QRIBFTTA" => "AccountTransfer",
                "QRIBFTTC" => "MerchantPayment",
                _ => code
            };
        }
    }
}
