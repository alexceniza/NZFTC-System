using Microsoft.EntityFrameworkCore;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.Models;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _context;

        public LeaveService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SubmitLeaveRequestAsync(int employeeId, LeaveRequestCreateViewModel model)
        {
            if (model.EndDate.Date < model.StartDate.Date)
            {
                throw new ArgumentException("End date cannot be earlier than start date.");
            }

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employeeId,
                LeaveType = model.LeaveType,
                StartDate = DateOnly.FromDateTime(model.StartDate),
                EndDate = DateOnly.FromDateTime(model.EndDate),
                Reason = model.Reason,
                Status = "Pending",
                AdminId = null
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LeaveRequest>> GetEmployeeLeaveRequestsAsync(int employeeId)
        {
            return await _context.LeaveRequests
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetPendingLeaveRequestsAsync()
        {
            return await _context.LeaveRequests
                .Include(x => x.Employee)
                    .ThenInclude(e => e.User)
                .Where(x => x.Status == "Pending")
                .OrderBy(x => x.StartDate)
                .ToListAsync();
        }

        public async Task<bool> ApproveOrDeclineAsync(int leaveRequestId, int adminId, string decision)
        {
            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId);

            if (leaveRequest == null)
                return false;

            if (leaveRequest.Status != "Pending")
                return false;

            if (decision != "Approved" && decision != "Declined")
                return false;

            leaveRequest.Status = decision;
            leaveRequest.AdminId = adminId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AdminLeaveReportFilterViewModel> GetAdminLeaveReportAsync(AdminLeaveReportFilterViewModel filter)
        {
            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                    .ThenInclude(e => e.User)
                .Include(l => l.Admin)
                    .ThenInclude(a => a.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(x => x.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.LeaveType))
            {
                query = query.Where(x => x.LeaveType == filter.LeaveType);
            }

            if (!string.IsNullOrWhiteSpace(filter.EmployeeName))
            {
                query = query.Where(x => x.Employee.User.FullName.Contains(filter.EmployeeName));
            }

            if (filter.StartDate.HasValue)
            {
                var start = DateOnly.FromDateTime(filter.StartDate.Value);
                query = query.Where(x => x.StartDate >= start);
            }

            if (filter.EndDate.HasValue)
            {
                var end = DateOnly.FromDateTime(filter.EndDate.Value);
                query = query.Where(x => x.EndDate <= end);
            }

            var data = await query
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();

            filter.Results = data.Select(x => new AdminLeaveReportRowViewModel
            {
                LeaveRequestId = x.LeaveRequestId,
                EmployeeName = x.Employee.User.FullName,
                EmployeeCode = x.Employee.EmployeeCode,
                Department = x.Employee.Department,
                LeaveType = x.LeaveType,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Reason = x.Reason,
                Status = x.Status,
                ReviewedByAdmin = x.Admin != null ? x.Admin.User.FullName : null,
                TotalDays = x.EndDate.DayNumber - x.StartDate.DayNumber + 1
            }).ToList();

            filter.TotalRequests = data.Count;
            filter.PendingCount = data.Count(x => x.Status == "Pending");
            filter.ApprovedCount = data.Count(x => x.Status == "Approved");
            filter.DeclinedCount = data.Count(x => x.Status == "Declined");

            return filter;
        }
    }
}