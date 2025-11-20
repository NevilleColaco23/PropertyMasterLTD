using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Infrastructure.Authentication.Services;
using System.Diagnostics.CodeAnalysis;

namespace MyWarehouse.Infrastructure.Authentication;

[ExcludeFromCodeCoverage]
internal static class AuthenticationStartup
{
    public static void AddMyApiAuthDeps(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>(); //TODO: Enable when auth is implemented
    }
}
