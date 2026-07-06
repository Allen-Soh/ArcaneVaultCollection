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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionItemCategory>()
                .HasKey(c => new { c.ItemId, c.CategoryCode });
        }
    }
}
