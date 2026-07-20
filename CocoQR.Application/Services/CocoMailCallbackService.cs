using CocoQR.Application.Contracts.IConfigs;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.CocoMail;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using System.Text.Json;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
{
    public class CocoMailCallbackService : ICocoMailCallbackService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailConfiguration _emailConfiguration;

        public CocoMailCallbackService(
            IUnitOfWork unitOfWork,
            IEmailConfiguration emailConfiguration)
        {
            _unitOfWork = unitOfWork;
            _emailConfiguration = emailConfiguration;
        }

        public async Task HandleAsync(
            CocoMailCallbackRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            Validate(request);

            if (!string.Equals(
                    request.SystemCode,
                    _emailConfiguration.SystemCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException(
                    ErrorCode.Unauthorized,
                    ErrorMessages.Unauthorized);
            }

            var alreadyProcessed = await _unitOfWork.CocoMailCallbackEvents
                .ExistsByEventIdAsync(request.EventId.Trim());
            if (alreadyProcessed)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var payload = JsonSerializer.Serialize(request, JsonOptions);
            var eventEntity = new CocoMailCallbackEvent
            {
                Id = Guid.NewGuid(),
                EventId = request.EventId.Trim(),
                EmailId = request.EmailId,
                CorrelationId = request.CorrelationId.Trim(),
                EventType = request.EventType.Trim(),
                Status = request.Status.Trim(),
                Payload = payload,
                ReceivedAt = now
            };

            await _unitOfWork.CocoMailCallbackEvents.AddAsync(eventEntity);

            var message = await _unitOfWork.EmailConversationMessages
                .GetByGatewayMessageIdAsync(request.EmailId);

            if (message != null)
            {
                message.Status = MapStatus(request.Status, request.EventType);
                message.LastCallbackEventId = request.EventId.Trim();
                message.LastCallbackAt = request.OccurredAt == default
                    ? now
                    : request.OccurredAt;
                message.LastCallbackPayload = payload;
                message.FailureCode = string.IsNullOrWhiteSpace(request.Failure?.Code)
                    ? null
                    : request.Failure.Code.Trim();
                message.ProviderMessageId = string.IsNullOrWhiteSpace(request.ProviderMessageId)
                    ? null
                    : request.ProviderMessageId.Trim();
                message.ErrorMessage = string.IsNullOrWhiteSpace(request.Failure?.Message)
                    ? null
                    : Truncate(request.Failure.Message.Trim(), 2000);

                await _unitOfWork.EmailConversationMessages.UpdateDeliveryStatusAsync(message);
            }

            await _unitOfWork.CocoMailCallbackEvents
                .MarkProcessedAsync(request.EventId.Trim(), DateTime.UtcNow);
        }

        private static void Validate(CocoMailCallbackRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EventId))
            {
                throw new ArgumentException("EventId la bat buoc.", nameof(request.EventId));
            }

            if (request.EmailId == Guid.Empty)
            {
                throw new ArgumentException("EmailId khong hop le.", nameof(request.EmailId));
            }

            if (string.IsNullOrWhiteSpace(request.SystemCode))
            {
                throw new ArgumentException("SystemCode la bat buoc.", nameof(request.SystemCode));
            }

            if (string.IsNullOrWhiteSpace(request.Status) &&
                string.IsNullOrWhiteSpace(request.EventType))
            {
                throw new ArgumentException("Status hoac EventType la bat buoc.");
            }
        }

        private static EmailDeliveryStatus MapStatus(string status, string eventType)
        {
            var value = string.IsNullOrWhiteSpace(status) ? eventType : status;

            return value.Trim().ToUpperInvariant() switch
            {
                "SENT" => EmailDeliveryStatus.Sent,
                "FAILED" => EmailDeliveryStatus.Failed,
                "QUEUED" => EmailDeliveryStatus.Queued,
                "PENDING" => EmailDeliveryStatus.Pending,
                "SENDING" => EmailDeliveryStatus.Sending,
                "CANCELLED" => EmailDeliveryStatus.Cancelled,
                _ => EmailDeliveryStatus.Pending
            };
        }

        private static string Truncate(string value, int maxLength)
            => value.Length <= maxLength ? value : value[..maxLength];
    }
}
