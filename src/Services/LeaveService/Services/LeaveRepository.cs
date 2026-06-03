using LeaveService.Data;
using LeaveService.Middleware;
using LeaveService.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Services;

public class LeaveRepository : ILeaveRepository
{
    private readonly AppDbContext _context;

    public LeaveRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveRequest> ApplyLeaveAsync(LeaveRequest request)
    {
        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<LeaveRequest?> GetByIdAsync(string leaveRequestId)
    {
        return await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.LeaveRequestId == leaveRequestId);
    }

    public async Task<PagedResult<LeaveRequest>> GetLeaveHistoryAsync(string employeeId, LeaveFilterRequest filter)
    {
        // Start with all requests for this employee
        var query = _context.LeaveRequests
            .Where(lr => lr.EmployeeId == employeeId);

        // Optionally filter by status (e.g. only show Pending)
        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(lr => lr.Status == filter.Status);

        // Optionally filter by date range
        if (filter.FromDate.HasValue)
            query = query.Where(lr => lr.StartDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(lr => lr.EndDate <= filter.ToDate.Value);

        // Count total BEFORE pagination (needed for TotalPages calculation)
        var totalCount = await query.CountAsync();

        // Apply pagination — skip past previous pages, take only current page
        var items = await query
            .OrderByDescending(lr => lr.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<LeaveRequest>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<LeaveRequest>> GetManagerRequestsAsync(string managerId, LeaveFilterRequest filter)
    {
        // Start with all requests under this manager
        var query = _context.LeaveRequests
            .Where(lr => lr.ManagerId == managerId);

        // Filter by specific employee if provided
        if (!string.IsNullOrWhiteSpace(filter.EmployeeId))
            query = query.Where(lr => lr.EmployeeId == filter.EmployeeId);

        // Filter by status
        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(lr => lr.Status == filter.Status);

        // Filter by date range
        if (filter.FromDate.HasValue)
            query = query.Where(lr => lr.StartDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(lr => lr.EndDate <= filter.ToDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(lr => lr.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<LeaveRequest>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<bool> HasOverlappingLeaveAsync(string employeeId, DateTime start, DateTime end)
    {
        // Overlap condition:
        // Existing leave starts BEFORE the new leave ends
        // AND existing leave ends AFTER the new leave starts
        // Only check Pending or Approved — Rejected/Cancelled don't block new requests
        return await _context.LeaveRequests.AnyAsync(lr =>
            lr.EmployeeId == employeeId &&
            (lr.Status == LeaveRequestStatus.Pending || lr.Status == LeaveRequestStatus.Approved) &&
            lr.StartDate < end &&
            lr.EndDate > start);
    }

    public async Task<LeaveRequest> ApproveLeaveAsync(string leaveRequestId, string? comments)
    {
        var leave = await GetByIdAsync(leaveRequestId);
        if (leave == null)
            throw new NotFoundException($"Leave request {leaveRequestId} not found.");

        if (leave.Status != LeaveRequestStatus.Pending)
            throw new BadRequestException($"Only Pending requests can be approved. Current status: {leave.Status}");

        // EF Core tracks this object — just update properties and save
        leave.Status = LeaveRequestStatus.Approved;
        leave.Comments = comments;
        leave.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return leave;
    }

    public async Task<LeaveRequest> RejectLeaveAsync(string leaveRequestId, string? comments)
    {
        var leave = await GetByIdAsync(leaveRequestId);
        if (leave == null)
            throw new NotFoundException($"Leave request {leaveRequestId} not found.");

        if (leave.Status != LeaveRequestStatus.Pending)
            throw new BadRequestException($"Only Pending requests can be rejected. Current status: {leave.Status}");

        leave.Status = LeaveRequestStatus.Rejected;
        leave.Comments = comments;
        leave.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return leave;
    }

    public async Task<LeaveRequest> CancelLeaveAsync(string leaveRequestId)
    {
        var leave = await GetByIdAsync(leaveRequestId);
        if (leave == null)
            throw new NotFoundException($"Leave request {leaveRequestId} not found.");

        if (leave.Status != LeaveRequestStatus.Pending)
            throw new BadRequestException("Only Pending requests can be cancelled.");

        leave.Status = LeaveRequestStatus.Cancelled;
        leave.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return leave;
    }
}