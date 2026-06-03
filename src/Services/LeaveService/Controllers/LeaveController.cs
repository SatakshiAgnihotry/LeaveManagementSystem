using System.Security.Claims;
using LeaveService.Middleware;
using LeaveService.Models;
using LeaveService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace LeaveService.Controllers;

[ApiController]
[Route("api/leaves")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveRepository _leaveRepository;
    private readonly IEmployeeServiceClient _employeeServiceClient;
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<LeaveController> _logger;

    public LeaveController(
        ILeaveRepository leaveRepository,
        IEmployeeServiceClient employeeServiceClient,
        IRabbitMQPublisher publisher,
        ILogger<LeaveController> logger)
    {
        _leaveRepository = leaveRepository;
        _employeeServiceClient = employeeServiceClient;
        _publisher = publisher;
        _logger = logger;
    }

    // POST /api/leaves/apply — Employee applies for leave
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveRequest request)
    {
        var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var employeeName = User.FindFirstValue(ClaimTypes.Name)!;
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        // Seeded employee role in IdentityService is "Emp"
        if (role != "Emp")
            return Forbid();

        // Validate leave type
        if (!LeaveTypes.IsValid(request.LeaveType))
            return BadRequest(ApiResponse<object>.Fail("Invalid leave type. Use Casual, Sick or Privilege."));

        // Validate dates
        if (request.StartDate >= request.EndDate)
            return BadRequest(ApiResponse<object>.Fail("End date must be after start date."));

        if (request.StartDate < DateTime.UtcNow.Date)
            return BadRequest(ApiResponse<object>.Fail("Cannot apply leave for past dates."));

        // Check for overlapping leave
        var hasOverlap = await _leaveRepository.HasOverlappingLeaveAsync(employeeId, request.StartDate, request.EndDate);
        if (hasOverlap)
            return Conflict(ApiResponse<object>.Fail("You already have a pending or approved leave in this date range."));

        // Check balance via EmployeeService
        var hasSufficientBalance = await _employeeServiceClient.HasSufficientLeaveBalanceAsync(employeeId, request.LeaveType, request.NumberOfDays);
        if (!hasSufficientBalance)
            return BadRequest(ApiResponse<object>.Fail($"Insufficient {request.LeaveType} leave balance."));

        // Create and save the leave request
        var leaveRequest = new LeaveRequest
        {
            LeaveRequestId = Guid.NewGuid().ToString(),
            EmployeeId = employeeId,
            EmployeeName = employeeName,
            ManagerId = request.ManagerId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NumberOfDays = request.NumberOfDays,
            Reason = request.Reason,
            Status = LeaveRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _leaveRepository.ApplyLeaveAsync(leaveRequest);

        // Publish event to RabbitMQ — notify employee and manager
        _publisher.PublishLeaveApplied(new LeaveEventMessage(
            created.LeaveRequestId, created.EmployeeId, created.EmployeeName,
            created.ManagerId, created.LeaveType, created.StartDate, created.EndDate,
            created.NumberOfDays, created.Reason, null, "LeaveApplied", DateTime.UtcNow));

        return CreatedAtAction(nameof(GetLeaveById),
            new { leaveRequestId = created.LeaveRequestId },
            ApiResponse<LeaveRequest>.Ok(created, "Leave request submitted successfully."));
    }

    // GET /api/leaves/{leaveRequestId}
    [HttpGet("{leaveRequestId}")]
    public async Task<IActionResult> GetLeaveById(string leaveRequestId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        var leave = await _leaveRepository.GetByIdAsync(leaveRequestId);
        if (leave == null)
            return NotFound(ApiResponse<object>.Fail("Leave request not found."));

        _logger.LogInformation("Retrieving leave request: {LeaveRequestId}", leaveRequestId);

        // Employee can only see their own; Manager can only see their team's
        if (role == "Emp" && leave.EmployeeId != currentUserId)
            return Forbid();

        if (role == "Manager" && leave.ManagerId != currentUserId)
            return Forbid();

        return Ok(ApiResponse<LeaveRequest>.Ok(leave));
    }

    // GET /api/leaves/history — Employee's own leave history
    [HttpGet("history")]
    public async Task<IActionResult> GetMyLeaveHistory([FromQuery] LeaveFilterRequest filter)
    {
        var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        // Employee role is seeded as "Emp"
        if (role != "Emp")
            return Forbid();

        var result = await _leaveRepository.GetLeaveHistoryAsync(employeeId, filter);
        return Ok(ApiResponse<PagedResult<LeaveRequest>>.Ok(result));
    }

    // GET /api/leaves/manager/requests — Manager sees team requests
    [HttpGet("manager/requests")]
    public async Task<IActionResult> GetManagerRequests([FromQuery] LeaveFilterRequest filter)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        if (role != "Manager")
            return Forbid();

        var result = await _leaveRepository.GetManagerRequestsAsync(managerId, filter);
        return Ok(ApiResponse<PagedResult<LeaveRequest>>.Ok(result));
    }

    // PUT /api/leaves/{leaveRequestId}/approve — Manager approves
    [HttpPut("{leaveRequestId}/approve")]
    public async Task<IActionResult> ApproveLeave(string leaveRequestId, [FromBody] ApproveRejectRequest request)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        if (role != "Manager")
            return Forbid();

        var leave = await _leaveRepository.GetByIdAsync(leaveRequestId);
        if (leave == null)
            return NotFound(ApiResponse<object>.Fail("Leave request not found."));

        if (leave.ManagerId != managerId)
            return Forbid();

        // Deduct balance via EmployeeService before approving
        await _employeeServiceClient.DeductLeaveBalanceAsync(leave.EmployeeId, leave.LeaveType, leave.NumberOfDays);

        var approved = await _leaveRepository.ApproveLeaveAsync(leaveRequestId, request.Comments);

        _publisher.PublishLeaveApproved(new LeaveEventMessage(
            approved.LeaveRequestId, approved.EmployeeId, approved.EmployeeName,
            approved.ManagerId, approved.LeaveType, approved.StartDate, approved.EndDate,
            approved.NumberOfDays, approved.Reason, request.Comments, "LeaveApproved", DateTime.UtcNow));

        return Ok(ApiResponse<LeaveRequest>.Ok(approved, "Leave approved successfully."));
    }

    // PUT /api/leaves/{leaveRequestId}/reject — Manager rejects
    [HttpPut("{leaveRequestId}/reject")]
    public async Task<IActionResult> RejectLeave(string leaveRequestId, [FromBody] ApproveRejectRequest request)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        if (role != "Manager")
            return Forbid();

        var leave = await _leaveRepository.GetByIdAsync(leaveRequestId);
        if (leave == null)
            return NotFound(ApiResponse<object>.Fail("Leave request not found."));

        if (leave.ManagerId != managerId)
            return Forbid();

        var rejected = await _leaveRepository.RejectLeaveAsync(leaveRequestId, request.Comments);

        _publisher.PublishLeaveRejected(new LeaveEventMessage(
            rejected.LeaveRequestId, rejected.EmployeeId, rejected.EmployeeName,
            rejected.ManagerId, rejected.LeaveType, rejected.StartDate, rejected.EndDate,
            rejected.NumberOfDays, rejected.Reason, request.Comments, "LeaveRejected", DateTime.UtcNow));

        return Ok(ApiResponse<LeaveRequest>.Ok(rejected, "Leave rejected."));
    }

    // DELETE /api/leaves/{leaveRequestId}/cancel — Employee cancels own pending leave
    [HttpDelete("{leaveRequestId}/cancel")]
    public async Task<IActionResult> CancelLeave(string leaveRequestId)
    {
        var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        // Employee role is seeded as "Emp"
        if (role != "Emp")
            return Forbid();

        var leave = await _leaveRepository.GetByIdAsync(leaveRequestId);
        if (leave == null)
            return NotFound(ApiResponse<object>.Fail("Leave request not found."));

        if (leave.EmployeeId != employeeId)
            return Forbid();

        var cancelled = await _leaveRepository.CancelLeaveAsync(leaveRequestId);
        return Ok(ApiResponse<LeaveRequest>.Ok(cancelled, "Leave cancelled successfully."));
    }

    // GET /api/leaves/circuit-breaker/test — demo circuit breaker state via downstream call
    [HttpGet("circuit-breaker/test/{input}")]
     [AllowAnonymous]  // No auth needed for demo
    public async Task<ActionResult<object>> TestResiliencePatterns(string input)
    {
        var instanceId = Environment.MachineName;
        var startTime = DateTime.UtcNow;
        
        _logger.LogWarning("[DEMO] 🔹 Request received | Input: '{Input}' | Instance: {InstanceId}", 
            input, instanceId);

        try
        {
            // This call will be protected by Polly policies (if enabled)
            // Retry Policy: 3 retries with exponential backoff (2s, 4s, 8s)
            // Circuit Breaker: Opens after 5 failures, 30s duration
            var result = await _employeeServiceClient.CallDemoEndpointAsync(input);
            
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            _logger.LogInformation("[DEMO] ✅ SUCCESS | Duration: {Duration}ms | Instance: {InstanceId}", 
                (int)duration, instanceId);

            return Ok(new {
                status = "SUCCESS",
                instance = instanceId,
                userServiceResponse = result,
                durationMs = (int)duration,
                input = input
            });
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            _logger.LogError("[DEMO] ❌ FAILED | Duration: {Duration}ms | Error: {Error} | Instance: {InstanceId}", 
                (int)duration, ex.Message, instanceId);

            return StatusCode(503, new {
                status = "FAILED",
                instance = instanceId,
                error = ex.Message,
                durationMs = (int)duration,
                input = input
            });
        }
     }
}