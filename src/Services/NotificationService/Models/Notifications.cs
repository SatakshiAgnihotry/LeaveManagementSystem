using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models;

public class Notification
{
    [Key]
    public string NotificationId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}

public static class NotificationType
{
    public const string LeaveApplied = "LeaveApplied";
    public const string LeaveApproved = "LeaveApproved";
    public const string LeaveRejected = "LeaveRejected";
}