using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Infrastructure.ErrorHandling;
using System.Diagnostics.CodeAnalysis;

namespace testAngularAPI.Server.ErrorHandling;

[ExcludeFromCodeCoverage]
internal static class ErrorHandlingStartup
{
    public static void AddMyErrorHandling(this IServiceCollection services)
    {
        services.Configure<MvcOptions>(o =>
        {
            if (o == null)
            {
                throw new ArgumentException($"Cannot find {nameof(MvcOptions)}. This module depends on MVC being already added, via e.g. AddControllers().");
            }

            o.Filters.Add<ExceptionMappingFilter>();
        });
    }
}
