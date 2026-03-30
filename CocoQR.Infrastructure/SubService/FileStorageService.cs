using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Domain.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static CocoQR.Domain.Constants.FileStorage;
using DomainException = CocoQR.Domain.Exceptions.DomainException;

namespace CocoQR.Infrastructure.SubService
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileStorageService> _logger;
        private readonly ICloudStorage _cloudStorage;
        private readonly IFileCleanupQueue _cleanupQueue;
        private readonly string _baseUrl;

        public FileStorageService(
            IWebHostEnvironment environment,
            ILogger<FileStorageService> logger,
            ICloudStorage cloudStorage,
            IFileCleanupQueue cleanupQueue,
            IConfiguration configuration)
        {
            _env = environment;
            _logger = logger;
            _cloudStorage = cloudStorage;
            _cleanupQueue = cleanupQueue;
            _baseUrl = (configuration["FileUrl:BaseUrl"] ?? string.Empty).Trim().TrimEnd('/');
        }

        #region FUNCTIONS FOR 1 FILE ONLY
        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            ValidateFile(file);

            var relativePath = BuildRelativePath(folder, file.FileName);

            try
            {
                if (_env.IsDevelopment())
                {
                    await UploadFileToLocalAsync(file, relativePath);
                    return GetFileUrl(relativePath);
                }

                if (ShouldUseCloudStorage())
                {
                    await UploadFileToCloudAsync(file, relativePath);

                    try
                    {
                        await UploadFileToLocalAsync(file, relativePath);
                    }
                    catch (Exception localEx)
                    {
                        await ProcessCleanupRequestAsync(
                            new FileCleanupRequest
                            {
                                FilePath = relativePath,
                                DeleteCloud = true,
                                DeleteLocal = false,
                                Attempt = 1
                            },
                            enqueueOnFailure: true);

                        throw new DomainException(ErrorCode.InternalError, "Uploaded cloud but failed to save local file", localEx);
                    }

                    return GetFileUrl(relativePath);
                }

                throw new DomainException(ErrorCode.InternalError, "Unsupported environment for file upload");
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file");
                throw new DomainException(ErrorCode.InternalError, "Failed to upload file");
            }
        }
        public async Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            await ProcessCleanupRequestAsync(
                new FileCleanupRequest
                {
                    FilePath = filePath,
                    DeleteCloud = ShouldUseCloudStorage(),
                    DeleteLocal = true,
                    Attempt = 1
                },
                enqueueOnFailure: true);
        }
        public async Task UploadLogFileToCloudAsync(string filePath)
        {
            if (!ShouldUseCloudStorage())
                return;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new DomainException(ErrorCode.BadRequest, "File path is required");
            }

            if (!File.Exists(filePath))
            {
                throw new DomainException(ErrorCode.NotFound, $"File not found: {filePath}");
            }

            try
            {
                var logRoot = GetLogRootPath();
                var relativePath = NormalizePath(Path.GetRelativePath(logRoot, filePath));

                if (relativePath.StartsWith("..", StringComparison.Ordinal))
                {
                    throw new DomainException(ErrorCode.BadRequest, $"Log file is outside configured log folder: {filePath}");
                }

                var key = BuildCloudKey($"{Folders.Logs}/{relativePath}");
                await using var stream = File.OpenRead(filePath);
                await _cloudStorage.UploadAsync(stream, key);

                _logger.LogInformation("Log uploaded: {Key}", key);
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DomainException(ErrorCode.InternalError, "Failed to upload log file", ex);
            }
        }
        #endregion
       
        #region FUNCTIONS FOR MULTIPLE FILES
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
        /// Deletes multiple files
        /// </summary>
        public async Task DeleteFilesAsync(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                await DeleteFileAsync(filePath);
            }
        }
        #endregion

        /// <summary>
        /// Checks if file exists
        /// </summary>
        public Task<bool> FileExistsAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Task.FromResult(false);

            var relativePath = StripEnvironmentPrefix(GetRelativePathFromUrlOrPath(filePath));
            var physicalPath = GetPhysicalPath(relativePath);
            return Task.FromResult(File.Exists(physicalPath));
        }

        public string GetFileUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            if (Uri.TryCreate(path, UriKind.Absolute, out _))
            {
                if (!ShouldUseCloudStorage())
                {
                    return path;
                }

                var relativePath = GetRelativePathFromUrlOrPath(path);
                return string.IsNullOrWhiteSpace(relativePath)
                    ? path
                    : _cloudStorage.GetPublicUrl(BuildCloudKey(relativePath));
            }

            var normalizedPath = TrimToEnvironmentPrefix(path);

            if (ShouldUseCloudStorage())
            {
                return _cloudStorage.GetPublicUrl(BuildCloudKey(normalizedPath));
            }

            if (!string.IsNullOrWhiteSpace(_baseUrl))
            {
                return BuildUrl(_baseUrl, StripEnvironmentPrefix(normalizedPath));
            }

            return NormalizePath(StripEnvironmentPrefix(normalizedPath));
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

            var relativePath = StripEnvironmentPrefix(GetRelativePathFromUrlOrPath(filePath));
            var physicalPath = GetPhysicalPath(relativePath);

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

            filePath = NormalizePath(filePath);

            return Path.Combine(_env.WebRootPath, filePath.Replace('/', Path.DirectorySeparatorChar));
        }

        #region HELPERS
        internal async Task<FileCleanupRequest?> ProcessCleanupRequestAsync(
            FileCleanupRequest request,
            bool enqueueOnFailure,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                return null;

            var normalizedPath = GetRelativePathFromUrlOrPath(request.FilePath);
            if (string.IsNullOrWhiteSpace(normalizedPath))
                return null;

            var cloudDeleted = !request.DeleteCloud || !ShouldUseCloudStorage();
            var localDeleted = !request.DeleteLocal;

            if (request.DeleteCloud && ShouldUseCloudStorage())
            {
                var key = BuildCloudKey(normalizedPath);
                cloudDeleted = await TryDeleteFileOnCloudAsync(key, request.FilePath, cancellationToken);
            }

            if (request.DeleteLocal)
            {
                var localPath = StripEnvironmentPrefix(normalizedPath);
                localDeleted = TryDeleteFileLocal(localPath);
            }

            if (cloudDeleted && localDeleted)
            {
                return null;
            }

            var retryRequest = new FileCleanupRequest
            {
                FilePath = normalizedPath,
                DeleteCloud = !cloudDeleted && request.DeleteCloud,
                DeleteLocal = !localDeleted && request.DeleteLocal,
                Attempt = request.Attempt + 1
            };

            _logger.LogWarning(
                "File cleanup incomplete. File: {FilePath}, RetryCloud: {RetryCloud}, RetryLocal: {RetryLocal}, Attempt: {Attempt}",
                retryRequest.FilePath,
                retryRequest.DeleteCloud,
                retryRequest.DeleteLocal,
                retryRequest.Attempt);

            if (enqueueOnFailure)
            {
                await _cleanupQueue.EnqueueAsync(retryRequest, cancellationToken);
            }

            return retryRequest;
        }

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

        private async Task<string> UploadFileToLocalAsync(IFormFile file, string relativePath)
        {
            var physicalPath = Path.Combine(_env.WebRootPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

            await using (var stream = new FileStream(physicalPath,
                                                     FileMode.CreateNew,
                                                     FileAccess.Write,
                                                     FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Uploaded local: {Path}", relativePath);

            return relativePath;
        }

        private async Task UploadFileToCloudAsync(IFormFile file, string relativePath)
        {
            using var stream = file.OpenReadStream();

            var key = BuildCloudKey(relativePath);

            await _cloudStorage.UploadAsync(stream, key);

            var url = _cloudStorage.GetPublicUrl(key);
            _logger.LogInformation("Uploaded cloud: {Url}", url);
        }

        private bool TryDeleteFileLocal(string relativePath)
        {
            var physicalPath = GetPhysicalPath(relativePath);

            try
            {
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    _logger.LogInformation("Local file deleted: {FilePath}", relativePath);
                }
                else
                {
                    _logger.LogInformation("Local file already removed: {FilePath}", relativePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete local file: {FilePath}", relativePath);
                return false;
            }
        }

        private async Task<bool> TryDeleteFileOnCloudAsync(string key, string fileUrlOrPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _cloudStorage.DeleteAsync(key);

                _logger.LogInformation("Cloud file deleted: {Key}", key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete cloud file: {FilePath}", fileUrlOrPath);
                return false;
            }
        }

        private string GetRelativePathFromUrlOrPath(string fileUrlOrPath)
        {
            if (string.IsNullOrWhiteSpace(fileUrlOrPath))
                return string.Empty;

            if (Uri.TryCreate(fileUrlOrPath, UriKind.Absolute, out var uri))
            {
                return TrimToEnvironmentPrefix(uri.AbsolutePath);
            }

            return TrimToEnvironmentPrefix(fileUrlOrPath);
        }

        private static bool IsAllowedExtension(string extension)
        {
            return FileStorage.AllowedImageExtensions.Contains(extension)
                || FileStorage.AllowedDocumentExtensions.Contains(extension);
        }

        private bool ShouldUseCloudStorage()
        {
            return _env.IsStaging() || _env.IsProduction();
        }

        private string BuildCloudKey(string path)
        {
            var normalized = TrimToEnvironmentPrefix(path);
            var envPrefix = GetEnvironmentPrefix();

            if (normalized.StartsWith(envPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            return $"{envPrefix}{normalized}";
        }

        private string StripEnvironmentPrefix(string path)
        {
            var normalized = NormalizePath(path);
            var envPrefix = GetEnvironmentPrefix();

            if (normalized.StartsWith(envPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return normalized[envPrefix.Length..];
            }

            return normalized;
        }

        private string GetEnvironmentPrefix()
        {
            return $"{_env.EnvironmentName.ToLowerInvariant()}/";
        }

        private string TrimToEnvironmentPrefix(string path)
        {
            var normalized = NormalizePath(path);
            var envPrefix = GetEnvironmentPrefix();
            var envPrefixIndex = normalized.IndexOf(envPrefix, StringComparison.OrdinalIgnoreCase);

            if (envPrefixIndex > 0)
            {
                return normalized[envPrefixIndex..];
            }

            return normalized;
        }

        private static string BuildRelativePath(string folder, string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            return $"{NormalizePath(folder)}/{fileName}";
        }

        private static string BuildUrl(string endpoint, string path)
        {
            return $"{endpoint.TrimEnd('/')}/{NormalizePath(path)}";
        }

        private static string NormalizePath(string path)
        {
            return path
                .Replace('\\', '/')
                .TrimStart('/');
        }

        private string GetLogRootPath()
        {
            var configuredLogPath = Environment.GetEnvironmentVariable(EnvKeys.Logs);
            if (!string.IsNullOrWhiteSpace(configuredLogPath))
            {
                if (Path.IsPathRooted(configuredLogPath))
                {
                    return Path.GetFullPath(configuredLogPath);
                }

                return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredLogPath));
            }

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, Folders.Logs));
        }
        #endregion
    }
}
