using System.Net.Http.Json;
using System.Text.Json;
using LeaveService.Middleware;

namespace LeaveService.Services
{
    public class EmployeeServiceClient : IEmployeeServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmployeeServiceClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmployeeServiceClient(HttpClient httpClient, ILogger<EmployeeServiceClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasSufficientLeaveBalanceAsync(string employeeId, string leaveType, int days)
        {
            var url = $"/api/employees/{employeeId}/leave-balances/{leaveType}/check?days={days}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Balance check failed for employee {EmployeeId}. Status Code: {StatusCode}", employeeId, response.StatusCode);
                return false;
            }
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
            .GetProperty("data")
            .GetProperty("hasSufficientBalance")
            .GetBoolean();

        }

        public async Task DeductLeaveBalanceAsync(string employeeId, string leaveType, int days)
        {
            var url=$"/api/employees/{employeeId}/leave-balances/deduct";
            var body= JsonContent.Create(new { leaveType, days });
            var response = await _httpClient.PutAsync(url, body);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to deduct leave balance for employee {EmployeeId}. Status Code: {StatusCode}", employeeId, response.StatusCode);
                throw new BadRequestException("Failed to deduct leave balance");
            }
        }

    public async Task<string> CallDemoEndpointAsync(string input)
    {
        try
        {
            SetCorrelationIdHeader();

            var response = await _httpClient.GetAsync($"/api/employees/demo/{input}");
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return $"SUCCESS: {content}";
            }

            // Throw exception so Polly can retry
            throw new HttpRequestException($"EmployeeService returned {response.StatusCode}");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw for Polly to handle
        }
        catch (TaskCanceledException ex)
        {
            throw new HttpRequestException("Request timed out", ex);
        }
      }    
          private void SetCorrelationIdHeader()
    {
        var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-ID"].ToString();
        if (!string.IsNullOrEmpty(correlationId))
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Correlation-ID", correlationId);
        }
    }      
    }

}