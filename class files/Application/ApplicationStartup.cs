using Microsoft.Extensions.DependencyInjection;
using MyWarehouse.Application.Common.Behaviors;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Services;
using System.Reflection;

namespace MyWarehouse.Application;

public static class ApplicationStartup
{
    public static void AddMyApplicationDependencies(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionLoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

        // Register AutoMapper
        services.AddAutoMapper(typeof(ApplicationStartup)); // Assuming your profiles are in the same assembly
    }
}
