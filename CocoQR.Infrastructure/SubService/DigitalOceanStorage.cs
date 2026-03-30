using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using CocoQR.Application.Contracts.ISubServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocoQR.Infrastructure.SubService
{
    public class DigitalOceanStorage : ICloudStorage
    {
        private readonly DigitalOceanSettings _settings;
        private readonly ILogger<DigitalOceanStorage> _logger;
        private readonly bool _isValid;

        public DigitalOceanStorage(IOptions<DigitalOceanSettings> options, ILogger<DigitalOceanStorage> logger)
        {
            _settings = options.Value;
            _logger = logger;
            _isValid = ValidateSettings();
        }

        public async Task UploadAsync(Stream stream, string path)
        {
            EnsureConfigured();

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var key = NormalizePath(path);
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Path is required", nameof(path));

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = _settings.Bucket,
                CannedACL = S3CannedACL.PublicRead
            };

            var transferUtility = new TransferUtility(GetClient());
            await transferUtility.UploadAsync(uploadRequest);
        }

        public async Task DeleteAsync(string path)
        {
            EnsureConfigured();

            var key = NormalizePath(path);
            if (string.IsNullOrWhiteSpace(key))
                return;

            var request = new DeleteObjectRequest
            {
                BucketName = _settings.Bucket,
                Key = key
            };

            await GetClient().DeleteObjectAsync(request);
        }

        public string GetPublicUrl(string path)
        {
            var key = NormalizePath(path);
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            var endpoint = GetNormalizedEndpoint();
            return $"{endpoint}/{key}";
        }

        private IAmazonS3 GetClient()
        {
            var configS3 = new AmazonS3Config
            {
                ServiceURL = GetS3ServiceEndpoint(),
                ForcePathStyle = true
            };

            return new AmazonS3Client(_settings.AccessKey, _settings.SecretKey, configS3);
        }

        private string GetS3ServiceEndpoint()
        {
            var normalizedEndpoint = GetNormalizedEndpoint();
            if (!Uri.TryCreate(normalizedEndpoint, UriKind.Absolute, out var endpointUri))
            {
                return normalizedEndpoint;
            }

            var bucket = (_settings.Bucket ?? string.Empty).Trim().Trim('/');
            if (string.IsNullOrWhiteSpace(bucket))
            {
                return normalizedEndpoint;
            }

            var host = endpointUri.Host;
            var bucketPrefix = $"{bucket}.";

            if (host.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
            {
                host = host[bucketPrefix.Length..]
                    .Replace(".cdn.digitaloceanspaces.com", ".digitaloceanspaces.com", StringComparison.OrdinalIgnoreCase);

                var builder = new UriBuilder(endpointUri)
                {
                    Host = host,
                    Path = string.Empty,
                    Query = string.Empty,
                    Fragment = string.Empty
                };

                return builder.Uri.ToString().TrimEnd('/');
            }

            return normalizedEndpoint;
        }

        private string GetNormalizedEndpoint()
        {
            var rawEndpoint = (_settings.Endpoint ?? string.Empty).Trim().TrimEnd('/');
            if (!Uri.TryCreate(rawEndpoint, UriKind.Absolute, out var endpointUri))
            {
                return rawEndpoint;
            }

            var bucket = (_settings.Bucket ?? string.Empty).Trim().Trim('/');
            var cleanedPath = endpointUri.AbsolutePath.Trim('/');

            if (!string.IsNullOrWhiteSpace(bucket))
            {
                if (cleanedPath.Equals(bucket, StringComparison.OrdinalIgnoreCase))
                {
                    cleanedPath = string.Empty;
                }
                else if (cleanedPath.StartsWith($"{bucket}/", StringComparison.OrdinalIgnoreCase))
                {
                    cleanedPath = cleanedPath[(bucket.Length + 1)..];
                }
            }

            var builder = new UriBuilder(endpointUri)
            {
                Path = string.IsNullOrWhiteSpace(cleanedPath) ? string.Empty : cleanedPath,
                Query = string.Empty,
                Fragment = string.Empty
            };

            return builder.Uri.ToString().TrimEnd('/');
        }

        private void EnsureConfigured()
        {
            if (!_isValid)
            {
                throw new InvalidOperationException("DigitalOcean storage is not configured correctly.");
            }
        }

        private bool ValidateSettings()
        {
            var isValid = true;

            if (string.IsNullOrWhiteSpace(_settings.AccessKey))
            {
                _logger.LogError("DigitalOcean AccessKey missing");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                _logger.LogError("DigitalOcean SecretKey missing");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(_settings.Bucket))
            {
                _logger.LogError("DigitalOcean Bucket missing");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(_settings.Endpoint))
            {
                _logger.LogError("DigitalOcean Endpoint missing");
                isValid = false;
            }

            if (!isValid)
            {
                _logger.LogWarning("DigitalOcean storage disabled due to invalid config");
            }

            return isValid;
        }

        private static string NormalizePath(string path)
        {
            return path
                .Replace('\\', '/')
                .TrimStart('/');
        }
    }
}