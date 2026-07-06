using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Domain.Constants;
using CocoQR.Infrastructure.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CocoQR.Infrastructure.SubService
{
    public class EmailService : IEmailService
    {
        private readonly ICocoMailClient _cocoMailClient;
        private readonly CocoMailOptions _options;

        public EmailService(
            ICocoMailClient cocoMailClient,
            IOptions<CocoMailOptions> options)
        {
            _cocoMailClient = cocoMailClient;
            _options = options.Value;
        }

        public async Task<MailGatewaySendResponse?> SendAsync(
            string to,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException(ValidationMessages.RequiredEmail, nameof(to));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException(ValidationMessages.RequiredSubject, nameof(subject));

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException(ValidationMessages.RequiredBody, nameof(body));

            EnsureConfigured();

            var email = new CocoMailEmailRequest
            {
                CorrelationId = Guid.NewGuid().ToString("N"),
                To = to.Trim(),
                Subject = subject.Trim(),
                HtmlBody = body,
                Priority = _options.DefaultPriority,
                Attachments = []
            };

            return await _cocoMailClient.SendAsync(email, cancellationToken);
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrWhiteSpace(_options.BaseUrl))
                throw new InvalidOperationException("CocoMail:BaseUrl is required.");

            if (string.IsNullOrWhiteSpace(_options.SendEndpoint))
                throw new InvalidOperationException("CocoMail:SendEndpoint is required.");

            if (string.IsNullOrWhiteSpace(_options.TemplateEndpoint))
                throw new InvalidOperationException("CocoMail:TemplateEndpoint is required.");

            if (string.IsNullOrWhiteSpace(_options.SystemCode))
                throw new InvalidOperationException("CocoMail:SystemCode is required.");

            if (string.IsNullOrWhiteSpace(_options.KeyId))
                throw new InvalidOperationException("CocoMail:KeyId is required.");

            if (string.IsNullOrWhiteSpace(_options.PrivateKeyBase64))
                throw new InvalidOperationException("CocoMail:PrivateKeyBase64 is required.");
        }
    }

    public sealed class CocoMailClient : ICocoMailClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _httpClient;
        private readonly CocoMailOptions _options;
        private readonly ILogger<CocoMailClient> _logger;

        public CocoMailClient(
            HttpClient httpClient,
            IOptions<CocoMailOptions> options,
            ILogger<CocoMailClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<MailGatewaySendResponse?> SendAsync(
            CocoMailEmailRequest email,
            CancellationToken cancellationToken = default)
        {
            var emailJson = JsonSerializer.Serialize(email, JsonOptions);
            var timestamp = DateTimeOffset.UtcNow.ToString("O");
            var nonce = Guid.NewGuid().ToString("N");
            var signedPayload = $"{timestamp}.{nonce}.{emailJson}";
            var signature = SignRsaSha256(_options.PrivateKeyBase64!, signedPayload);

            var request = new CocoMailGatewayRequest
            {
                Authentication = new CocoMailAuthentication
                {
                    SystemCode = _options.SystemCode!,
                    KeyId = _options.KeyId!,
                    Timestamp = timestamp,
                    Nonce = nonce,
                    Signature = signature
                },
                Email = email
            };

            var response = await _httpClient.PostAsJsonAsync(
                _options.SendEndpoint,
                request,
                JsonOptions,
                cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CocoMail returned {StatusCode}. To={To}, CorrelationId={CorrelationId}, Body={Body}",
                    response.StatusCode,
                    email.To,
                    email.CorrelationId,
                    responseBody);

                response.EnsureSuccessStatusCode();
            }

            return JsonSerializer.Deserialize<MailGatewaySendResponse>(responseBody, JsonOptions);
        }

        public async Task<IReadOnlyList<CocoMailTemplateResponse>> GetTemplatesAsync(
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(
                _options.TemplateEndpoint,
                cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CocoMail template endpoint returned {StatusCode}. Body={Body}",
                    response.StatusCode,
                    responseBody);
                response.EnsureSuccessStatusCode();
            }

            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            var templatesElement = ResolveTemplatesElement(root);

            if (templatesElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<CocoMailTemplateResponse>>(
                templatesElement.GetRawText(),
                JsonOptions) ?? [];
        }

        private static JsonElement ResolveTemplatesElement(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                return root;
            }

            if (!root.TryGetProperty("data", out var data))
            {
                return default;
            }

            if (data.ValueKind == JsonValueKind.Array)
            {
                return data;
            }

            if (data.ValueKind == JsonValueKind.Object)
            {
                if (data.TryGetProperty("items", out var items))
                {
                    return items;
                }

                if (data.TryGetProperty("list", out var list))
                {
                    return list;
                }
            }

            return default;
        }

        private static string SignRsaSha256(string privateKeyBase64, string payload)
        {
            using var rsa = RSA.Create();
            ImportPrivateKey(rsa, privateKeyBase64);

            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var signatureBytes = rsa.SignData(
                payloadBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        private static void ImportPrivateKey(RSA rsa, string privateKeyBase64)
        {
            var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
            var privateKeyText = Encoding.UTF8.GetString(privateKeyBytes);

            if (privateKeyText.Contains("BEGIN", StringComparison.OrdinalIgnoreCase))
            {
                rsa.ImportFromPem(privateKeyText);
                return;
            }

            try
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            }
            catch (CryptographicException)
            {
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            }
        }
    }
}
