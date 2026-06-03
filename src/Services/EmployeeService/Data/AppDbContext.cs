
using EmployeeService.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
            public DbSet<Employee> Employees{get;set;}
            public DbSet<LeaveBalance> LeaveBalances{get;set;}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeaveBalance>().HasOne<Employee>().WithMany().HasForeignKey(lb=>lb.EmployeeId);

            modelBuilder.Entity<Employee>().HasData
            (
            new Employee{
            EmployeeId="usr-mgr-001",
            Name="Mini",
            Email="mgr@gmail.com",
            Role="Manager",
            ManagerId=null,
            CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc)},
            new Employee{
            EmployeeId="usr-emp-001",
            Name="Arav",
            Email="memp@gmail.com",
            Role="Emp",
            ManagerId="usr-mgr-001",
            CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc)}
             );
 
           modelBuilder.Entity<LeaveBalance>().HasData
            (
            new LeaveBalance{
            Id=1,
            EmployeeId="usr-mgr-001",
            LeaveType="Casual",
            TotalAllocated=15,
            Used=0},
            new LeaveBalance{
            Id=2,
            EmployeeId="usr-mgr-001",
            LeaveType="Sick",
            TotalAllocated=15,
            Used=0},new LeaveBalance{
            Id=3,
            EmployeeId="usr-mgr-001",
            LeaveType="Privilege",
            TotalAllocated=15,
            Used=0},
            new LeaveBalance{
            Id=4,
            EmployeeId="usr-emp-001",
            LeaveType="Casual",
            TotalAllocated=15,
            Used=0},
            new LeaveBalance{
            Id=5,
            EmployeeId="usr-emp-001",
            LeaveType="Sick",
            TotalAllocated=15,
            Used=0},new LeaveBalance{
            Id=6,
            EmployeeId="usr-emp-001",
            LeaveType="Privilege",
            TotalAllocated=15,
            Used=0}
             );

        }
    }
}