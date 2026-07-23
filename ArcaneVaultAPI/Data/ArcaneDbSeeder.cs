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
                        Password = "Admin123",
                        Country = "Singapore"
                    },
                    new ArcaneVaultUser
                    {
                        UserName = "Allen",
                        Email = "allen@example.com",
                        IsDeleted = false,
                        RoleId = 2,
                        Password = "Password123",
                        Country = "Malaysia"
                    },
                    new ArcaneVaultUser
                    {
                        UserName = "Allen2",
                        Email = "enderfunneh@gmail.com",
                        IsDeleted = false,
                        RoleId = 2,
                        Password = "Password123",
                        Country = "Singapore"
                    },
                    new ArcaneVaultUser
                    {
                        UserName = "Allen3",
                        Email = "altiasvas@gmail.com",
                        IsDeleted = false,
                        RoleId = 2,
                        Password = "Password123",
                        Country = "Malaysia"
                    }
                );
                context.SaveChanges();
            }

            //Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { CategoryCode = "TCS-001", CategoryName = "Trading Cards" },
                    new Category { CategoryCode = "AFS-001", CategoryName = "Action Figures" },
                    new Category { CategoryCode = "CBS-001", CategoryName = "Comic Books" },
                    new Category { CategoryCode = "CIS-001", CategoryName = "Coins" },
                    new Category { CategoryCode = "PSS-001", CategoryName = "Posters" },
                    new Category { CategoryCode = "BGS-001", CategoryName = "Board Games" },
                    new Category { CategoryCode = "VRS-001", CategoryName = "Vinyl Records" }
                );
                context.SaveChanges();
            }

            // Seed Collection Items
            if (!context.CollectionItems.Any())
            {
                context.CollectionItems.AddRange(
                    new CollectionItem
                    {
                        ItemId = 1,
                        ItemName = "Charizard Holographic Card",
                        IsDeleted = false,
                        StartingQuantity = 1,
                        CurrentQuantity = 1,
                        UserName = "Allen"
                    },
                    new CollectionItem
                    {
                        ItemId = 2,
                        ItemName = "Ancient Roman Silver Coin",
                        IsDeleted = false,
                        StartingQuantity = 3,
                        CurrentQuantity = 3,
                        UserName = "Allen"
                    },
                    new CollectionItem
                    {
                        ItemId = 3,
                        ItemName = "Detective Comics #27",
                        IsDeleted = false,
                        StartingQuantity = 1,
                        CurrentQuantity = 1,
                        UserName = "Allen2"
                    },
                    new CollectionItem
                    {
                        ItemId = 4,
                        ItemName = "Vintage Spider-Man Figure",
                        IsDeleted = false,
                        StartingQuantity = 2,
                        CurrentQuantity = 2,
                        UserName = "Allen2"
                    },
                    new CollectionItem
                    {
                        ItemId = 5,
                        ItemName = "Abbey Road Vinyl",
                        IsDeleted = false,
                        StartingQuantity = 1,
                        CurrentQuantity = 1,
                        UserName = "Allen3"
                    },
                    new CollectionItem
                    {
                        ItemId = 6,
                        ItemName = "Pikachu First Edition Card",
                        IsDeleted = false,
                        StartingQuantity = 2,
                        CurrentQuantity = 2,
                        UserName = "Allen3"
                    }
                );
                context.SaveChanges();
            }

            // Seed Collection Item Categories
            if (!context.CollectionItemCategories.Any())
            {
                context.CollectionItemCategories.AddRange(
                    new CollectionItemCategory { ItemId = 1, CategoryCode = "TCS-001" },
                    new CollectionItemCategory { ItemId = 2, CategoryCode = "CIS-001" },
                    new CollectionItemCategory { ItemId = 3, CategoryCode = "CBS-001" },
                    new CollectionItemCategory { ItemId = 4, CategoryCode = "AFS-001" },
                    new CollectionItemCategory { ItemId = 5, CategoryCode = "VRS-001" },
                    new CollectionItemCategory { ItemId = 6, CategoryCode = "TCS-001" }
                );
                context.SaveChanges();
            }

            // Seed Showoffs
            if (!context.Showoffs.Any())
            {
                context.Showoffs.AddRange(
                    new Showoff
                    {
                        ShowoffId = 1,
                        ShowoffName = "My Pokemon Collection",
                        BackgroundColor = "#fffde7",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        UserName = "Allen"
                    },
                    new Showoff
                    {
                        ShowoffId = 2,
                        ShowoffName = "Rare Comics",
                        BackgroundColor = "#e3f2fd",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        UserName = "Allen2"
                    },
                    new Showoff
                    {
                        ShowoffId = 3,
                        ShowoffName = "Vinyl Classics",
                        BackgroundColor = "#f3e5f5",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        UserName = "Allen3"
                    }
                );
                context.SaveChanges();
            }

            // Seed Showoff Items
            if (!context.ShowoffItems.Any())
            {
                context.ShowoffItems.AddRange(
                    new ShowoffItem { ShowoffId = 1, ItemId = 1, DisplayOrder = 0 },
                    new ShowoffItem { ShowoffId = 1, ItemId = 6, DisplayOrder = 1 },
                    new ShowoffItem { ShowoffId = 2, ItemId = 3, DisplayOrder = 0 },
                    new ShowoffItem { ShowoffId = 2, ItemId = 4, DisplayOrder = 1 },
                    new ShowoffItem { ShowoffId = 3, ItemId = 5, DisplayOrder = 0 }
                );
                context.SaveChanges();
            }

            // Seed Showoff Likes
            if (!context.ShowoffLikes.Any())
            {
                context.ShowoffLikes.AddRange(
                    new ShowoffLike { ShowoffId = 1, UserName = "Allen2" },
                    new ShowoffLike { ShowoffId = 1, UserName = "Allen3" },
                    new ShowoffLike { ShowoffId = 2, UserName = "Allen" },
                    new ShowoffLike { ShowoffId = 3, UserName = "Allen" }
                );
                context.SaveChanges();
            }

            // Seed Showoff Views
            if (!context.ShowoffViews.Any())
            {
                context.ShowoffViews.AddRange(
                    new ShowoffView { ShowoffId = 1, ViewerUserName = "Allen2", ViewedAt = DateTime.UtcNow.AddDays(-4) },
                    new ShowoffView { ShowoffId = 1, ViewerUserName = "Allen3", ViewedAt = DateTime.UtcNow.AddDays(-3) },
                    new ShowoffView { ShowoffId = 2, ViewerUserName = "Allen", ViewedAt = DateTime.UtcNow.AddDays(-2) },
                    new ShowoffView { ShowoffId = 3, ViewerUserName = "Allen2", ViewedAt = DateTime.UtcNow.AddDays(-1) }
                );
                context.SaveChanges();
            }
        }
    }
}