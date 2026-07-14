using System;
using System.Linq;

namespace ArcaneVaultAPI.Data
{
    public class ArcaneDbSeeder
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ArcaneDbContext>();

            if (context == null) throw new ArgumentNullException(nameof(context));

            // Seed User Roles
            if (!context.ArcaneVaultUserRoles.Any())
            {
                context.ArcaneVaultUserRoles.AddRange(
                    new ArcaneVaultUserRoles { RoleId = 1, RoleName = "Admin" },
                    new ArcaneVaultUserRoles { RoleId = 2, RoleName = "User" }
                );
                context.SaveChanges();
            }

            // Seed Users
            if (!context.ArcaneVaultUsers.Any())
            {
                context.ArcaneVaultUsers.AddRange(
                    new ArcaneVaultUser
                    {
                        UserName = "admin_user",
                        Email = "admin@arcanevault.com",
                        IsDeleted = false,
                        RoleId = 1,
                        Password = "Admin123"
                    },
                    new ArcaneVaultUser
                    {
                        UserName = "Allen",
                        Email = "allen@example.com",
                        IsDeleted = false,
                        RoleId = 2,
                        Password = "Password123"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}