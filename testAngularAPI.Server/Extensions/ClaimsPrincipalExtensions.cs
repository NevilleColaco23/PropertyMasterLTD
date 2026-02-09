using System.Security.Claims;

namespace testAngularAPI.Server.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the user ID from the ClaimsPrincipal.
        /// Looks for common claim types: NameIdentifier, Sub, or UserId
        /// </summary>
        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                return null;

            // Try standard claim types
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value
                ?? principal.FindFirst("userId")?.Value
                ?? principal.FindFirst("id")?.Value;

            return userId;
        }

        /// <summary>
        /// Gets the username from the ClaimsPrincipal
        /// </summary>
        public static string? GetUsername(this ClaimsPrincipal principal)
        {
            if (principal == null)
                return null;

            return principal.FindFirst(ClaimTypes.Name)?.Value
                ?? principal.FindFirst("username")?.Value
                ?? principal.Identity?.Name;
        }

        /// <summary>
        /// Gets the email from the ClaimsPrincipal
        /// </summary>
        public static string? GetEmail(this ClaimsPrincipal principal)
        {
            if (principal == null)
                return null;

            return principal.FindFirst(ClaimTypes.Email)?.Value
                ?? principal.FindFirst("email")?.Value;
        }
    }
}
