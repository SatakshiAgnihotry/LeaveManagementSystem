using LeaveService.Models;

namespace LeaveService.Services;

public interface ILeaveRepository
{
    // Save a new leave request and return the saved entity
    Task<LeaveRequest> ApplyLeaveAsync(LeaveRequest request);

    // Get a single leave request by ID — null if not found
    Task<LeaveRequest?> GetByIdAsync(string leaveRequestId);

    // Get employee's own leave history with optional status filter and pagination
    Task<PagedResult<LeaveRequest>> GetLeaveHistoryAsync(string employeeId, LeaveFilterRequest filter);

    // Get all requests under a manager with full filtering (status, employee, date range) and pagination
    Task<PagedResult<LeaveRequest>> GetManagerRequestsAsync(string managerId, LeaveFilterRequest filter);

    // Check if employee already has an approved/pending leave overlapping the requested dates
    Task<bool> HasOverlappingLeaveAsync(string employeeId, DateTime start, DateTime end);

    // Manager approves a pending request — returns updated request
    Task<LeaveRequest> ApproveLeaveAsync(string leaveRequestId, string? comments);

    // Manager rejects a pending request — returns updated request
    Task<LeaveRequest> RejectLeaveAsync(string leaveRequestId, string? comments);

    // Employee cancels their own pending request — returns updated request
    Task<LeaveRequest> CancelLeaveAsync(string leaveRequestId);
}