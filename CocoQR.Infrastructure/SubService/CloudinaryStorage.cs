using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CocoQR.Application.Contracts.ISubServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocoQR.Infrastructure.SubService
{
    public class CloudinaryStorage : ICloudStorage
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;
        private readonly ILogger<CloudinaryStorage> _logger;
        private readonly bool _isValid;

        public CloudinaryStorage(IOptions<CloudinarySettings> options, ILogger<CloudinaryStorage> logger)
        {
            _settings = options.Value;
            _logger = logger;
            _isValid = ValidateSettings();

            var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task UploadAsync(Stream stream, string path)
        {
            EnsureConfigured();

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var normalizedPath = NormalizePath(path);
            if (string.IsNullOrWhiteSpace(normalizedPath))
                throw new ArgumentException("Path is required", nameof(path));

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(Path.GetFileName(normalizedPath), stream),
                PublicId = RemoveExtension(normalizedPath),
                Overwrite = true,
                UniqueFilename = false,
                UseFilename = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }
        }

        public async Task DeleteAsync(string path)
        {
            EnsureConfigured();

            var normalizedPath = NormalizePath(path);
            if (string.IsNullOrWhiteSpace(normalizedPath))
                return;

            var deletionParams = new DeletionParams(RemoveExtension(normalizedPath))
            {
                ResourceType = ResourceType.Raw
            };

            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);
            if (deletionResult.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary delete failed: {deletionResult.Error.Message}");
            }
        }

        public string GetPublicUrl(string path)
        {
            var normalizedPath = NormalizePath(path);
            if (string.IsNullOrWhiteSpace(normalizedPath))
                return string.Empty;

            var endpoint = (_settings.CdnEndpoint ?? string.Empty).Trim().TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                return $"{endpoint}/{normalizedPath}";
            }

            return normalizedPath;
        }

        private void EnsureConfigured()
        {
            if (!_isValid)
            {
                throw new InvalidOperationException("Cloudinary storage is not configured correctly.");
            }
        }

        private bool ValidateSettings()
        {
            var isValid = true;

            if (string.IsNullOrWhiteSpace(_settings.CloudName))
            {
                _logger.LogError("Cloudinary CloudName missing");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _logger.LogError("Cloudinary ApiKey missing");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiSecret))
            {
                _logger.LogError("Cloudinary ApiSecret missing");
                isValid = false;
            }

            if (!isValid)
            {
                _logger.LogWarning("Cloudinary storage disabled due to invalid config");
            }

            return isValid;
        }

        private static string RemoveExtension(string path)
        {
            var extension = Path.GetExtension(path);
            return string.IsNullOrWhiteSpace(extension)
                ? path
                : path[..^extension.Length];
        }

        private static string NormalizePath(string path)
        {
            return path
                .Replace('\\', '/')
                .TrimStart('/');
        }
    }
}