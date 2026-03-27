using Microsoft.Extensions.DependencyInjection;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Services;

namespace CocoQR.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddService();
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

            services.AddScoped<IUserRoleService, UserRoleService>();
        }
    }
}
