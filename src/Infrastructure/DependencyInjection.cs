using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Application.Common.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Database
        var connectionString = configuration.GetConnectionString("Default");
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");

        if (useInMemory)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDB")
            );
        else
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>()
        );

        // Register services
        services.AddScoped<ITokenService, JwtService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        // JWT
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtKey = configuration["Jwt:Key"]!;
                var jwtIssuer = configuration["Jwt:Issuer"]!;
                var jwtAudience = configuration["Jwt:Audience"]!;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                };
            });

        // Amazon S3
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var endpoint = config["S3:Endpoint"];
            var useSsl = config.GetValue<bool>("S3:UseSsl");

            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USWest1,
                ForcePathStyle = true,
            };

            if (!string.IsNullOrEmpty(endpoint))
            {
                s3Config.ServiceURL = endpoint;
                s3Config.UseHttp = !useSsl;
            }

            return new AmazonS3Client(new AnonymousAWSCredentials(), s3Config);
        });

        services.AddScoped<IFileStorageService, S3FileStorageService>();

        return services;
    }
}
