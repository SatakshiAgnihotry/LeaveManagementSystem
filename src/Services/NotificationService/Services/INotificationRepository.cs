using NotificationService.Models;
namespace NotificationService.Services
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(Notification notification);
        Task<IEnumerable<Notification>> GetNotificationsByUserAsync(string userId);
        Task<Notification> MarkAsReadAsync(string notificationId, string userId);
    }
}