using EmployeeService.Models;
namespace EmployeeService.Services
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetEmployeeByIdAsync(string employeeId);
        Task<IEnumerable<Employee>> GetEmployeesByManagerAsync(string managerId);
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task <IEnumerable<LeaveBalance>> GetLeaveBalancesAsync(string employeeId);
        Task<LeaveBalance?> GetLeaveBalanceAsync(string employeeId, string leaveType);
        Task<bool> HasSufficientBalanceAsync(string employeeId, string leaveType, int days);
        Task<LeaveBalance> DeductLeaveAsync(string employeeId, string leaveType, int days);
        Task<string> GetNextEmployeeIdAsync();
        Task<bool> EmailExistsAsync(string email);
    }
}