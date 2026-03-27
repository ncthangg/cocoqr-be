namespace CocoQR.QR_Decoder.QR
{
    public static class BankLookup
    {
        public static readonly IReadOnlyDictionary<string, (string BankCode, string BankName)> BinToBank =
    new Dictionary<string, (string, string)>
{
        { "970425", ("ABB", "Ngân hàng TMCP An Bình") },
        { "970416", ("ACB", "Ngân hàng TMCP Á Châu") },
        { "970405", ("AGR", "Ngân hàng Nông nghiệp và Phát triển Nông thôn Việt Nam") },
        { "970409", ("BAB", "Ngân hàng TMCP Bắc Á") },
        { "970418", ("BIDV", "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam") },
        { "970438", ("BVB", "Ngân hàng TMCP Bảo Việt") },
        { "970444", ("CBB", "Ngân hàng Thương mại TNHH MTV Xây dựng Việt Nam") },
        { "970406", ("DAB", "Ngân hàng TMCP Đông Á") },
        { "970431", ("EIB", "Ngân hàng TMCP Xuất Nhập Khẩu Việt Nam") },
        { "970408", ("GPB", "Ngân hàng Thương mại TNHH MTV Dầu Khí Toàn Cầu") },
        { "970437", ("HDB", "Ngân hàng TMCP Phát Triển Thành Phố Hồ Chí Minh") },
        { "970442", ("HLB", "Ngân hàng TNHH MTV Hong Leong Việt Nam") },
        { "970434", ("IVB", "Ngân hàng TNHH Indovina") },
        { "970452", ("KLB", "Ngân hàng TMCP Kiên Long") },
        { "970449", ("LPB", "Ngân hàng TMCP Bưu Điện Liên Việt") },
        { "970422", ("MBB", "Ngân hàng TMCP Quân Đội") },
        { "970426", ("MSB", "Ngân hàng TMCP Hàng Hải Việt Nam") },
        { "970428", ("NAB", "Ngân hàng TMCP Nam Á") },
        { "970419", ("NCB", "Ngân hàng TMCP Quốc Dân") },
        { "970448", ("OCB", "Ngân hàng TMCP Phương Đông") },
        { "970414", ("OJB", "Ngân hàng TNHH MTV Đại Dương") },
        { "970430", ("PGB", "Ngân hàng TMCP Xăng dầu Petrolimex") },
        { "970412", ("PVCB", "Ngân hàng TMCP Đại Chúng Việt Nam") },
        { "970429", ("SCB", "Ngân hàng TMCP Sài Gòn") },
        { "970440", ("SEAB", "Ngân hàng TMCP Đông Nam Á") },
        { "970400", ("SGB", "Ngân hàng TMCP Sài Gòn Công Thương") },
        { "970443", ("SHB", "Ngân hàng TMCP Sài Gòn - Hà Nội") },
        { "970403", ("STB", "Ngân hàng TMCP Sài Gòn Thương Tín") },
        { "970407", ("TCB", "Ngân hàng TMCP Kỹ Thương Việt Nam") },
        { "970423", ("TPB", "Ngân hàng TMCP Tiên Phong") },
        { "970427", ("VAB", "Ngân hàng TMCP Việt Á") },
        { "970454", ("VBB", "Ngân hàng TMCP Bản Việt") },
        { "970441", ("VIB", "Ngân hàng TMCP Quốc Tế Việt Nam") },
        { "970433", ("VietBank", "Ngân hàng TMCP Việt Nam Thương Tín") },
        { "970436", ("VCB", "Ngân hàng TMCP Ngoại Thương Việt Nam") },
        { "970415", ("VTB", "Ngân hàng TMCP Công Thương Việt Nam") },
        { "970432", ("VPB", "Ngân hàng TMCP Việt Nam Thịnh Vượng") },
        { "970421", ("VRB", "Ngân hàng Liên doanh Việt - Nga") },
        { "970446", ("COOP", "Ngân hàng Hợp tác xã Việt Nam") },
        { "970458", ("UOB", "Ngân hàng United Overseas Bank Việt Nam") }
};
        public static (string BankCode, string BankName) GetBank(string bin)
        {
            if (BinToBank.TryGetValue(bin, out var bank))
                return bank;

            return ("UNKNOWN", "Unknown Bank");
        }
    }
}
