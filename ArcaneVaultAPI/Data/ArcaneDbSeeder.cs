using System;
using System.Linq;

namespace ArcaneVaultAPI.Data
{
    public class ArcaneDbSeeder
    {
        /// <summary>
        /// Seeds the database with temporary/sample data.
        /// </summary>
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
                var adminRole = context.ArcaneVaultUserRoles.First(r => r.RoleName == "Admin");
                var userRole = context.ArcaneVaultUserRoles.First(r => r.RoleName == "User");

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
                        UserName = "collector_john",
                        Email = "john@arcanevault.com",
                        IsDeleted = false,
                        RoleId = 2,
                        Password = "JohnPassword"
                    },
                    new ArcaneVaultUser
                    {
                        UserName = "collector_jane",
                        Email = "jane@arcanevault.com",
                        IsDeleted = false,
                        RoleId = 2,
                        Password = "JanePassword"
                    }
                );
                context.SaveChanges();
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { CategoryCode = "CARD", CategoryName = "Trading Cards" },
                    new Category { CategoryCode = "COIN", CategoryName = "Coins" },
                    new Category { CategoryCode = "COMIC", CategoryName = "Comic Books" },
                    new Category { CategoryCode = "FIGURE", CategoryName = "Action Figures" },
                    new Category { CategoryCode = "POSTER", CategoryName = "Posters" }
                );
                context.SaveChanges();
            }

            // Seed Collection Items
            if (!context.CollectionItems.Any())
            {
                var collectorJohn = context.ArcaneVaultUsers.First(u => u.UserName == "collector_john");
                var collectorJane = context.ArcaneVaultUsers.First(u => u.UserName == "collector_jane");

                context.CollectionItems.AddRange(
                    new CollectionItem
                    {
                        ItemId = 1,
                        ItemName = "Charizard Holographic Card",
                        IsDeleted = false,
                        StartingQuantity = 1,
                        CurrentQuantity = 1,
                        User = collectorJohn
                    },
                    new CollectionItem
                    {
                        ItemId = 2,
                        ItemName = "Ancient Roman Silver Coin",
                        IsDeleted = false,
                        StartingQuantity = 3,
                        CurrentQuantity = 3,
                        User = collectorJohn
                    },
                    new CollectionItem
                    {
                        ItemId = 3,
                        ItemName = "Detective Comics #27",
                        IsDeleted = false,
                        StartingQuantity = 1,
                        CurrentQuantity = 1,
                        User = collectorJane
                    },
                    new CollectionItem
                    {
                        ItemId = 4,
                        ItemName = "Vintage Spider-Man Figure",
                        IsDeleted = false,
                        StartingQuantity = 2,
                        CurrentQuantity = 2,
                        User = collectorJane
                    }
                );
                context.SaveChanges();
            }

            // Seed Collection Item Categories (join table)
            if (!context.CollectionItemCategories.Any())
            {
                var charizardCard = context.CollectionItems.First(i => i.ItemName == "Charizard Holographic Card");
                var romanCoin = context.CollectionItems.First(i => i.ItemName == "Ancient Roman Silver Coin");
                var detectiveComic = context.CollectionItems.First(i => i.ItemName == "Detective Comics #27");
                var spiderManFigure = context.CollectionItems.First(i => i.ItemName == "Vintage Spider-Man Figure");

                var cardCategory = context.Categories.First(c => c.CategoryCode == "CARD");
                var coinCategory = context.Categories.First(c => c.CategoryCode == "COIN");
                var comicCategory = context.Categories.First(c => c.CategoryCode == "COMIC");
                var figureCategory = context.Categories.First(c => c.CategoryCode == "FIGURE");

                context.CollectionItemCategories.AddRange(
                    new CollectionItemCategory { ItemId = charizardCard.ItemId, CategoryCode = cardCategory.CategoryCode },
                    new CollectionItemCategory { ItemId = romanCoin.ItemId, CategoryCode = coinCategory.CategoryCode },
                    new CollectionItemCategory { ItemId = detectiveComic.ItemId, CategoryCode = comicCategory.CategoryCode },
                    new CollectionItemCategory { ItemId = spiderManFigure.ItemId, CategoryCode = figureCategory.CategoryCode }
                );
                context.SaveChanges();
            }
        }
    }
}