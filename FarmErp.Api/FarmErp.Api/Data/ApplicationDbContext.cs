using FarmErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmErp.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Farm> Farms => Set<Farm>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Farm>(b =>
            {
                b.HasKey(f => f.Id);
                // Store as SQL Server 'geography' with SRID 4326 (WGS84) / postgres, use 'geometry' with SRID 4326
                b.Property(f => f.Area)
                    .HasColumnType("geography");
                
                b.Property(f => f.Name)
                    .HasMaxLength(200)
                    .IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
