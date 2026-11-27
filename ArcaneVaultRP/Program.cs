//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using Microsoft.AspNetCore.HttpOverrides;

namespace ArcaneVaultRP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddHttpClient(); // default client, used by RecaptchaService for Google's endpoint
            builder.Services.AddScoped<ArcaneVaultRP.Services.IRecaptchaService, ArcaneVaultRP.Services.RecaptchaService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<JwtAuthHandler>();

            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");

            builder.Services.AddHttpClient("ArcaneVaultAPI",
                client => { client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/"); }).AddHttpMessageHandler<JwtAuthHandler>();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

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

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSession();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
                .WithStaticAssets();

            app.Run();
        }
    }
}
