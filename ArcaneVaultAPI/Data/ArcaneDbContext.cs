using Microsoft.EntityFrameworkCore;

namespace ArcaneVaultAPI.Data
{
    public class ArcaneDbContext : DbContext
    {
        public ArcaneDbContext(DbContextOptions<ArcaneDbContext> options) : base(options) { }

        public DbSet<ArcaneVaultUser> ArcaneVaultUsers { get; set; }
        public DbSet<ArcaneVaultUserRoles> ArcaneVaultUserRoles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CollectionItem> CollectionItems { get; set; }
        public DbSet<CollectionItemCategory> CollectionItemCategories { get; set; }
        public DbSet<Showoff> Showoffs { get; set;  }
        public DbSet<ShowoffItem> ShowoffItems { get; set; }
        public DbSet<ShowoffLike> ShowoffLikes { get; set; }
        public DbSet<ShowoffView> ShowoffViews { get; set; }
        public DbSet<ShowoffComment> ShowoffComments { get; set; }
        public DbSet<ShowoffReport> ShowoffReports { get; set; }
        public DbSet<ReportAssignmentCounter> ReportAssignmentCounters { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<SearchLog> SearchLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionItemCategory>()
                .HasKey(c => new { c.ItemId, c.CategoryCode });

            // ShowoffItem composite PK
            modelBuilder.Entity<ShowoffItem>()
                .HasKey(s => new { s.ShowoffId, s.ItemId });

            // ShowoffLike composite PK
            modelBuilder.Entity<ShowoffLike>()
                .HasKey(s => new { s.ShowoffId, s.UserName });
        }
    }
}
