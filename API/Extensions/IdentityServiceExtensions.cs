using System;
using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class IdentityServiceExtensions
{
     public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
     {
    services.AddIdentityCore<AppUser>(opt =>
    {
      opt.Password.RequireNonAlphanumeric = false;
    })
       .AddRoles<AppRole>()
       .AddRoleManager<RoleManager<AppRole>>()
       .AddEntityFrameworkStores <DataContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(option =>
       {
         var tokenKey = config["TokenKey"] ?? throw new Exception("TokenKey not found");
         option.TokenValidationParameters = new TokenValidationParameters
         {
           ValidateIssuerSigningKey = true,
           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
           ValidateIssuer = false,
           ValidateAudience = false
         };
         option.Events = new JwtBearerEvents
         {
           OnMessageReceived = context =>
            {
              var accessToken = context.Request.Query["access_token"];
              var path = context.HttpContext.Request.Path;
              if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
              {
                context.Token = accessToken;
              }
              return Task.CompletedTask;
            }
         };
       });

    services.AddAuthorizationBuilder()
       .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
       .AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));

       return services;
     }
}
