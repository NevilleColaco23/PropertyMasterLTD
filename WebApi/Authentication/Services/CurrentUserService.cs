using Microsoft.AspNetCore.Http;
using MyWarehouse.Application.Dependencies.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal; // For generic IIdentity/IPrincipal

namespace MyWarehouse.Infrastructure.Authentication.Services;

public class CurrentUserService : ICurrentUserService
{
    private const string DefaultNonUserMoniker = "System";
    private const string UknownUserMoniker = "Anonymous";

    // Store the accessor, not the raw ID
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Constructor now just stores the accessor
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // 1. String UserId Property (Reads from context on access)
    public string? UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return DefaultNonUserMoniker; // Return "System" for non-HTTP context
            }

            // Null-coalescing chain to check standard claims: NameIdentifier (SOAP), Sub (JWT)
            string? userId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? httpContext.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            // Return the found ID, or "Anonymous" if authenticated principal is present but no ID claim found.
            // If you want to return null/default, you can remove the last part of the chain.
            return userId ?? UknownUserMoniker;
        }
    }

    // 2. Convenience property for the integer ID (If your AccessLog requires an int)
    public int? UserIdInt
    {
        get
        {
            // Try to parse the string ID into an integer
            if (int.TryParse(this.UserId, out int id))
            {
                return id;
            }
            return null;
        }
    }
}