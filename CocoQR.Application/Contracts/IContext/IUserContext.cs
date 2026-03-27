namespace CocoQR.Application.Contracts.IContext
{
    public interface IUserContext
    {
        /// <summary>
        /// Gets the current authenticated user ID from JWT claims
        /// </summary>
        Guid? UserId { get; }

        /// <summary>
        /// Gets the current user's role from JWT claims
        /// </summary>
        IEnumerable<string> RoleNames { get; }

        /// <summary>
        /// Gets the client's IP address
        /// </summary>
        string IpAddress { get; }

        /// <summary>
        /// Gets the client's browser user agent
        /// </summary>
        string? Browser { get; }

        /// <summary>
        /// Gets the client's browser version
        /// </summary>
        string? BrowserVersion { get; }

        /// <summary>
        /// Gets the device type (mobile/tablet/desktop)
        /// </summary>
        string Device { get; }

        /// <summary>
        /// Gets the HTTP referer header
        /// </summary>
        string? Referer { get; }

        /// <summary>
        /// Gets or creates a unique visitor ID from cookies
        /// </summary>
        string VisitorId { get; }

        /// <summary>
        /// Ensures the visitor cookie exists
        /// </summary>
        void EnsureVisitorCookie();

        bool IsAuthenticated();
        bool IsAdmin();
        bool IsUser();
    }
}
