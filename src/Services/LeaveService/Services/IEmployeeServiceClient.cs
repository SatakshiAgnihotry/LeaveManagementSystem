namespace LeaveService.Services
{
    public interface IEmployeeServiceClient
    {
        // Get employee details by ID — returns null if not found
        Task<bool> HasSufficientLeaveBalanceAsync(string employeeId, string leaveType, int days);
        Task DeductLeaveBalanceAsync(string employeeId, string leaveType, int days);
        Task<string> CallDemoEndpointAsync(string input);
    }
}