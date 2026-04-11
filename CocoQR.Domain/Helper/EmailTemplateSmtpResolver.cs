using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Domain.Helper
{
    public static class EmailTemplateSmtpResolver
    {
        public static SmtpSettingType Resolve(string templateKey)
        {
            if (string.IsNullOrWhiteSpace(templateKey))
            {
                return SmtpSettingType.System;
            }

            templateKey = templateKey.ToLowerInvariant();

            return templateKey switch
            {
                var k when k.StartsWith("contact") => SmtpSettingType.Contact,
                var k when k.StartsWith("admin") => SmtpSettingType.Admin,
                var k when k.StartsWith("support") => SmtpSettingType.Support,
                _ => SmtpSettingType.System
            };
        }
    }
}
