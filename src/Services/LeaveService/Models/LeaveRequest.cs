using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LeaveService.Models
{
    public class LeaveRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string LeaveRequestId { get; set; }=string.Empty;

        [Required]
        public string EmployeeId { get; set; }=string.Empty;

         [Required]
        public string EmployeeName { get; set; }=string.Empty;

         [Required]
        public string ManagerId { get; set; }=string.Empty;
        [Required]
        public string LeaveType { get; set; }=string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfDays { get; set; }
        
        [Required]
        public string Reason { get; set; }=string.Empty;

        [Required]
        public string Status { get; set; }=string.Empty; // e.g., "Pending", "Approved", "Rejected"
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

public static class LeaveRequestStatus
    {
        public const string Pending="Pending";
        public const string Approved="Approved";
        public const string Rejected="Rejected";
        public const string Cancelled="Cancelled";
    }
    public static class LeaveTypes
    {
        public const string Casual="Casual";
        public const string Sick="Sick";
        public const string Privilege="Privilege";
        
        public static bool IsValid(string leaveType)=>
        leaveType is Casual or Sick or Privilege;
    }
}