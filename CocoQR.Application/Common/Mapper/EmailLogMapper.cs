using CocoQR.Application.DTOs.EmailLogs.Queries;
using CocoQR.Application.DTOs.EmailLogs.Responses;
using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.Common.Mapper
{
    public static class EmailLogMapper
    {
        public static GetEmailLogRes ToListResponse(EmailLogListQueryDto dto)
        {
            return new GetEmailLogRes
            {
                Id = dto.Id,
                RecipientUserId = dto.RecipientUserId,
                RecipientFullName = dto.RecipientFullName,
                ToEmail = dto.ToEmail,
                Subject = dto.Subject,
                Status = ParseStatus(dto.Status),
                ErrorMessage = dto.ErrorMessage,
                SmtpType = ParseSmtpType(dto.SmtpType),
                EmailDirection = ParseDirection(dto.EmailDirection),
                TemplateKey = dto.TemplateKey,
                CreatedAt = dto.CreatedAt
            };
        }

        public static GetEmailLogByIdRes ToDetailResponse(EmailLogQueryDto dto)
        {
            return new GetEmailLogByIdRes
            {
                Id = dto.Id,
                RecipientUserId = dto.RecipientUserId,
                RecipientFullName = dto.RecipientFullName,
                ToEmail = dto.ToEmail,
                Subject = dto.Subject,
                Body = dto.Body,
                Status = ParseStatus(dto.Status),
                ErrorMessage = dto.ErrorMessage,
                SmtpType = ParseSmtpType(dto.SmtpType),
                EmailDirection = ParseDirection(dto.EmailDirection),
                TemplateKey = dto.TemplateKey,
                CreatedAt = dto.CreatedAt
            };
        }

        private static EmailLogStatus ParseStatus(string status)
        {
            return Enum.TryParse<EmailLogStatus>(status, true, out var parsed)
                ? parsed
                : EmailLogStatus.FAIL;
        }

        private static SmtpSettingType ParseSmtpType(string smtpType)
        {
            return Enum.TryParse<SmtpSettingType>(smtpType, true, out var parsed)
                ? parsed
                : SmtpSettingType.System;
        }

        private static EmailDirection ParseDirection(string direction)
        {
            return Enum.TryParse<EmailDirection>(direction, true, out var parsed)
                ? parsed
                : EmailDirection.OUTGOING;
        }
    }
}
