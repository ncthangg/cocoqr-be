using Microsoft.AspNetCore.Http;

namespace MyWallet.Application.Common.Extensions
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Get base URL from request headers (with Nginx forwarded headers support)
        /// Returns: https://staging.be.cocome.online (no trailing slash)
        /// </summary>
        public static string GetBaseUrl(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // ✅ Get scheme (will be https from X-Forwarded-Proto if available)
            var scheme = request.Scheme;

            // ✅ Get host (staging.be.cocome.online)
            var host = request.Host.Host;

            // ✅ Build port string (only if non-standard)
            var portString = GetPortString(scheme, request.Host.Port);

            var baseUrl = $"{scheme}://{host}{portString}";

            Console.WriteLine($"[GetBaseUrl] Scheme: {scheme}, Host: {host}, Port: {request.Host.Port}, Result: {baseUrl}");

            return baseUrl;
        }

        /// <summary>
        /// Get callback URL with origin parameter
        /// </summary>
        public static string GetCallbackUrl(
            this HttpRequest request,
            string origin,
            string callbackPath = "/api/auths/google-auth/callback")
        {
            if (string.IsNullOrWhiteSpace(origin))
                throw new ArgumentException("Origin cannot be empty", nameof(origin));

            var baseUrl = GetBaseUrl(request);
            var encodedOrigin = Uri.EscapeDataString(origin);
            var result = $"{baseUrl}{callbackPath}?origin={encodedOrigin}";

            Console.WriteLine($"[GetCallbackUrl] BaseUrl: {baseUrl}, CallbackPath: {callbackPath}, Result: {result}");

            return result;
        }

        /// <summary>
        /// Helper: Build port string (only if non-standard)
        /// ✅ FIX: Properly handles null port
        /// </summary>
        private static string GetPortString(string scheme, int? port)
        {
            // ✅ Check if port is explicitly set (not null)
            if (!port.HasValue)
            {
                // Port is null (standard port being used)
                return "";
            }

            // ✅ Port has value, check if it's standard
            var portValue = port.Value;
            var isStandardPort = (scheme == "https" && portValue == 443) ||
                                 (scheme == "http" && portValue == 80);

            return isStandardPort ? "" : $":{portValue}";
        }
    }
}
