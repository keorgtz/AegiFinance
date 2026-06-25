using System.Text;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Infrastructure.Authorization;
using AegiFinance.Infrastructure.Data;
using AegiFinance.Infrastructure.Data.Interceptors;
using AegiFinance.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AegiFinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPinHasher, PinHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrencyConverter, CurrencyConverter>();
        services.AddScoped<IClientCodeGenerator, ClientCodeGenerator>();
        services.AddScoped<IServiceCodeGenerator, ServiceCodeGenerator>();
        services.AddScoped<ISubscriptionCodeGenerator, SubscriptionCodeGenerator>();
        services.AddScoped<IBillingGenerationService, BillingGenerationService>();
        services.AddScoped<IAccountBalanceCalculator, AccountBalanceCalculator>();
        services.AddScoped<ILedgerService, LedgerService>();
        services.AddScoped<IAllocationService, AllocationService>();
        services.AddScoped<IAccountNumberProtector, AccountNumberProtector>();

        var dataProtectionKeysPath = configuration["DataProtection:KeysPath"];
        var dataProtectionBuilder = services.AddDataProtection().SetApplicationName("AegiFinance");
        if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
        {
            dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
        }

        services.AddHttpClient<IExchangeRateProvider, ExchangeRateApiProvider>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var secret = configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret no está configurado.");
                var issuer = configuration["JwtSettings:Issuer"];
                var audience = configuration["JwtSettings:Audience"];

                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization();

        return services;
    }
}
