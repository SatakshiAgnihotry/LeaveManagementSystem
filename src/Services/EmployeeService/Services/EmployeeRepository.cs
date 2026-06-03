using EmployeeService.Data;
using EmployeeService.Models;
using EmployeeService.Middleware;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Services;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetEmployeeByIdAsync(string employeeId)
    {
        // Find() works only for primary keys — for anything else use FirstOrDefaultAsync
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByManagerAsync(string managerId)
    {
        // Get all employees where their ManagerId matches
        return await _context.Employees
            .Where(e => e.ManagerId == managerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        return await _context.Employees.ToListAsync();
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        // Add the new employee to the DbSet (not saved yet)
        _context.Employees.Add(employee);

        // Create 3 leave balance rows automatically for the new employee
        _context.LeaveBalances.AddRange(
            new LeaveBalance { EmployeeId = employee.EmployeeId, LeaveType = LeaveTypes.Casual,    TotalAllocated = 12, Used = 0 },
            new LeaveBalance { EmployeeId = employee.EmployeeId, LeaveType = LeaveTypes.Sick,      TotalAllocated = 10, Used = 0 },
            new LeaveBalance { EmployeeId = employee.EmployeeId, LeaveType = LeaveTypes.Privilege, TotalAllocated = 15, Used = 0 }
        );

        // SaveChangesAsync writes ALL pending changes to the database in one transaction
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<IEnumerable<LeaveBalance>> GetLeaveBalancesAsync(string employeeId)
    {
        return await _context.LeaveBalances
            .Where(lb => lb.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<LeaveBalance?> GetLeaveBalanceAsync(string employeeId, string leaveType)
    {
        return await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.LeaveType == leaveType);
    }

    public async Task<bool> HasSufficientBalanceAsync(string employeeId, string leaveType, int days)
    {
        var balance = await GetLeaveBalanceAsync(employeeId, leaveType);

        // If no balance record found — return false (safe default)
        if (balance == null) return false;

        // Remaining is a computed property: TotalAllocated - Used
        return balance.Remaining >= days;
    }

    public async Task<LeaveBalance> DeductLeaveAsync(string employeeId, string leaveType, int days)
    {
        var balance = await GetLeaveBalanceAsync(employeeId, leaveType);

        if (balance == null)
            throw new NotFoundException($"Leave balance not found for employee {employeeId}, type {leaveType}");

        if (balance.Remaining < days)
            throw new BadRequestException($"Insufficient leave balance. Requested: {days}, Available: {balance.Remaining}");

        // EF Core tracks this object — just mutate it and call SaveChanges
        balance.Used += days;
        await _context.SaveChangesAsync();

        return balance;
    }

    public async Task<string> GetNextEmployeeIdAsync()
    {
        // Get all employees and extract numeric suffix from IDs in format "usr-emp-XXX"
        var employees = await _context.Employees.ToListAsync();
        
        if (!employees.Any())
            return "usr-emp-001";

        // Extract numeric part and find max
        var maxNumber = employees
            .Select(e => e.EmployeeId)
            .Where(id => id.StartsWith("usr-emp-"))
            .Select(id => id.Substring("usr-emp-".Length))
            .Where(suffix => int.TryParse(suffix, out _))
            .Select(suffix => int.Parse(suffix))
            .DefaultIfEmpty(0)
            .Max();

        return $"usr-emp-{(maxNumber + 1):D3}";
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        // Check if an employee with this email already exists
        return await _context.Employees
            .AnyAsync(e => e.Email == email.ToLowerInvariant());
    }
}