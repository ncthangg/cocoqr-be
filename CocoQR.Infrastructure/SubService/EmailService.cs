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

            if (string.IsNullOrWhiteSpace(_options.SystemCode))
                throw new InvalidOperationException("CocoMail:SystemCode is required.");

            if (string.IsNullOrWhiteSpace(_options.KeyId))
                throw new InvalidOperationException("CocoMail:KeyId is required.");

            if (string.IsNullOrWhiteSpace(_options.PrivateKeyPem))
                throw new InvalidOperationException("CocoMail:PrivateKeyPem is required.");
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
            var signature = SignRsaSha256(_options.PrivateKeyPem!, signedPayload);

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

            var response = await _httpClient.PostAsJsonAsync("/api/mail/send", request, JsonOptions, cancellationToken);
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
    }
}
