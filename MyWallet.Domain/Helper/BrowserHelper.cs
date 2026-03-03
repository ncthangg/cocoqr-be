namespace MyWallet.Domain.Helper
{
    public static class BrowserHelper
    {
        public static (string Browser, string Version) Parse(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return ("Unknown", "");

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("edg/"))
                return ("Edge", ExtractVersion(userAgent, "edg/"));

            if (userAgent.Contains("chrome/"))
                return ("Chrome", ExtractVersion(userAgent, "chrome/"));

            if (userAgent.Contains("firefox/"))
                return ("Firefox", ExtractVersion(userAgent, "firefox/"));

            if (userAgent.Contains("safari/") && !userAgent.Contains("chrome"))
                return ("Safari", ExtractVersion(userAgent, "version/"));

            return ("Other", "");
        }

        private static string ExtractVersion(string ua, string key)
        {
            var idx = ua.IndexOf(key);
            if (idx < 0) return "";

            var version = ua.Substring(idx + key.Length)
                .Split(' ', ';', ')')
                .FirstOrDefault();

            return version ?? "";
        }
    }

}
