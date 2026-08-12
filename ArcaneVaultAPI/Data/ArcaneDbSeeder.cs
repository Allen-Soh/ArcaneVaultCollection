using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVaultAPI.Data
{
    public class ArcaneDbSeeder
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ArcaneDbContext>();

            if (context == null) throw new ArgumentNullException(nameof(context));

            // ── Schema migration (idempotent) ──
            RunMigrations(context);


            // Seed User Roles
            if (!context.ArcaneVaultUserRoles.Any())
            {
                context.ArcaneVaultUserRoles.AddRange(
                    new ArcaneVaultUserRoles { RoleId = 1, RoleName = "Admin" },
                    new ArcaneVaultUserRoles { RoleId = 2, RoleName = "User" }
                );
                context.SaveChanges();
            }

            // Seed Users — Admin, Admin1, Allen only
            if (!context.ArcaneVaultUsers.Any())
            {
                context.ArcaneVaultUsers.AddRange(
                    new ArcaneVaultUser
                    {
                        UserName = "Admin",
                        Email = "admin@example.com",
                        IsDeleted = false,
                        RoleId = 1,
                        Password = "Admin123",
                        Country = "Singapore",
                        AvatarUrl = "https://res.cloudinary.com/nyj7a9za/image/upload/v1785909172/arcanevault/showoffs/ky1vusmphqminxjtaudh.jpg",
                        IsActive = false,
                        LastNotificationCheck = "2026-08-08 06:37:25.6235841"
                    },
                    new ArcaneVaultUser
                    {
                        UserName = "Admin1",
                        Email = "admin1@example.com",
                        IsDeleted = false,
                        RoleId = 1,
                        Password = "Admin123",
                        Country = "Singapore",
                        AvatarUrl = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786465075/arcanevault/showoffs/xo3h891vxwqcaorctr9m.jpg",
                        IsActive = true,
                        LastNotificationCheck = "2026-08-11 16:22:28.101829"
                    },
                    new ArcaneVaultUser
                    {
                        UserName = "Allen",
                        Email = "allen@example.com",
                        IsDeleted = false,
                        RoleId = 2,
                        Password = "Password123",
                        Country = "Malaysia",
                        AvatarUrl = "https://res.cloudinary.com/nyj7a9za/image/upload/v1785665644/arcanevault/showoffs/jtu5vm8ognpxjp05zc8k.jpg",
                        IsActive = true,
                        LastNotificationCheck = "2026-08-11 16:27:35.1914499"
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

            // Seed Collection Items — only items owned by a kept user (Admin/Admin1/Allen)
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
                        ItemId = 7,
                        ItemName = "Pikachu Surging Sparks SIR Card",
                        IsDeleted = false,
                        StartingQuantity = 1,
                        CurrentQuantity = 1,
                        UserName = "Allen"
                    },
                    new CollectionItem
                    {
                        ItemId = 8,
                        ItemName = "Aemeath Figurine",
                        IsDeleted = false,
                        StartingQuantity = 1,
                        CurrentQuantity = 1,
                        UserName = "Admin1",
                        ImageUrl = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786092181/arcanevault/showoffs/azjqddbkjscabhgt5nzl.jpg",
                        ImagePublicId = "arcanevault/showoffs/azjqddbkjscabhgt5nzl"
                    }
                );
                context.SaveChanges();
            }

            // Seed Collection Item Categories — cascaded to match the kept Collection Items above
            if (!context.CollectionItemCategories.Any())
            {
                context.CollectionItemCategories.AddRange(
                    new CollectionItemCategory { ItemId = 1, CategoryCode = "TCS-001" },
                    new CollectionItemCategory { ItemId = 2, CategoryCode = "CIS-001" },
                    new CollectionItemCategory { ItemId = 8, CategoryCode = "AFS-001" }
                );
                context.SaveChanges();
            }

            // Seed Showoffs — only owned by a kept user AND with a background image
            if (!context.Showoffs.Any())
            {
                context.Showoffs.AddRange(
                    new Showoff
                    {
                        ShowoffId = 4,
                        ShowoffName = "Private Collection",
                        BackgroundColor = "#e0f7fa",
                        BackgroundImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1785909325/arcanevault/showoffs/moazvhkgqtcub3fjb7go.jpg",
                        BackgroundImagePublicId = "arcanevault/showoffs/moazvhkgqtcub3fjb7go",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.Parse("2026-08-04T14:03:04.6120009", null, System.Globalization.DateTimeStyles.RoundtripKind),
                        UserName = "Allen",
                        Orientation = "Portrait"
                    },
                    new Showoff
                    {
                        ShowoffId = 19,
                        ShowoffName = "Aemeath",
                        BackgroundColor = "#1c2128",
                        BackgroundImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786464670/arcanevault/showoffs/xv0koxaufycair0zdqsw.jpg",
                        BackgroundImagePublicId = "arcanevault/showoffs/xv0koxaufycair0zdqsw",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.Parse("2026-08-07T08:44:15.5213093", null, System.Globalization.DateTimeStyles.RoundtripKind),
                        UserName = "Admin1",
                        Orientation = "Portrait",
                        EquippedFrameCode = "FRM_39C37E"
                    },
                    new Showoff
                    {
                        ShowoffId = 26,
                        ShowoffName = "Charizard Holo Base Set",
                        BackgroundColor = "#1c2128",
                        BackgroundImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786449961/arcanevault/showoffs/g8ueysqdecqsj8pkkklf.jpg",
                        BackgroundImagePublicId = "arcanevault/showoffs/g8ueysqdecqsj8pkkklf",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.Parse("2026-08-11T12:06:26.4904021", null, System.Globalization.DateTimeStyles.RoundtripKind),
                        UserName = "Allen",
                        Orientation = "Portrait"
                    }
                );
                context.SaveChanges();
            }

            // Seed Showoff Items — cascaded to match the kept Showoffs/Collection Items above
            if (!context.ShowoffItems.Any())
            {
                context.ShowoffItems.AddRange(
                    new ShowoffItem
                    {
                        ShowoffId = 26,
                        ItemId = 1,
                        DisplayOrder = 0,
                        ImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786449979/arcanevault/showoffs/oys4cb6imzm2qkgllh37.jpg",
                        ImagePublicId = "arcanevault/showoffs/oys4cb6imzm2qkgllh37"
                    },
                    new ShowoffItem
                    {
                        ShowoffId = 4,
                        ItemId = 7,
                        DisplayOrder = 0,
                        ImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786032694/arcanevault/showoffs/vysehht8eysgqbvpc1fz.jpg",
                        ImagePublicId = "arcanevault/showoffs/vysehht8eysgqbvpc1fz"
                    },
                    new ShowoffItem
                    {
                        ShowoffId = 19,
                        ItemId = 8,
                        DisplayOrder = 0,
                        ImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786464679/arcanevault/showoffs/mvqxfsiatipl5vcvk3ud.jpg",
                        ImagePublicId = "arcanevault/showoffs/mvqxfsiatipl5vcvk3ud"
                    }
                );
                context.SaveChanges();
            }

            // Seed Showoff Likes — cascaded to match the kept Showoffs/Users above
            if (!context.ShowoffLikes.Any())
            {
                context.ShowoffLikes.AddRange(
                    new ShowoffLike { ShowoffId = 19, UserName = "Admin1" },
                    new ShowoffLike { ShowoffId = 4, UserName = "Admin1" }
                );
                context.SaveChanges();
            }

            // Seed Showoff Views — cascaded to match the kept Showoffs/Users above
            if (!context.ShowoffViews.Any())
            {
                context.ShowoffViews.AddRange(
                    new ShowoffView { ViewId = 8, ShowoffId = 4, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-06T16:11:15.7913217", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 9, ShowoffId = 4, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-06T16:11:44.2226258", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 13, ShowoffId = 4, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-07T08:06:23.1470452", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 14, ShowoffId = 4, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-07T08:24:02.2058721", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 15, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-07T15:22:24.9268788", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 16, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-07T15:22:30.9383773", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 17, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-07T15:22:37.8324461", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 18, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-07T15:22:52.3331527", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 19, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-07T15:23:16.2498561", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 20, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-10T09:21:26.4015141", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 21, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-10T17:56:21.6066669", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 22, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-10T17:56:26.5940009", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 23, ShowoffId = 19, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-10T20:41:33.3557917", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 24, ShowoffId = 19, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-10T20:41:39.2399645", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 25, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-10T20:42:00.2764380", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 26, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-10T20:42:03.8317386", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 27, ShowoffId = 19, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-10T21:22:12.1064707", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 28, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-10T21:30:34.9081751", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 29, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-10T21:30:43.2519135", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 30, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-10T21:30:48.2445177", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 31, ShowoffId = 4, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-10T21:40:50.5427624", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 32, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-10T21:40:54.0263385", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 33, ShowoffId = 19, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-10T21:41:22.5247647", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 34, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T09:42:37.5921205", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 35, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T09:44:15.1695831", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 36, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T09:58:31.0834581", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 38, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T10:01:56.6549142", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 39, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T10:02:00.9987592", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 40, ShowoffId = 4, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-11T11:54:01.2842838", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 41, ShowoffId = 19, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-11T11:54:02.8059660", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 42, ShowoffId = 4, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T11:56:18.8072978", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 43, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T11:56:19.8835433", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 44, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T11:56:22.1670912", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 45, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T12:00:25.8125897", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 46, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T12:00:36.6875793", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 47, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T12:00:38.5571822", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 48, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T12:00:46.1825643", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 49, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T12:00:57.0318459", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 50, ShowoffId = 19, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-11T12:05:22.4623323", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 51, ShowoffId = 26, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-11T12:06:38.4050474", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 52, ShowoffId = 4, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-11T15:54:27.7162077", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 53, ShowoffId = 4, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-11T15:54:44.9099800", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 54, ShowoffId = 4, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-11T15:54:54.0704239", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 55, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T16:10:26.0090445", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 56, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T16:10:40.7107114", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 57, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T16:10:54.6818915", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 58, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T16:12:04.7243421", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 59, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T16:16:16.0047308", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 60, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T16:17:11.3675316", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 61, ShowoffId = 4, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T16:19:35.9233989", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 62, ShowoffId = 4, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-11T16:19:47.3022702", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 63, ShowoffId = 4, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-11T16:23:24.0788899", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 64, ShowoffId = 4, ViewerUserName = "Allen", ViewedAt = DateTime.Parse("2026-08-11T16:26:49.7354325", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 65, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-12T01:18:58.8649842", null, System.Globalization.DateTimeStyles.RoundtripKind) },
                    new ShowoffView { ViewId = 66, ShowoffId = 19, ViewerUserName = "Admin1", ViewedAt = DateTime.Parse("2026-08-12T01:20:10.5261190", null, System.Globalization.DateTimeStyles.RoundtripKind) }
                );
                context.SaveChanges();
            }

            // Seed Shop — Frames (all, no filtering per rule 5)
            if (!context.Frames.Any())
            {
                context.Frames.AddRange(
                    new Frame { FrameCode = "FRM_C192B1", FrameName = "Royal Frame", ImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786384476/arcanevault/showoffs/oirvngdsifswffabu3ej.png", ImagePublicId = "arcanevault/showoffs/oirvngdsifswffabu3ej", Orientation = "Landscape", Price = 14.0m, IsLimited = false, LimitedQuantity = null, IsDelisted = false },
                    new Frame { FrameCode = "FRM_CF6617", FrameName = "Tranquil", ImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786441577/arcanevault/showoffs/aykvqfnykqcowgej4wsb.png", ImagePublicId = "arcanevault/showoffs/aykvqfnykqcowgej4wsb", Orientation = "Landscape", Price = 5.0m, IsLimited = false, LimitedQuantity = null, IsDelisted = false },
                    new Frame { FrameCode = "FRM_E6CBF2", FrameName = "SCI-FI", ImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786463521/arcanevault/showoffs/fto3p7dlpuvm41hsv9mh.png", ImagePublicId = "arcanevault/showoffs/fto3p7dlpuvm41hsv9mh", Orientation = "Portrait", Price = 2.0m, IsLimited = false, LimitedQuantity = null, IsDelisted = false },
                    new Frame { FrameCode = "FRM_39C37E", FrameName = "Stream of lights", ImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786464803/arcanevault/showoffs/suu7rj541vzzonf56d8j.png", ImagePublicId = "arcanevault/showoffs/suu7rj541vzzonf56d8j", Orientation = "Portrait", Price = 0.0m, IsLimited = false, LimitedQuantity = null, IsDelisted = false },
                    new Frame { FrameCode = "FRM_1466B6", FrameName = "huH.", ImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786464915/arcanevault/showoffs/nmrhadcbzqvc3hegzhwa.png", ImagePublicId = "arcanevault/showoffs/nmrhadcbzqvc3hegzhwa", Orientation = "Landscape", Price = 0.0m, IsLimited = false, LimitedQuantity = null, IsDelisted = false }
                );
                context.SaveChanges();
            }

            // Seed Shop — Stickers (all, no filtering per rule 5)
            if (!context.Stickers.Any())
            {
                context.Stickers.AddRange(
                    new Sticker { StickerCode = "TREEK", StickerName = "Treekachu", ImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786115988/arcanevault/showoffs/n9dretqeki7qkcvlgujv.jpg", ImagePublicId = "arcanevault/showoffs/n9dretqeki7qkcvlgujv", Price = 10.0m, IsLimited = true, LimitedQuantity = 2, IsDelisted = false },
                    new Sticker { StickerCode = "STK_CAC9B2", StickerName = "Neon", ImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1786464567/arcanevault/showoffs/pz9gip8rkdjdqsyjgzjk.png", ImagePublicId = "arcanevault/showoffs/pz9gip8rkdjdqsyjgzjk", Price = 0.0m, IsLimited = false, LimitedQuantity = null, IsDelisted = false }
                );
                context.SaveChanges();
            }

            // Seed Payments and CosmeticInventory via raw ADO.NET, not the EF DbSets — the "Payments"/
            // "CosmeticInventories" DbSets map to those plural table names by EF default convention,
            // but the real tables (created via raw SQL in RunMigrations, and the only ones the rest of
            // the app — e.g. PaymentController's Stripe webhook — ever reads or writes) are named
            // "Payment" and "CosmeticInventory" (singular). Using the EF DbSets here would silently
            // insert into empty, unused phantom tables instead. Payment isn't one of the 5 named seed
            // groups; these rows exist purely to satisfy CosmeticInventory.PaymentId's FK constraint.
            {
                var conn = (SqliteConnection)context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                using var checkCmd = conn.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Payment";
                if (Convert.ToInt32(checkCmd.ExecuteScalar()) == 0)
                {
                    var payments = new (int PaymentId, string UserName, string CosmeticId, string CosmeticName, string CosmeticType, decimal Amount, string StripeSessionId, string CreatedAt)[]
                    {
                        (18, "Admin1", "TREEK", "Treekachu", "Sticker", 10m, "cs_test_a1U8JiQqJrMcgtkKsHWQa0FI9s7C8QxlhbKL6OKmHmp2DgejQDraOPx6gH", "2026-08-10T17:18:15.6045188Z"),
                        (21, "Admin1", "FRM_C192B1", "Royal Frame", "Frame", 14m, "cs_test_a1zAYC7O5vJs8tCmKZHRK2JbA7M2FFBGh26PyKggo0oiQuS24RhJB7osbv", "2026-08-10T17:55:18.2291349Z"),
                        (22, "Admin1", "FRM_CF6617", "Tranquil", "Frame", 5m, "cs_test_a1agtuxpkWsY9W5auCH8NZInWEsgooGPRpvCKwNAuL2wSMIvX5vnwaolAR", "2026-08-11T09:48:01.4742878Z"),
                        (23, "Allen", "FRM_E6CBF2", "SCI-FI", "Frame", 2m, "cs_test_a1ubfn9jpNVnDcrUuwHqsgwKnyc8IvKiC7JVAN9xMiJkczh7NBaCpS9sz8", "2026-08-11T15:53:29.5925199Z"),
                        (24, "Admin1", "STK_CAC9B2", "Neon", "Sticker", 0m, "cs_test_a1F6KN70hWdjiHjSwL7ttlSlLA0Yq58dXEWJ0x7FJijFetmR6OooD9k2RM", "2026-08-11T16:09:33.2620975Z"),
                        (25, "Admin1", "FRM_39C37E", "Stream of lights", "Frame", 0m, "cs_test_a1UOgQm2etK0D2rJWH7dGU0kBwFE0wBfKVGa3bsAAneA8yr79M5Rw0F8sh", "2026-08-11T16:13:36.4689399Z"),
                        (26, "Admin1", "FRM_39C37E", "Stream of lights", "Frame", 0m, "cs_test_a1ZJTz5mRTacojGRCfkaaNKViL5AeDHp1Xs5Kd2s6CnlekJgcwr93qrLck", "2026-08-11T16:15:25.3798874Z"),
                        (27, "Admin1", "FRM_1466B6", "huH.", "Frame", 0m, "cs_test_a1GfGTCe6AeTkCHtSo0lBTa1fM9CB8pRfUG8KWDjXu9aGQX7IeieAfGfnr", "2026-08-11T16:15:56.0126489Z"),
                    };

                    foreach (var p in payments)
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = @"
                            INSERT INTO Payment (PaymentId, UserName, CosmeticId, CosmeticName, CosmeticType, Amount, Currency, StripeSessionId, Status, CreatedAt)
                            VALUES (@PaymentId, @UserName, @CosmeticId, @CosmeticName, @CosmeticType, @Amount, 'SGD', @StripeSessionId, 'Success', @CreatedAt)";
                        cmd.Parameters.AddWithValue("@PaymentId", p.PaymentId);
                        cmd.Parameters.AddWithValue("@UserName", p.UserName);
                        cmd.Parameters.AddWithValue("@CosmeticId", p.CosmeticId);
                        cmd.Parameters.AddWithValue("@CosmeticName", p.CosmeticName);
                        cmd.Parameters.AddWithValue("@CosmeticType", p.CosmeticType);
                        cmd.Parameters.AddWithValue("@Amount", p.Amount);
                        cmd.Parameters.AddWithValue("@StripeSessionId", p.StripeSessionId);
                        cmd.Parameters.AddWithValue("@CreatedAt", p.CreatedAt);
                        cmd.ExecuteNonQuery();
                    }
                }

                using var checkCmd2 = conn.CreateCommand();
                checkCmd2.CommandText = "SELECT COUNT(*) FROM CosmeticInventory";
                if (Convert.ToInt32(checkCmd2.ExecuteScalar()) == 0)
                {
                    var inventory = new (string Uid, string UserName, string CosmeticId, string CosmeticType, string AcquiredAt, int PaymentId)[]
                    {
                        ("3fbf1a4d-2637-49ec-b723-a7163715f20b", "Admin1", "TREEK", "Sticker", "2026-08-10T17:18:54.5769561Z", 18),
                        ("ec0bfed4-c138-4066-aa4e-e1aba599fbe9", "Admin1", "FRM_C192B1", "Frame", "2026-08-10T17:55:35.8778772Z", 21),
                        ("ee1b46b6-6bae-4c0e-a514-f99dcf017a4d", "Admin1", "FRM_CF6617", "Frame", "2026-08-11T09:48:17.1221037Z", 22),
                        ("debe3875-6dee-4ac8-80f7-31bceae9145f", "Allen", "FRM_E6CBF2", "Frame", "2026-08-11T15:53:59.8424910Z", 23),
                        ("f0baebd9-2f1d-4813-9038-4a5e288fd890", "Admin1", "STK_CAC9B2", "Sticker", "2026-08-11T16:09:39.2216448Z", 24),
                        ("3c36d26a-7f97-4bad-b319-033f4321fc85", "Admin1", "FRM_39C37E", "Frame", "2026-08-11T16:13:40.6293474Z", 25),
                        ("491972bb-8e01-43e2-a34c-352a673c19bf", "Admin1", "FRM_39C37E", "Frame", "2026-08-11T16:15:29.7495173Z", 26),
                        ("18112e0d-201b-4c57-9d9c-84ce23e7ab1e", "Admin1", "FRM_1466B6", "Frame", "2026-08-11T16:16:01.0266502Z", 27),
                    };

                    foreach (var i in inventory)
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = @"
                            INSERT INTO CosmeticInventory (Uid, UserName, CosmeticId, CosmeticType, AcquiredAt, PaymentId)
                            VALUES (@Uid, @UserName, @CosmeticId, @CosmeticType, @AcquiredAt, @PaymentId)";
                        cmd.Parameters.AddWithValue("@Uid", i.Uid);
                        cmd.Parameters.AddWithValue("@UserName", i.UserName);
                        cmd.Parameters.AddWithValue("@CosmeticId", i.CosmeticId);
                        cmd.Parameters.AddWithValue("@CosmeticType", i.CosmeticType);
                        cmd.Parameters.AddWithValue("@AcquiredAt", i.AcquiredAt);
                        cmd.Parameters.AddWithValue("@PaymentId", i.PaymentId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // Seed Report Assignment Counter
            if (!context.ReportAssignmentCounters.Any())
            {
                context.ReportAssignmentCounters.Add(new ReportAssignmentCounter { Id = 1, Counter = 1 });
                context.SaveChanges();
            }

            // Seed Ticket Assignment Counter
            if (!context.TicketAssignmentCounters.Any())
            {
                context.TicketAssignmentCounters.Add(new TicketAssignmentCounter { Id = 1, Counter = 3 });
                context.SaveChanges();
            }
        }

        private static void RunMigrations(ArcaneDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            connection.Open();

            using var cmd = connection.CreateCommand();

            // Add Description column to Showoffs (if not exists)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('Showoffs') WHERE name='Description'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE Showoffs ADD COLUMN Description TEXT";
                cmd.ExecuteNonQuery();
            }

            // Add IsDisabled column to Showoffs (if not exists)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('Showoffs') WHERE name='IsDisabled'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE Showoffs ADD COLUMN IsDisabled INTEGER DEFAULT 0";
                cmd.ExecuteNonQuery();
            }

            // Add DisabledAt column to Showoffs (if not exists)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('Showoffs') WHERE name='DisabledAt'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE Showoffs ADD COLUMN DisabledAt TEXT";
                cmd.ExecuteNonQuery();
            }

            // Add Orientation column to Showoffs (if not exists)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('Showoffs') WHERE name='Orientation'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE Showoffs ADD COLUMN Orientation TEXT DEFAULT 'Landscape'";
                cmd.ExecuteNonQuery();
            }

            // Add EquippedFrameCode column to Showoffs (if not exists)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('Showoffs') WHERE name='EquippedFrameCode'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE Showoffs ADD COLUMN EquippedFrameCode TEXT";
                cmd.ExecuteNonQuery();
            }

            // Add IsActive column to ArcaneVaultUsers (staff availability toggle)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('ArcaneVaultUsers') WHERE name='IsActive'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE ArcaneVaultUsers ADD COLUMN IsActive INTEGER DEFAULT 1";
                cmd.ExecuteNonQuery();
            }

            // Add LastNotificationCheck column to ArcaneVaultUsers (Phase 11 notifications)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('ArcaneVaultUsers') WHERE name='LastNotificationCheck'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE ArcaneVaultUsers ADD COLUMN LastNotificationCheck TEXT";
                cmd.ExecuteNonQuery();
            }

            // Add UserName column to SupportTickets — captures the logged-in submitter, if any (Phase 11 notifications)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('SupportTickets') WHERE name='UserName'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE SupportTickets ADD COLUMN UserName TEXT";
                cmd.ExecuteNonQuery();
            }

            // Add RepliedAt column to SupportTickets (Phase 11 notifications)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('SupportTickets') WHERE name='RepliedAt'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE SupportTickets ADD COLUMN RepliedAt TEXT";
                cmd.ExecuteNonQuery();
            }

            // Add ImagePublicId column to Frames (if not exists)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('Frames') WHERE name='ImagePublicId'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE Frames ADD COLUMN ImagePublicId TEXT";
                cmd.ExecuteNonQuery();
            }

            // Add ImagePublicId column to Stickers (if not exists)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('Stickers') WHERE name='ImagePublicId'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE Stickers ADD COLUMN ImagePublicId TEXT";
                cmd.ExecuteNonQuery();
            }

            // Add Shop columns (Price/IsLimited/LimitedQuantity/IsDelisted) to Frames and Stickers,
            // plus Orientation on Frames only.
            foreach (var table in new[] { "Frames", "Stickers" })
            {
                cmd.CommandText = $@"
                    SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name='Price'";
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN Price REAL DEFAULT 0";
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = $@"
                    SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name='IsLimited'";
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN IsLimited INTEGER DEFAULT 0";
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = $@"
                    SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name='LimitedQuantity'";
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN LimitedQuantity INTEGER";
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = $@"
                    SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name='IsDelisted'";
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN IsDelisted INTEGER DEFAULT 0";
                    cmd.ExecuteNonQuery();
                }
            }

            // Add Orientation column to Frames (if not exists)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('Frames') WHERE name='Orientation'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE Frames ADD COLUMN Orientation TEXT DEFAULT 'Landscape'";
                cmd.ExecuteNonQuery();
            }

            // Create ShowoffModerationEvent table — history of disable/re-enable actions (Phase 11 notifications)
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ShowoffModerationEvent (
                    EventId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ShowoffId INTEGER NOT NULL,
                    ShowoffName TEXT NOT NULL,
                    EventType TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (ShowoffId) REFERENCES Showoffs(ShowoffId)
                )";
            cmd.ExecuteNonQuery();

            // Create ShowoffAppeal table — persists appeal message + staff decision (Phase 11 notifications)
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ShowoffAppeal (
                    AppealId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ShowoffId INTEGER NOT NULL,
                    ShowoffName TEXT NOT NULL,
                    UserName TEXT NOT NULL,
                    AppealMessage TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    StaffNote TEXT,
                    SubmittedAt TEXT NOT NULL,
                    DecidedAt TEXT,
                    FOREIGN KEY (ShowoffId) REFERENCES Showoffs(ShowoffId)
                )";
            cmd.ExecuteNonQuery();

            // Create ShowoffStickerPlacement table — per-showoff sticker positions (Phase 10 equipping)
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ShowoffStickerPlacement (
                    PlacementId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ShowoffId INTEGER NOT NULL,
                    StickerCode TEXT NOT NULL,
                    ImagePath TEXT,
                    X REAL NOT NULL,
                    Y REAL NOT NULL,
                    Scale REAL NOT NULL DEFAULT 1.0,
                    Rotation REAL NOT NULL DEFAULT 0,
                    FOREIGN KEY (ShowoffId) REFERENCES Showoffs(ShowoffId)
                )";
            cmd.ExecuteNonQuery();

            // Create CosmeticAuditLog table — tamper-evident hash-chained log of Manage Shop actions
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS CosmeticAuditLog (
                    LogId INTEGER PRIMARY KEY AUTOINCREMENT,
                    AdminId TEXT NOT NULL,
                    AdminName TEXT NOT NULL,
                    Timestamp TEXT NOT NULL,
                    CosmeticId TEXT NOT NULL,
                    CosmeticName TEXT NOT NULL,
                    CosmeticType TEXT NOT NULL,
                    Action TEXT NOT NULL,
                    EntryHash TEXT NOT NULL,
                    PreviousEntryHash TEXT NOT NULL
                )";
            cmd.ExecuteNonQuery();

            // Create Payment table
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Payment (
                    PaymentId INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName TEXT NOT NULL,
                    CosmeticId TEXT NOT NULL,
                    CosmeticName TEXT NOT NULL,
                    CosmeticType TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    Currency TEXT NOT NULL,
                    StripeSessionId TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                )";
            cmd.ExecuteNonQuery();

            // Create CosmeticInventory table — minimal ownership record written by
            // the Stripe webhook on a successful Payment (full Inventory UI is Phase 9)
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS CosmeticInventory (
                    Uid TEXT PRIMARY KEY,
                    UserName TEXT NOT NULL,
                    CosmeticId TEXT NOT NULL,
                    CosmeticType TEXT NOT NULL,
                    AcquiredAt TEXT NOT NULL,
                    PaymentId INTEGER NOT NULL,
                    FOREIGN KEY (PaymentId) REFERENCES Payment(PaymentId)
                )";
            cmd.ExecuteNonQuery();

            // Create ShowoffComments table
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ShowoffComments (
                    CommentId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ShowoffId INTEGER NOT NULL,
                    UserName TEXT NOT NULL,
                    Text TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (ShowoffId) REFERENCES Showoffs(ShowoffId),
                    FOREIGN KEY (UserName) REFERENCES ArcaneVaultUsers(UserName)
                )";
            cmd.ExecuteNonQuery();

            // Create ShowoffReports table
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ShowoffReports (
                    ReportId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ShowoffId INTEGER NOT NULL,
                    ReporterUserName TEXT NOT NULL,
                    AssignedStaffUserName TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Reason TEXT,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (ShowoffId) REFERENCES Showoffs(ShowoffId),
                    FOREIGN KEY (ReporterUserName) REFERENCES ArcaneVaultUsers(UserName)
                )";
            cmd.ExecuteNonQuery();

            // Create ReportAssignmentCounters table
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ReportAssignmentCounters (
                    Id INTEGER PRIMARY KEY,
                    Counter INTEGER NOT NULL DEFAULT 0
                )";
            cmd.ExecuteNonQuery();

            // Create SupportTickets table
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS SupportTickets (
                    TicketId INTEGER PRIMARY KEY AUTOINCREMENT,
                    TicketUid TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Subject TEXT NOT NULL,
                    Issue TEXT NOT NULL,
                    IsResolved INTEGER NOT NULL DEFAULT 0,
                    AssignedStaffUserName TEXT,
                    CreatedAt TEXT NOT NULL
                )";
            cmd.ExecuteNonQuery();

            // Create TicketAssignmentCounters table (round-robin, mirrors ReportAssignmentCounters)
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS TicketAssignmentCounters (
                    Id INTEGER PRIMARY KEY,
                    Counter INTEGER NOT NULL DEFAULT 0
                )";
            cmd.ExecuteNonQuery();

            connection.Close();
        }
    }
}
