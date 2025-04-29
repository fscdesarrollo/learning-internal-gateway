// Program.cs
using Elastic.Apm.NetCoreAll;
using Learning.InternalGateway.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var authority = jwtSettings.GetValue<string>("Authority");
var validIssuer = jwtSettings.GetValue<string>("ValidIssuer");
var validAudience = jwtSettings.GetValue<string>("ValidAudience");
var requireHttpsMetadata = jwtSettings.GetValue<bool>("RequireHttpsMetadata");

builder.Services
    .AddAuthentication("InternalScheme")
    .AddJwtBearer("InternalScheme", options =>
    {
        options.Authority = authority;
        options.RequireHttpsMetadata = requireHttpsMetadata;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = validIssuer,

            ValidateAudience = true,
            ValidAudience = validAudience,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            NameClaimType = "sub",
            RoleClaimType = "scope"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claims = context.Principal?.Claims;
                var issuer = claims?.FirstOrDefault(c => c.Type == "iss")?.Value;

                if (issuer != validIssuer)
                {
                    context.Fail("Invalid Issuer.");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddOcelot();

var elasticApmServerUrl = builder.Configuration.GetValue<string>("ElasticApm:ServerUrl");
var elasticServiceName = builder.Configuration.GetValue<string>("ElasticApm:ServiceName") ?? "InternalGateway";

builder.Services.ConfigureLogger(builder.Environment,
    builder.Configuration,
    elasticServiceName);

builder.Host.UseSerilog();

var app = builder.Build();

app.UseAllElasticApm(builder.Configuration);
app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();
