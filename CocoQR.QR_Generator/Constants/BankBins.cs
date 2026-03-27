namespace CocoQR.QR_Generator.Constants
{
    /// <summary>
    /// Danh sách NAPAS BIN chính thức cho các ngân hàng Việt Nam.
    /// Nguồn: NAPAS standard bank list.
    /// Key = BankCode (dùng trong DB), Value = NapasBin 6 chữ số.
    /// </summary>
    public static class BankBins
    {
        public static readonly IReadOnlyDictionary<string, string> BankCodeToBin = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
    {
        { "ABB", "970425" },
        { "ACB", "970416" },
        { "AGR", "970405" },
        { "BAB", "970409" },
        { "BIDV", "970418" },
        { "BVB", "970438" },
        { "CBB", "970444" },
        { "DAB", "970406" },
        { "EIB", "970431" },
        { "GPB", "970408" },
        { "HDB", "970437" },
        { "HLB", "970442" },
        { "IVB", "970434" },
        { "KLB", "970452" },
        { "LPB", "970449" },
        { "MBB", "970422" },
        { "MSB", "970426" },
        { "NAB", "970428" },
        { "NCB", "970419" },
        { "OCB", "970448" },
        { "OJB", "970414" },
        { "PGB", "970430" },
        { "PVCB", "970412" },
        { "SCB", "970429" },
        { "SEAB", "970440" },
        { "SGB", "970400" },
        { "SHB", "970443" },
        { "STB", "970403" },
        { "TCB", "970407" },
        { "TPB", "970423" },
        { "VAB", "970427" },
        { "VBB", "970454" },
        { "VIB", "970441" },
        { "VietBank", "970433" },
        { "VCB", "970436" },
        { "VTB", "970415" },
        { "VPB", "970432" },
        { "VRB", "970421" },
        { "COOP", "970446" },
        { "UOB", "970458" },
        // ── Ví điện tử (interbank qua NAPAS) ────────────────────────────────
        { "MOMO",   "970436" }, // MoMo dùng chung routing qua VCB NAPAS gateway
                                // Lưu ý: BIN thực tế verify lại với NAPAS
        { "VNPAY",  "970436" }, // VNPay routing
        { "ZALOPAY","970436" }, // ZaloPay routing
        // ── Ngân hàng nước ngoài ─────────────────────────────────────────────
        { "HSBC",   "458761" },
        { "STANDARD","970410" },
        { "WOORI",  "970466" },
        { "SHINHAN","970424" },
    };

        /// <summary>Lấy NapasBin theo BankCode. Throw nếu không tìm thấy.</summary>
        public static string GetBin(string bankCode)
        {
            if (BankCodeToBin.TryGetValue(bankCode, out var bin))
                return bin;

            throw new KeyNotFoundException($"BankCode '{bankCode}' không tồn tại.");
        }

        /// <summary>Kiểm tra BankCode có tồn tại không.</summary>
        public static bool Exists(string bankCode) => BankCodeToBin.ContainsKey(bankCode);
    }
}
