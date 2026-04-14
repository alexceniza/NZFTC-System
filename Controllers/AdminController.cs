using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NZFTC_Portal.Models;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Check if user is admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin"
                || HttpContext.Session.GetString("Role") == "Administrator";
        }

        // Reusable header setup
        private void SetAdminViewData(string activeTab)
        {
            var fullName = HttpContext.Session.GetString("FullName") ?? "Admin";
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            ViewData["PortalUserName"] = $"{fullName} - ADM{userId}";
            ViewData["PortalRole"] = "Admin";
            ViewData["ActiveTab"] = activeTab;
            ViewData["PortalNavItems"] = new[] { "Dashboard", "Leave", "Payroll", "Employees", "Cases" };
        }

        // Admin dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Dashboard");

            // Total pending leave approvals
            int pendingLeaveApprovals = await _context.LeaveRequests
                .CountAsync(lr => lr.Status == "Pending");

            // Total employees
            int employeeCount = await _context.Employees.CountAsync();

            // Total open grievances/cases
            int openCases = await _context.Cases
                .CountAsync(c => c.Status == "Pending" || c.Status == "Open" || c.Status == "In Progress");

            // Recent leave requests for dashboard preview
            var recentLeaveRequests = await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .ThenInclude(e => e.User)
                .OrderByDescending(lr => lr.LeaveRequestId)
                .Take(5)
                .Select(lr => new AdminRecentLeaveRequestViewModel
                {
                    LeaveRequestId = lr.LeaveRequestId,
                    EmployeeName = lr.Employee.User.FullName,
                    LeaveType = lr.LeaveType,
                    StartDate = lr.StartDate,
                    EndDate = lr.EndDate,
                    Status = lr.Status
                })
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                PendingLeaveApprovals = pendingLeaveApprovals,
                EmployeeCount = employeeCount,
                OpenCases = openCases,
                RecentLeaveRequests = recentLeaveRequests
            };

            return View(model);
        }
    }
    
}