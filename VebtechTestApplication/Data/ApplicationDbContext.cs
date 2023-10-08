using Microsoft.EntityFrameworkCore;
using VebtechTestApplication.Models;

namespace VebtechTestApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = "User" },
                new Role { RoleId = 2, Name = "Admin" },
                new Role { RoleId = 3, Name = "Support" },
                new Role { RoleId = 4, Name = "SuperAdmin" }
            );
        }
    }
}
