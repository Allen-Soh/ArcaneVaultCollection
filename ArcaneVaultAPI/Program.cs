
using ArcaneVaultAPI.Data;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVaultAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("ArcaneDbContext") ?? throw new InvalidOperationException("Connection string 'ArcaneDbContext' not found.");

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton(new Cloudinary(new Account(
                "nyj7a9za",
                "***REMOVED-CLOUDINARY-APIKEY***",
                "<***REMOVED-CLOUDINARY-APISECRET***>"
            )));

            //Poll
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("https://localhost:7051")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                    // .AllowCredentials() // only if you're sending cookies/auth
                });
            });
            //Poll

            builder.Services.AddDbContext<ArcaneDbContext>(options =>
            {
                options.UseSqlite("Data Source=ArcaneVault.db");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");

            app.UseAuthorization();

            app.MapControllers();

            // Ensure database and tables are created first
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ArcaneDbContext>();
                context.Database.EnsureCreated();  // ← creates all tables first
            }

            // Then run seeder
            ArcaneDbSeeder.Seed(app.Services);

            app.Run();
        }
    }
}
