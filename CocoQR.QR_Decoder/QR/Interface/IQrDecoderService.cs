using CocoQR.QR_Decoder.QR.Dto;

namespace CocoQR.QR_Decoder.QR.Interface
{
    public interface IQrDecoderService
    {
        Task<string> DecodeAsync(Stream imageStream);
        VietQrInfo ParsePayload(string payload);
    }
}
