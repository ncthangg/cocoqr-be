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
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _httpClient;
        private readonly MailGatewaySettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            HttpClient httpClient,
            IOptions<MailGatewaySettings> settings,
            ILogger<EmailService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
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

            var email = new MailGatewayEmailRequest
            {
                CorrelationId = Guid.NewGuid().ToString("N"),
                To = to.Trim(),
                Subject = subject.Trim(),
                HtmlBody = body,
                Priority = _settings.DefaultPriority,
                Attachments = []
            };

            var emailJson = JsonSerializer.Serialize(email, JsonOptions);
            var timestamp = DateTimeOffset.UtcNow.ToString("O");
            var nonce = Guid.NewGuid().ToString("N");
            var signedPayload = $"{timestamp}.{nonce}.{emailJson}";
            var signature = SignRsaSha256(_settings.PrivateKeyPem!, signedPayload);

            var request = new MailGatewaySendRequest
            {
                Authentication = new MailGatewayAuthentication
                {
                    SystemCode = _settings.SystemCode!,
                    KeyId = _settings.KeyId!,
                    Timestamp = timestamp,
                    Nonce = nonce,
                    Signature = signature
                },
                Email = email
            };

            var response = await _httpClient.PostAsJsonAsync("/api/mail/send", request, JsonOptions, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Mail gateway returned {StatusCode}. To={To}, CorrelationId={CorrelationId}, Body={Body}",
                    response.StatusCode,
                    email.To,
                    email.CorrelationId,
                    responseBody);

                response.EnsureSuccessStatusCode();
            }

            return JsonSerializer.Deserialize<MailGatewaySendResponse>(responseBody, JsonOptions);
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
                throw new InvalidOperationException("MailGateway:BaseUrl is required.");

            if (string.IsNullOrWhiteSpace(_settings.SystemCode))
                throw new InvalidOperationException("MailGateway:SystemCode is required.");

            if (string.IsNullOrWhiteSpace(_settings.KeyId))
                throw new InvalidOperationException("MailGateway:KeyId is required.");

            if (string.IsNullOrWhiteSpace(_settings.PrivateKeyPem))
                throw new InvalidOperationException("MailGateway:PrivateKeyPem is required.");
        }

        private static string SignRsaSha256(string privateKeyPem, string payload)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var signatureBytes = rsa.SignData(
                payloadBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        private sealed class MailGatewaySendRequest
        {
            public MailGatewayAuthentication Authentication { get; set; } = new();
            public MailGatewayEmailRequest Email { get; set; } = new();
        }

        private sealed class MailGatewayAuthentication
        {
            public string SystemCode { get; set; } = string.Empty;
            public string KeyId { get; set; } = string.Empty;
            public string Timestamp { get; set; } = string.Empty;
            public string Nonce { get; set; } = string.Empty;
            public string Signature { get; set; } = string.Empty;
        }

        private sealed class MailGatewayEmailRequest
        {
            public string CorrelationId { get; set; } = string.Empty;
            public string To { get; set; } = string.Empty;
            public string Cc { get; set; } = string.Empty;
            public string Bcc { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public string HtmlBody { get; set; } = string.Empty;
            public int Priority { get; set; }
            public object[] Attachments { get; set; } = [];
        }
    }
}
