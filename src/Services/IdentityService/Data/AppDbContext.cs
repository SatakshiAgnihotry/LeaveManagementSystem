using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options){}

        public DbSet<User> Users{get;set;}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
            new User{
            UserId="usr-mgr-001",
            Email="mgr@gmail.com",
            PasswordHash=BCrypt.Net.BCrypt.HashPassword("Password@123"),
            Name="Mini",
            Role="Manager",
            ManagerId=null},
            new User{
            UserId="usr-emp-001",
            Email="emp@gmail.com",
            PasswordHash=BCrypt.Net.BCrypt.HashPassword("Password@123"),
            Name="Arav",
            Role="Emp",
            ManagerId="usr-mgr-001"}
        );
        }

    }
}