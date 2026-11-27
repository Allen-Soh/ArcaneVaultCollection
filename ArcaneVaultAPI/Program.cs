//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultAPI.Data;
using ArcaneVaultAPI.Services;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Text;

namespace ArcaneVaultAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // Cloudinary
            builder.Services.AddSingleton(new Cloudinary(new CloudinaryDotNet.Account(
                builder.Configuration["Cloudinary:CloudName"],
                builder.Configuration["Cloudinary:ApiKey"],
                builder.Configuration["Cloudinary:ApiSecret"]
            )));

            //Stripe API
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            // CORS — allow RP to communicate with API
            var rpBaseUrl = builder.Configuration["RpSettings:BaseUrl"]
                ?? throw new InvalidOperationException("RpSettings:BaseUrl is not configured.");

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(rpBaseUrl)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // DbContext
            builder.Services.AddDbContext<ArcaneDbContext>(options =>
            {
                options.UseSqlite("Data Source=ArcaneVault.db");
            });

            // ════════════════════════════════════════════
            // JWT AUTHENTICATION SERVICE REGISTRATION
            // ════════════════════════════════════════════
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

            builder.Services.AddScoped<IEmailService, EmailService>();

            var app = builder.Build();

            // Behind a hosting proxy (e.g. Render) TLS ends at the proxy, so trust its
            // X-Forwarded-Proto/For headers — otherwise the app thinks every request is plain HTTP.
            var forwardedOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedOptions.KnownIPNetworks.Clear();
            forwardedOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedOptions);

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // ════════════════════════════════════════════
            // MIDDLEWARE ORDER — MUST BE IN THIS EXACT ORDER
            // ════════════════════════════════════════════
            app.UseCors("AllowFrontend");        // before Authentication
            app.UseAuthentication();             // identify who is sending requests
            app.UseAuthorization();

            app.MapControllers();

            // Ensure DB + run seeder
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ArcaneDbContext>();
                context.Database.EnsureCreated();
            }
            ArcaneDbSeeder.Seed(app.Services);

            app.Run();
        }
    }
}