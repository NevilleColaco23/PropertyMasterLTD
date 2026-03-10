using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MyWarehouse.Infrastructure.Identity.Model;
using MyWarehouse.Infrastructure.Models;
using MyWarehouse.Infrastructure.Persistence.Context;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;

namespace MyWarehouse.Infrastructure.Identity;

[ExcludeFromCodeCoverage]
internal static class Startup
{
    //TODO : fix all hardcoded and remove function and use from common
    public static void ConfigureServices(IServiceCollection services, IConfiguration _)
    {
        //ConfigureServicesSQL(services, _);
        ConfigureServicesMongoDb(services, _);
    }

    private static void ConfigureServicesMongoDb(IServiceCollection services, IConfiguration _)
    {
        //var c = _.GetSection("AppSettings")?["MongoDb"];
        var settings = MongoClientSettings.FromConnectionString(_.GetConnectionString(GetValueString("MongoDb",_)));

        services.AddIdentity<ApplicationUserIdentity, ApplicationRoleIdentity>(options =>
        {
            // Require confirmed email before allowing sign-in
            options.SignIn.RequireConfirmedEmail = true;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Password requirements (can be customized as needed)
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddMongoDbStores<ApplicationUserIdentity, ApplicationRoleIdentity, int>
        (
            _.GetConnectionString(GetValueString("MongoDb", _)), "ListingDB"
        ).AddDefaultTokenProviders();
    }

    public static string GetValueString(string keyName, IConfiguration _config, string sectionName = "AppSettings")
    {
        return sectionName == null ? _config[keyName] : _config.GetSection(sectionName)?[keyName];
    }

    /// <summary>
    /// Use this if using sql for identity
    /// </summary>
    /// <param name="services"></param>s
    /// <param name="_"></param>
    private static void ConfigureServicesSQL(IServiceCollection services, IConfiguration _)
    {
        services.AddIdentity<ApplicationUserIdentity, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub; // JWT specific
        })
            .AddDefaultTokenProviders()

            // Adding Roles is optional, and mostly exists for backwards-compatibility.
            // Not needed if policy/claim based authorization is used (which is recommended).
            // But, if AddRoles() is called, it must be before calling AddEntityFrameworkStores(), because otherwise IRoleStore won't be added (despite what the summary says)..
            //.AddRoles<IdentityRole>()

            .AddEntityFrameworkStores<ApplicationDbContext>(); // EF specific
    }
}
