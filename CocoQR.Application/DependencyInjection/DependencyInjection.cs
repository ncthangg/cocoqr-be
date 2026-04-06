using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CocoQR.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddService();
            return services;
        }
        private static void AddService(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAccountService, AccountService>();

            services.AddScoped<IQrService, QrService>();
            services.AddScoped<IQrStyleLibraryService, QrStyleLibraryService>();

            services.AddScoped<IBankInfoService, BankInfoService>();
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IEmailLogService, EmailLogService>();

            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<ISmtpSettingService, SmtpSettingService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        }
    }
}
