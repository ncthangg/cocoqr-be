using Microsoft.Extensions.DependencyInjection;
using MyWallet.QR_Generator.Builders;
using MyWallet.QR_Generator.Builders.DeepLink;
using MyWallet.QR_Generator.Engine;
using MyWallet.QR_Generator.Image;
using MyWallet.QR_Generator.Interface;

namespace MyWallet.QR_Generator.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddQrGenerator(this IServiceCollection services)
        {
            services.AddService();
        }
        private static void AddService(this IServiceCollection services)
        {
            // ── Builders — mỗi builder handle 1 provider/mode combo ──────────────
            services.AddSingleton<IQrPayloadBuilder, NapasQrBuilder>();
            services.AddSingleton<IQrPayloadBuilder, MomoNativeBuilder>();
            services.AddSingleton<IQrPayloadBuilder, VnPayQrBuilder>();

            // ── Core engine ───────────────────────────────────────────────────────
            // Singleton vì engine không có state — chỉ đọc builders
            services.AddSingleton<IQrPayloadEngine, QrPayloadEngine>();

            // ── Image renderer ────────────────────────────────────────────────────
            // Scoped vì QRCoder dùng IDisposable objects bên trong
            services.AddScoped<IQrImageRenderer, QrImageRenderer>();

            // Phase 2: uncomment khi implement SkiaSharp customize
            // services.AddScoped<IQrCustomizeRenderer, QrCustomizeRenderer>();
        }
    }
}
