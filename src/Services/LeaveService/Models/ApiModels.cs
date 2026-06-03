using System.ComponentModel.DataAnnotations;
namespace LeaveService.Models
{
    public class ApplyLeaveRequest
    {
        [Required]
        public string LeaveType { get; set; }=string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Range(1,365)]
        public int NumberOfDays { get; set; }        
        [Required]
        public string Reason { get; set; }=string.Empty;
        [Required]
        public string ManagerId { get; set; }=string.Empty;
    }
    public class ApproveRejectRequest
    {
        public string? Comments { get; set; }
    }
    public class LeaveFilterRequest
    {
        public string? Status { get; set; }
        public string? EmployeeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page{ get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }=string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public static ApiResponse<T> Ok(T data, string message = null) =>
            new ApiResponse<T> { Success = true, Data = data, Message = message ?? string.Empty };
        public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
            new ApiResponse<T> { Success = false, Message = message, Errors = errors };
    }
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
