using Microsoft.EntityFrameworkCore;
using ITTP23.Models;

namespace ITTP23.Storage
{
    public class AutoDataContext : DbContext
    {
        public AutoDataContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(p => new { p.Login })
                .IsUnique(true);

            modelBuilder.Entity<User>()
                .HasIndex(p => new { p.Token })
                .IsUnique(true);
        }
    }
}
