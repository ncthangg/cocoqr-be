using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Domain.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DomainException = CocoQR.Domain.Exceptions.DomainException;

namespace CocoQR.Infrastructure.SubService
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileStorageService> _logger;

        private readonly DigitalOceanSettings _settings;

        private readonly IAmazonS3 _client;
        public FileStorageService(
            IWebHostEnvironment environment,
            ILogger<FileStorageService> logger,
            IOptions<DigitalOceanSettings> options)
        {
            _env = environment;
            _logger = logger;
            _settings = options.Value;
        }
        private IAmazonS3 GetClient()
        {
            var configS3 = new AmazonS3Config
            {
                ServiceURL = _settings.Endpoint,
                ForcePathStyle = false
            };

            return new AmazonS3Client(
                _settings.AccessKey,
                _settings.SecretKey,
                configS3);
        }
        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            ValidateFile(file);

            try
            {
                if (_env.IsProduction())
                {
                    await UploadFileToCloudAsync(file, folder);
                    return await UploadFileToLocalAsync(file, folder);
                }
                else
                {
                    return await UploadFileToLocalAsync(file, folder);
                }
            }
            catch (DomainException)
            {
                throw;
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
        public async Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            try
            {
                await DeleteFileLocalAsync(filePath);

                if (_env.IsProduction())
                {
                    await DeleteFileOnCloudAsync(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
            }
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
            return Path.Combine(_env.WebRootPath, filePath);
        }

        #region
        private static void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new DomainException(ErrorCode.BadRequest, "File is empty or not provided");
            }

            if (file.Length > FileStorage.MaxFileSize)
            {
                throw new DomainException(ErrorCode.BadRequest, $"File size exceeds maximum allowed size of {FileStorage.MaxFileSize / (1024 * 1024)} MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!IsAllowedExtension(extension))
            {
                throw new DomainException(ErrorCode.BadRequest, $"File type '{extension}' is not allowed");
            }
        }

        private async Task<string> UploadFileToLocalAsync(IFormFile file, string folder)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";

            var uploadPath = Path.Combine(_env.WebRootPath, folder);
            var relativePath = $"{folder}/{uniqueFileName}";

            Directory.CreateDirectory(uploadPath);

            var physicalPath = Path.Combine(uploadPath, uniqueFileName);

            await using (var stream = new FileStream(physicalPath,
                                                     FileMode.CreateNew,
                                                     FileAccess.Write,
                                                     FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File uploaded successfully to local: {FileName} -> {RelativePath}",
                                   file.FileName,
                                   relativePath);

            return relativePath;
        }

        private async Task<string> UploadFileToCloudAsync(IFormFile file, string folder)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using var stream = file.OpenReadStream();

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = $"{folder}/{fileName}",
                BucketName = $"{_settings.Bucket}",
                CannedACL = S3CannedACL.PublicRead
            };

            var client = GetClient();
            var transferUtility = new TransferUtility(client);
            await transferUtility.UploadAsync(uploadRequest);

            var fileUrl = $"{_settings.Endpoint}/{folder}/{fileName}";
            _logger.LogInformation("File uploaded successfully to cloud: {FileName} -> {FileUrl}", file.FileName, fileUrl);

            return fileUrl;
        }

        private Task DeleteFileLocalAsync(string filePath)
        {
            var relativePath = GetRelativePathFromUrlOrPath(filePath);
            var physicalPath = GetPhysicalPath(relativePath);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                _logger.LogInformation("Local file deleted: {FilePath}", relativePath);
            }
            else
            {
                _logger.LogWarning("Local file not found for deletion: {FilePath}", relativePath);
            }

            return Task.CompletedTask;
        }

        private async Task DeleteFileOnCloudAsync(string fileUrlOrPath)
        {
            if (string.IsNullOrWhiteSpace(fileUrlOrPath))
                return;

            try
            {
                var key = GetRelativePathFromUrlOrPath(fileUrlOrPath);
                if (string.IsNullOrWhiteSpace(key))
                    return;

                var request = new DeleteObjectRequest
                {
                    BucketName = _settings.Bucket,
                    Key = key
                };

                await _client.DeleteObjectAsync(request);

                _logger.LogInformation("Cloud file deleted: {FilePath}", fileUrlOrPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete cloud file: {FilePath}", fileUrlOrPath);
            }
        }

        private string GetRelativePathFromUrlOrPath(string fileUrlOrPath)
        {
            if (string.IsNullOrWhiteSpace(fileUrlOrPath))
                return string.Empty;

            if (Uri.TryCreate(fileUrlOrPath, UriKind.Absolute, out var uri))
            {
                var path = uri.AbsolutePath.TrimStart('/');
                var bucketPrefix = $"{_settings.Bucket}/";

                if (path.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    path = path[bucketPrefix.Length..];
                }

                return path;
            }

            return fileUrlOrPath.TrimStart('/');
        }

        /// <summary>
        /// Validates file extension
        /// </summary>
        private static bool IsAllowedExtension(string extension)
        {
            return FileStorage.AllowedImageExtensions.Contains(extension)
                || FileStorage.AllowedDocumentExtensions.Contains(extension);
        }
        #endregion
    }
}
