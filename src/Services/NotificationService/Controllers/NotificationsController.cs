using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationRepository _repository;

    public NotificationController(INotificationRepository repository)
    {
        _repository = repository;
    }

    // GET /api/notifications — get caller's own notifications
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<object>.Fail("Invalid token."));

        var notifications = await _repository.GetNotificationsByUserAsync(userId);
        return Ok(ApiResponse<IEnumerable<Notification>>.Ok(notifications));
    }

    // PUT /api/notifications/{id}/read — mark one notification as read
    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<object>.Fail("Invalid token."));

        var notification = await _repository.MarkAsReadAsync(notificationId, userId);
        if (notification == null)
            return NotFound(ApiResponse<object>.Fail("Notification not found or does not belong to you."));

        return Ok(ApiResponse<Notification>.Ok(notification, "Marked as read."));
    }
}