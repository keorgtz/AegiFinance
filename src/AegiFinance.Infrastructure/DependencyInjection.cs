using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        services.AddHttpContextAccessor();
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<TenantSessionContextInterceptor>();
        services.AddScoped<MajorLedgerInvariantInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            // Financial write flows use explicit transactions to keep the operational
            // record and its Major Ledger journal atomic. SQL Server's retrying
            // execution strategy cannot be combined with those user transactions.
            options.UseSqlServer(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<TenantSessionContextInterceptor>(),
                sp.GetRequiredService<MajorLedgerInvariantInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPinHasher, PinHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IUiPermissionService, UiPermissionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrencyConverter, CurrencyConverter>();
        services.AddScoped<IClientCodeGenerator, ClientCodeGenerator>();
        services.AddScoped<IServiceCodeGenerator, ServiceCodeGenerator>();
        services.AddScoped<ISubscriptionCodeGenerator, SubscriptionCodeGenerator>();
        services.AddScoped<IBillingGenerationService, BillingGenerationService>();
        services.AddScoped<IAccountBalanceCalculator, AccountBalanceCalculator>();
        services.AddScoped<IClientDocumentService, ClientDocumentService>();
        services.AddScoped<IClientGovernanceService, ClientGovernanceService>();
        services.AddScoped<ILedgerService, LedgerService>();
        services.AddScoped<IMajorLedgerService, MajorLedgerService>();
        services.AddScoped<IAllocationService, AllocationService>();
        services.AddSingleton<IAccountStatementExporter, AccountStatementExporter>();
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
                if (secret.Length < 32)
                    throw new InvalidOperationException("JWT Secret debe tener al menos 32 caracteres.");
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
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                        var sessionClaim = context.Principal?.FindFirst("sessionId")?.Value;
                        if (!Guid.TryParse(userClaim, out var userId) || !Guid.TryParse(sessionClaim, out var sessionId))
                        {
                            context.Fail("Token de sesión inválido.");
                            return;
                        }

                        var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                        var active = await db.UserSessions.AsNoTracking().AnyAsync(item =>
                            item.Id == sessionId && item.UserId == userId && item.RevokedAt == null &&
                            item.ExpiresAt > DateTime.UtcNow && item.User.IsActive,
                            context.HttpContext.RequestAborted);
                        if (!active) context.Fail("La sesión fue revocada.");
                    }
                };
            });

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization();

        return services;
    }
}
