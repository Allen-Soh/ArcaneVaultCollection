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

            // Seed Users
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
                        IsActive = false
                    },
                    new ArcaneVaultUser
                    {
                        UserName = "Admin1",
                        Email = "admin1@example.com",
                        IsDeleted = false,
                        RoleId = 1,
                        Password = "Admin123",
                        Country = "Singapore",
                        AvatarUrl = "https://res.cloudinary.com/nyj7a9za/image/upload/v1785661451/arcanevault/showoffs/yhvlnmpzeoy4n773kxwu.jpg",
                        IsActive = true
                    },
                    new ArcaneVaultUser
                    {
                        UserName = "Admin2",
                        Email = "apocoloto@gmail.com",
                        IsDeleted = false,
                        RoleId = 1,
                        Password = "Admin123",
                        Country = "Singapore",
                        IsActive = true
                    },
                    new ArcaneVaultUser
                    {
                        UserName = "Allen",
                        Email = "allen@example.com",
                        IsDeleted = false,
                        RoleId = 2,
                        Password = "Password123",
                        Country = "Malaysia",
                        AvatarUrl = "https://res.cloudinary.com/nyj7a9za/image/upload/v1785665644/arcanevault/showoffs/jtu5vm8ognpxjp05zc8k.jpg"
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
                    },
                    new CollectionItem
                    {
                    ItemId = 7,
                        ItemName = "Pikachu Surging Sparks SIR Card",
                        IsDeleted = false,
                        StartingQuantity = 1,
                        CurrentQuantity = 1,
                        UserName = "Allen"
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
                    },
                    new Showoff
                    {
                        ShowoffId = 4,
                        ShowoffName = "Private Collection",
                        BackgroundColor = "#e0f7fa",
                        BackgroundImagePath = "https://res.cloudinary.com/nyj7a9za/image/upload/v1785909325/arcanevault/showoffs/moazvhkgqtcub3fjb7go.jpg",
                        BackgroundImagePublicId = "arcanevault/showoffs/moazvhkgqtcub3fjb7go",
                        IsPrivate = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        UserName = "Allen"
                    },
                    new Showoff
                    {
                        ShowoffId = 5,
                        ShowoffName = "Disabled Collection",
                        BackgroundColor = "#fbe9e7",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-4),
                        UserName = "Allen2",
                        IsDisabled = true,
                        DisabledAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new Showoff
                    {
                        ShowoffId = 6,
                        ShowoffName = "Deleted Collection",
                        BackgroundColor = "#e8f5e9",
                        IsPrivate = false,
                        IsDeleted = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-6),
                        UserName = "Allen3"
                    },
                    new Showoff
                    {
                        ShowoffId = 7,
                        ShowoffName = "My Action Figures",
                        BackgroundColor = "#fff3e0",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-7),
                        UserName = "Allen"
                    },
                    new Showoff
                    {
                        ShowoffId = 8,
                        ShowoffName = "My Coin Collection",
                        BackgroundColor = "#f1f8e9",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-8),
                        UserName = "Allen2"
                    },
                    new Showoff
                    {
                        ShowoffId = 9,
                        ShowoffName = "My Poster Collection",
                        BackgroundColor = "#fce4ec",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-9),
                        UserName = "Allen3"
                    },
                    new Showoff
                    {
                        ShowoffId = 10,
                        ShowoffName = "My Board Games",
                        BackgroundColor = "#e0f2f1",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        UserName = "Allen"
                    },
                    new Showoff
                    {
                        ShowoffId = 11,
                        ShowoffName = "My Vinyl Records",
                        BackgroundColor = "#ede7f6",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-11),
                        UserName = "Allen2"
                    },
                    new Showoff
                    {
                        ShowoffId = 12,
                        ShowoffName = "My Comic Books",
                        BackgroundColor = "#fff8e1",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-12),
                        UserName = "Allen3"
                    },
                    new Showoff
                    {
                        ShowoffId = 13,
                        ShowoffName = "My Rare Coins",
                        BackgroundColor = "#e8f5e9",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-13),
                        UserName = "Allen"
                    },
                    new Showoff
                    {
                        ShowoffId = 14,
                        ShowoffName = "My Poster Collection",
                        BackgroundColor = "#fce4ec",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-14),
                        UserName = "Allen2"
                    },
                    new Showoff
                    {
                        ShowoffId = 15,
                        ShowoffName = "My Board Games",
                        BackgroundColor = "#e0f2f1",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-15),
                        UserName = "Allen3"
                    },
                    new Showoff
                    {
                        ShowoffId = 16,
                        ShowoffName = "My Vinyl Records",
                        BackgroundColor = "#ede7f6",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-16),
                        UserName = "Allen"
                    },
                    new Showoff
                    {
                        ShowoffId = 17,
                        ShowoffName = "My Comic Books",
                        BackgroundColor = "#fff8e1",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-17),
                        UserName = "Allen2"
                    },
                    new Showoff
                    {
                        ShowoffId = 18,
                        ShowoffName = "My Rare Coins",
                        BackgroundColor = "#e8f5e9",
                        IsPrivate = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-18),
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

            // Seed Report Assignment Counter
            if (!context.ReportAssignmentCounters.Any())
            {
                context.ReportAssignmentCounters.Add(new ReportAssignmentCounter { Id = 1, Counter = 0 });
                context.SaveChanges();
            }

            // Seed Ticket Assignment Counter
            if (!context.TicketAssignmentCounters.Any())
            {
                context.TicketAssignmentCounters.Add(new TicketAssignmentCounter { Id = 1, Counter = 0 });
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

            // Add IsActive column to ArcaneVaultUsers (staff availability toggle)
            cmd.CommandText = @"
                SELECT COUNT(*) FROM pragma_table_info('ArcaneVaultUsers') WHERE name='IsActive'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                cmd.CommandText = "ALTER TABLE ArcaneVaultUsers ADD COLUMN IsActive INTEGER DEFAULT 1";
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