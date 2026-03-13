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

    public string UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return DefaultNonUserMoniker;
            }

            var isAuthenticated = httpContext.User.Identity.IsAuthenticated;
            Console.WriteLine($"IsAuthenticated: {isAuthenticated}");

            string? userId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? httpContext.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            // Always return a non-null value to match the interface
            return userId ?? UknownUserMoniker;
        }
    }

    public int? UserIdInt
    {
        get
        {
            if (int.TryParse(this.UserId, out int id))
            {
                return id;
            }
            return null;
        }
    }
}
