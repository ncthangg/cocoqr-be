namespace CocoQR.Application.Contracts.ISubServices
{
    public interface ICloudStorage
    {
        Task UploadAsync(Stream stream, string path);
        Task DeleteAsync(string path);
        string GetPublicUrl(string path);
    }
}
