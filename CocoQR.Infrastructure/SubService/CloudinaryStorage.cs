using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CocoQR.Application.Contracts.IConfigs;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Domain.Constants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Infrastructure.SubService
{
    public class CloudinaryStorage : ICloudStorage
    {
        private readonly ICloudinaryConfiguration _settings;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<CloudinaryStorage> _logger;
        private readonly bool _isValid;

        public CloudinaryStorage(
            ICloudinaryConfiguration settings,
            IHostEnvironment hostEnvironment,
            ILogger<CloudinaryStorage> logger)
        {
            _settings = settings;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _isValid = ValidateSettings();
        }

        public async Task UploadAsync(Stream stream, string path)
        {
            EnsureConfigured();

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var storagePath = BuildStoragePath(path);
            if (string.IsNullOrWhiteSpace(storagePath))
                throw new ArgumentException(ValidationMessages.RequiredPath, nameof(path));

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var client = GetClient();
            var publicId = GetPublicId(storagePath);
            var assetFolder = GetAssetFolder(storagePath);
            var resourceType = ResolveResourceType(storagePath);

            UploadResult uploadResult = resourceType switch
            {
                ResourceType.Image => await client.UploadAsync(new ImageUploadParams
                {
                    File = new FileDescription(Path.GetFileName(storagePath), stream),
                    PublicId = publicId,
                    AssetFolder = assetFolder,
                    Overwrite = true,
                    UniqueFilename = false,
                    UseFilename = false
                }),
                ResourceType.Video => await client.UploadAsync(new VideoUploadParams
                {
                    File = new FileDescription(Path.GetFileName(storagePath), stream),
                    PublicId = publicId,
                    AssetFolder = assetFolder,
                    Overwrite = true,
                    UniqueFilename = false,
                    UseFilename = false
                }),
                _ => await client.UploadAsync(new RawUploadParams
                {
                    File = new FileDescription(Path.GetFileName(storagePath), stream),
                    PublicId = publicId,
                    AssetFolder = assetFolder,
                    Overwrite = true,
                    UniqueFilename = false,
                    UseFilename = false
                })
            };

            if (uploadResult.Error != null)
            {
                throw new ApplicationException(
                    ErrorCode.ServiceUnavailable,
                    ErrorMessages.CloudinaryUploadFailed,
                    data: new
                    {
                        Provider = "Cloudinary",
                        Error = uploadResult.Error.Message
                    });
            }
        }

        public async Task DeleteAsync(string path)
        {
            EnsureConfigured();

            if (TryParseCloudinaryUrl(path, out var publicIdFromUrl, out var resourceTypeFromUrl))
            {
                var deletionResultFromUrl = await GetClient().DestroyAsync(new DeletionParams(publicIdFromUrl)
                {
                    ResourceType = resourceTypeFromUrl
                });

                if (deletionResultFromUrl.Error != null)
                {
                    throw new ApplicationException(
                        ErrorCode.ServiceUnavailable,
                        ErrorMessages.CloudinaryDeleteFailed,
                        data: new
                        {
                            Provider = "Cloudinary",
                            Error = deletionResultFromUrl.Error.Message
                        });
                }

                return;
            }

            var storagePath = BuildStoragePath(path);
            if (string.IsNullOrWhiteSpace(storagePath))
                return;

            var deletionParams = new DeletionParams(GetPublicId(storagePath))
            {
                ResourceType = ResolveResourceType(storagePath)
            };

            var deletionResult = await GetClient().DestroyAsync(deletionParams);
            if (deletionResult.Error != null)
            {
                throw new ApplicationException(
                    ErrorCode.ServiceUnavailable,
                    ErrorMessages.CloudinaryDeleteFailed,
                    data: new
                    {
                        Provider = "Cloudinary",
                        Error = deletionResult.Error.Message
                    });
            }
        }

        public string GetPublicUrl(string path)
        {
            if (Uri.TryCreate(path, UriKind.Absolute, out _))
                return path;

            var storagePath = BuildStoragePath(path);
            if (string.IsNullOrWhiteSpace(storagePath))
                return string.Empty;

            var resourceTypeSegment = GetResourceTypeSegment(ResolveResourceType(storagePath));
            var publicId = GetPublicId(storagePath);
            return $"{GetPublicBaseUrl()}/{resourceTypeSegment}/{CloudinaryConfig.DeliveryTypeUpload}/{publicId}";
        }

        private void EnsureConfigured()
        {
            if (!_isValid)
            {
                throw new ApplicationException(ErrorCode.ServiceUnavailable, ErrorMessages.CloudinaryStorageNotConfigured);
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

            if (string.IsNullOrWhiteSpace(_settings.ProjectName))
            {
                _logger.LogError("Cloudinary ProjectName missing");
                isValid = false;
            }

            if (!isValid)
            {
                _logger.LogWarning("Cloudinary storage disabled due to invalid config");
            }

            return isValid;
        }

        private Cloudinary GetClient()
        {
            var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
            return new Cloudinary(account);
        }

        private string BuildStoragePath(string path)
        {
            var normalizedPath = NormalizePath(path);
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return string.Empty;
            }

            var envSegment = NormalizePath(_hostEnvironment.EnvironmentName.ToLowerInvariant());
            var projectSegment = NormalizePath(_settings.ProjectName);

            if (!string.IsNullOrWhiteSpace(envSegment)
                && !normalizedPath.StartsWith($"{envSegment}/", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(normalizedPath, envSegment, StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = $"{envSegment}/{normalizedPath}";
            }

            if (!string.IsNullOrWhiteSpace(projectSegment)
                && !normalizedPath.StartsWith($"{projectSegment}/", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(normalizedPath, projectSegment, StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = $"{projectSegment}/{normalizedPath}";
            }

            return normalizedPath;
        }

        private string GetPublicBaseUrl()
        {
            var cloudName = NormalizePath(_settings.CloudName);
            var configuredBaseUrl = (_settings.BaseUrl ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(configuredBaseUrl))
            {
                return $"{CloudinaryConfig.DefaultBaseUrl}/{cloudName}";
            }

            if (!Uri.TryCreate(configuredBaseUrl, UriKind.Absolute, out var baseUri))
            {
                return $"{CloudinaryConfig.DefaultBaseUrl}/{cloudName}";
            }

            var segments = baseUri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizePath)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (segments.Count >= 2
                && string.Equals(segments[^1], CloudinaryConfig.DeliveryTypeUpload, StringComparison.OrdinalIgnoreCase)
                && IsResourceTypeSegment(segments[^2]))
            {
                segments.RemoveRange(segments.Count - 2, 2);
            }

            if (segments.Count == 0 || !string.Equals(segments[0], cloudName, StringComparison.OrdinalIgnoreCase))
            {
                segments.Insert(0, cloudName);
            }

            var builder = new UriBuilder(baseUri)
            {
                Path = string.Join('/', segments),
                Query = string.Empty,
                Fragment = string.Empty
            };

            return builder.Uri.ToString().TrimEnd('/');
        }

        private static bool IsResourceTypeSegment(string segment)
        {
            return string.Equals(segment, CloudinaryConfig.ResourceTypeRaw, StringComparison.OrdinalIgnoreCase)
                || string.Equals(segment, CloudinaryConfig.ResourceTypeImage, StringComparison.OrdinalIgnoreCase)
                || string.Equals(segment, CloudinaryConfig.ResourceTypeVideo, StringComparison.OrdinalIgnoreCase);
        }

        private static ResourceType ResolveResourceType(string path)
        {
            if (TryParseCloudinaryUrl(path, out _, out var resourceTypeFromUrl))
            {
                return resourceTypeFromUrl;
            }

            var extension = Path.GetExtension(path);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return ResourceType.Raw;
            }

            if (FileStorage.AllowedImageExtensions.Contains(extension))
            {
                return ResourceType.Image;
            }

            if (FileStorage.AllowedVideoExtensions.Contains(extension))
            {
                return ResourceType.Video;
            }

            return ResourceType.Raw;
        }

        private static string GetResourceTypeSegment(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Image => CloudinaryConfig.ResourceTypeImage,
                ResourceType.Video => CloudinaryConfig.ResourceTypeVideo,
                _ => CloudinaryConfig.ResourceTypeRaw
            };
        }

        private static string GetPublicId(string path)
        {
            if (TryParseCloudinaryUrl(path, out var publicIdFromUrl, out _))
            {
                return publicIdFromUrl;
            }

            var normalizedPath = NormalizePath(path).TrimEnd('/');
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return string.Empty;
            }

            var extension = Path.GetExtension(normalizedPath);
            return string.IsNullOrWhiteSpace(extension)
                ? normalizedPath
                : normalizedPath[..^extension.Length];
        }

        private static string GetAssetFolder(string path)
        {
            var normalized = NormalizePath(path).TrimEnd('/');
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            var lastSlashIndex = normalized.LastIndexOf('/');
            if (lastSlashIndex <= 0)
            {
                return string.Empty;
            }

            return normalized[..lastSlashIndex];
        }

        private static string NormalizePath(string path)
        {
            return path
                .Replace('\\', '/')
                .TrimStart('/');
        }

        private static bool TryParseCloudinaryUrl(string path, out string publicId, out ResourceType resourceType)
        {
            publicId = string.Empty;
            resourceType = ResourceType.Raw;

            if (!Uri.TryCreate(path, UriKind.Absolute, out var uri))
            {
                return false;
            }

            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (segments.Count < 4)
            {
                return false;
            }

            var uploadIndex = segments.FindIndex(s => string.Equals(s, CloudinaryConfig.DeliveryTypeUpload, StringComparison.OrdinalIgnoreCase));
            if (uploadIndex <= 0 || uploadIndex >= segments.Count - 1)
            {
                return false;
            }

            var resourceSegment = segments[uploadIndex - 1];
            if (string.Equals(resourceSegment, CloudinaryConfig.ResourceTypeImage, StringComparison.OrdinalIgnoreCase))
            {
                resourceType = ResourceType.Image;
            }
            else if (string.Equals(resourceSegment, CloudinaryConfig.ResourceTypeVideo, StringComparison.OrdinalIgnoreCase))
            {
                resourceType = ResourceType.Video;
            }
            else if (string.Equals(resourceSegment, CloudinaryConfig.ResourceTypeRaw, StringComparison.OrdinalIgnoreCase))
            {
                resourceType = ResourceType.Raw;
            }
            else
            {
                return false;
            }

            publicId = string.Join('/', segments.Skip(uploadIndex + 1));
            return !string.IsNullOrWhiteSpace(publicId);
        }
    }
}