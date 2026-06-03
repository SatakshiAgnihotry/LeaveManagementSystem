namespace LeaveService.Services
{
    public interface IRabbitMQPublisher
    {
        void PublishLeaveApplied(LeaveEventMessage message);
        void PublishLeaveApproved(LeaveEventMessage message);
        void PublishLeaveRejected(LeaveEventMessage message);
    }
    public record LeaveEventMessage(
        string LeaveRequestId,
        string EmployeeId,
        string EmployeeName,
        string ManagerId,
        string LeaveType,
        DateTime StartDate,
        DateTime EndDate,
        int NumberOfDays,
        string Reason,
        string? Comments,
        string EventType,
        DateTime OccurredAt
    );
}