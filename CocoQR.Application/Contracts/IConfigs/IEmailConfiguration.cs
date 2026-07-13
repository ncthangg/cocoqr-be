namespace CocoQR.Application.Contracts.IConfigs
{
    public interface IEmailConfiguration
    {
        string? SystemCode { get; }
        string? AdminNotificationEmail { get; }
    }
}
