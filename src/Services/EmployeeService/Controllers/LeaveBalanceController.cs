using System.Security.Claims;
using EmployeeService.Models;
using EmployeeService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class LeaveBalanceController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IConfiguration _configuration;

    public LeaveBalanceController(IEmployeeRepository employeeRepository, IConfiguration configuration)
    {
        _employeeRepository = employeeRepository;
        _configuration = configuration;
    }

    [HttpGet("{employeeId}/leave-balances")]
    public async Task<IActionResult> GetLeaveBalances(string employeeId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(currentUserId) || string.IsNullOrWhiteSpace(role))
            return Unauthorized(ApiResponse<object>.Fail("Invalid token claims."));

        var targetEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
        if (targetEmployee == null)
            return NotFound(ApiResponse<object>.Fail("Employee not found."));

        if (role == "Employee" && employeeId != currentUserId)
            return Forbid();

        if (role == "Manager" && employeeId != currentUserId && targetEmployee.ManagerId != currentUserId)
            return Forbid();

        var balances = await _employeeRepository.GetLeaveBalancesAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<LeaveBalance>>.Ok(balances));
    }

    [AllowAnonymous]
    [HttpGet("{employeeId}/leave-balances/{leaveType}/check")]
    public async Task<IActionResult> CheckLeaveBalance(string employeeId, string leaveType, [FromQuery] int days)
    {
        if (!IsInternalServiceRequest())
            return Unauthorized(ApiResponse<object>.Fail("Invalid service API key."));

        if (!LeaveTypes.IsValid(leaveType))
            return BadRequest(ApiResponse<object>.Fail("Invalid leave type."));

        if (days <= 0)
            return BadRequest(ApiResponse<object>.Fail("Days must be greater than zero."));

        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
        if (employee == null)
            return NotFound(ApiResponse<object>.Fail("Employee not found."));

        var hasSufficient = await _employeeRepository.HasSufficientBalanceAsync(employeeId, leaveType, days);

        return Ok(ApiResponse<object>.Ok(new
        {
            employeeId,
            leaveType,
            requestedDays = days,
            hasSufficientBalance = hasSufficient
        }));
    }

    [AllowAnonymous]
    [HttpPut("{employeeId}/leave-balances/deduct")]
    public async Task<IActionResult> DeductLeaveBalance(string employeeId, [FromBody] DeductLeaveRequest request)
    {
        if (!IsInternalServiceRequest())
            return Unauthorized(ApiResponse<object>.Fail("Invalid service API key."));

        if (request == null)
            return BadRequest(ApiResponse<object>.Fail("Request body is required."));

        if (!LeaveTypes.IsValid(request.LeaveType))
            return BadRequest(ApiResponse<object>.Fail("Invalid leave type."));

        if (request.Days <= 0)
            return BadRequest(ApiResponse<object>.Fail("Days must be greater than zero."));

        var updated = await _employeeRepository.DeductLeaveAsync(employeeId, request.LeaveType, request.Days);

        return Ok(ApiResponse<LeaveBalance>.Ok(updated, "Leave balance deducted successfully."));
    }

    private bool IsInternalServiceRequest()
    {
        var configuredKey = _configuration["ServiceApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey))
            return false;

        if (!Request.Headers.TryGetValue("X-Service-Api-Key", out var providedKey))
            return false;

        return string.Equals(providedKey.ToString(), configuredKey, StringComparison.Ordinal);
    }
}