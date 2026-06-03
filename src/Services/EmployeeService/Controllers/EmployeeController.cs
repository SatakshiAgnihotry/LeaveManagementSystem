using System.Security.Claims;
using EmployeeService.Models;
using EmployeeService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeRepository employeeRepository, ILogger<EmployeeController> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetEmployeeById(string employeeId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(currentUserId) || string.IsNullOrWhiteSpace(role))
            return Unauthorized(ApiResponse<object>.Fail("Invalid token claims."));

        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
        if (employee == null)
            return NotFound(ApiResponse<object>.Fail("Employee not found."));

        if (role == "Employee" && employee.EmployeeId != currentUserId)
            return Forbid();

        if (role == "Manager" && employee.EmployeeId != currentUserId && employee.ManagerId != currentUserId)
            return Forbid();

        return Ok(ApiResponse<Employee>.Ok(employee));
    }

    [HttpGet("team")]
    public async Task<IActionResult> GetMyTeam()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(currentUserId) || string.IsNullOrWhiteSpace(role))
            return Unauthorized(ApiResponse<object>.Fail("Invalid token claims."));

        if (role != "Manager")
            return Forbid();

        var team = await _employeeRepository.GetEmployeesByManagerAsync(currentUserId);
        return Ok(ApiResponse<IEnumerable<Employee>>.Ok(team));
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(currentUserId) || string.IsNullOrWhiteSpace(role))
            return Unauthorized(ApiResponse<object>.Fail("Invalid token claims."));

        if (role != "Manager")
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(ApiResponse<object>.Fail("Name and Email are required."));

        // Check if email already exists
        var emailExists = await _employeeRepository.EmailExistsAsync(request.Email);
        if (emailExists)
            return Conflict(ApiResponse<object>.Fail("An employee with this email already exists."));

        var nextEmployeeId = await _employeeRepository.GetNextEmployeeIdAsync();

        var employee = new Employee
        {
            EmployeeId = nextEmployeeId,
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Role = "Emp",
            ManagerId = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        // CreateEmployeeAsync automatically seeds leave balances (Casual, Sick, Privilege)
        var created = await _employeeRepository.CreateEmployeeAsync(employee);

        return CreatedAtAction(
            nameof(GetEmployeeById),
            new { employeeId = created.EmployeeId },
            ApiResponse<Employee>.Ok(created, "Employee created successfully with auto-assigned leave balances."));
    }

    [HttpGet("demo/{input}")]
    [AllowAnonymous]  // No auth needed for demo
    public async Task<ActionResult<object>> DemoResilienceEndpoint(string input)
    {
        var instanceId = Environment.MachineName;
        _logger.LogWarning("[Instance: {InstanceId}] DEMO endpoint called with input: {Input}", instanceId, input);

        // Simulate different scenarios based on input
        switch (input.ToLower())
        {
            case "error":
                // Simulate transient server error (500)
                // This will trigger Polly retry policy in OrderService
                _logger.LogError("[Instance: {InstanceId}] DEMO: Simulating 500 Internal Server Error", instanceId);
                return StatusCode(500, new { 
                    message = "Simulated transient error - Polly will retry this!",
                    instance = instanceId,
                    timestamp = DateTime.UtcNow
                });

            default:
                // Normal successful response
                _logger.LogInformation("[Instance: {InstanceId}] DEMO: Returning success response", instanceId);
                return Ok(new { 
                    message = $"Success! Input was: {input}",
                    instance = instanceId,
                    timestamp = DateTime.UtcNow
                });
        }
    }
        
}