using Microsoft.AspNetCore.Http;

namespace CocoQR.Application.Contracts.ISubServices
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<List<string>> UploadFilesAsync(IEnumerable<IFormFile> files, string folder);

        Task DeleteFileAsync(string filePath);
        Task DeleteFilesAsync(IEnumerable<string> filePaths);

        Task<bool> FileExistsAsync(string filePath);

        Task<Stream> GetFileStreamAsync(string filePath);
        string GetPhysicalPath(string filePath);
    }
}
