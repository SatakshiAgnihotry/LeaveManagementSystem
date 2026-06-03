using LeaveService.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options){}

        public DbSet<LeaveRequest> LeaveRequests{get;set;}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeaveRequest>().HasIndex(x=>new{x.EmployeeId,x.StartDate,x.EndDate}); // For filtering and sorting
             modelBuilder.Entity<LeaveRequest>().HasIndex(x=>new{x.ManagerId,x.Status}); // For manager's pending requests
        }

    }
}