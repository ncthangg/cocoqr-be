using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyWallet.Application.Contracts.ISubServices;
using MyWallet.Domain.Constants;
using System.Runtime.CompilerServices;
using DomainException = MyWallet.Domain.Exceptions.DomainException;

namespace MyWallet.Infrastructure.SubService
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(
            IWebHostEnvironment environment,
            ILogger<LocalFileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Uploads a file to local storage
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                throw new DomainException(ErrorCode.BadRequest, "File is empty or not provided");
            }

            if (file.Length > FileStorage.MaxFileSize)
            {
                throw new DomainException(ErrorCode.BadRequest, $"File size exceeds maximum allowed size of {FileStorage.MaxFileSize / (1024 * 1024)} MB");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!IsAllowedExtension(extension))
            {
                throw new DomainException(ErrorCode.BadRequest, $"File type '{extension}' is not allowed");
            }

            try
            {
                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid():N}{extension}";

                string uploadPath;
                string relativePath;

                if (folder == FileStorage.Folders.QRs)
                {
                    // Create directory path: wwwroot/{folder}/{year-month}
                    var yearMonth = DateTime.UtcNow.ToString("yyyy-MM");
                    uploadPath = Path.Combine(_environment.WebRootPath, folder, yearMonth);
                    relativePath = $"{folder}/{yearMonth}/{uniqueFileName}";
                }
                else
                {
                    uploadPath = Path.Combine(_environment.WebRootPath, folder);
                    relativePath = $"{folder}/{uniqueFileName}";
                }


                Directory.CreateDirectory(uploadPath);

                var physicalPath = Path.Combine(uploadPath, uniqueFileName);


                // Save file
                await using (var stream = new FileStream(physicalPath,
                                                         FileMode.CreateNew,
                                                         FileAccess.Write,
                                                         FileShare.None))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File uploaded successfully: {FileName} -> {RelativePath}",
                                       file.FileName,
                                       relativePath);

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file: {FileName}", file.FileName);
                throw new DomainException(ErrorCode.InternalError, "Failed to upload file", ex);
            }
        }

        /// <summary>
        /// Uploads multiple files
        /// </summary>
        public async Task<List<string>> UploadFilesAsync(IEnumerable<IFormFile> files, string folder)
        {
            var uploadedPaths = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var path = await UploadFileAsync(file, folder);
                    uploadedPaths.Add(path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload file: {FileName}", file.FileName);

                    // Rollback: delete already uploaded files
                    foreach (var uploadedPath in uploadedPaths)
                    {
                        await DeleteFileAsync(uploadedPath);
                    }

                    throw;
                }
            }

            return uploadedPaths;
        }

        /// <summary>
        /// Deletes a file from storage
        /// </summary>
        public Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Task.CompletedTask;

            try
            {
                var physicalPath = GetPhysicalPath(filePath);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    _logger.LogInformation("File deleted: {FilePath}", filePath);
                }
                else
                {
                    _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
                // Don't throw - deletion failure shouldn't break the application
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes multiple files
        /// </summary>
        public async Task DeleteFilesAsync(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                await DeleteFileAsync(filePath);
            }
        }

        /// <summary>
        /// Checks if file exists
        /// </summary>
        public Task<bool> FileExistsAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Task.FromResult(false);

            var physicalPath = GetPhysicalPath(filePath);
            return Task.FromResult(File.Exists(physicalPath));
        }

        /// <summary>
        /// Gets file stream for reading/downloading
        /// </summary>
        public Task<Stream> GetFileStreamAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new DomainException(ErrorCode.BadRequest, "File path is required");
            }

            var physicalPath = GetPhysicalPath(filePath);

            if (!File.Exists(physicalPath))
            {
                throw new DomainException(ErrorCode.NotFound, $"File not found: {filePath}");
            }

            var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream>(stream);
        }

        /// <summary>
        /// Gets full physical path from relative path
        /// </summary>
        public string GetPhysicalPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            // Remove leading slash if present
            filePath = filePath.TrimStart('/');

            // Combine with WebRootPath
            return Path.Combine(_environment.WebRootPath, filePath);
        }

        /// <summary>
        /// Validates file extension
        /// </summary>
        private static bool IsAllowedExtension(string extension)
        {
            return FileStorage.AllowedImageExtensions.Contains(extension)
                || FileStorage.AllowedDocumentExtensions.Contains(extension);
        }
    }
}
