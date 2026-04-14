using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Models;
using System.Linq;

namespace NZFTC_Portal.Controllers
{
    public class AdminLeaveController : Controller
    {
        private readonly AppDbContext _context;

        public AdminLeaveController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin"
                || HttpContext.Session.GetString("Role") == "Administrator";
        }

        private void SetAdminViewData(string activeTab)
        {
            var fullName = HttpContext.Session.GetString("FullName") ?? "Admin";
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var adminCode = _context.Admins
                .Where(a => a.UserId == userId)
                .Select(a => a.AdminCode)
                .FirstOrDefault() ?? $"ADM{userId}";

            ViewData["PortalUserName"] = $"{fullName} - {adminCode}";
            ViewData["PortalRole"] = "Admin";
            ViewData["ActiveTab"] = activeTab;
            ViewData["PortalNavItems"] = new[] { "Dashboard", "Leave", "Payroll", "Employees", "Cases" };
        }

        [HttpGet]
        public IActionResult Pending()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Leave");

            // Loads all leave requests for the admin review table.
            var allLeaveRequests = _context.LeaveRequests
                .Join(
                    _context.Employees,
                    request => request.EmployeeId,
                    employee => employee.UserId,
                    (request, employee) => new { request, employee })
                .Join(
                    _context.Users,
                    combined => combined.employee.UserId,
                    user => user.UserId,
                    (combined, user) => new
                    {
                        combined.request.LeaveRequestId,
                        combined.request.LeaveType,
                        combined.request.StartDate,
                        combined.request.EndDate,
                        combined.request.Reason,
                        combined.request.Status,
                        EmployeeName = user.FullName,
                        EmployeeCode = combined.employee.EmployeeCode
                    })
                .OrderByDescending(r => r.LeaveRequestId)
                .AsEnumerable()
                .Select(r => new[]
                {
                    $"LV-{r.LeaveRequestId:D3}",
                    r.EmployeeName,
                    r.EmployeeCode,
                    r.LeaveType,
                    $"{r.StartDate:dd/MM/yyyy} - {r.EndDate:dd/MM/yyyy}",
                    ((r.EndDate.DayNumber - r.StartDate.DayNumber) + 1).ToString(),
                    string.IsNullOrWhiteSpace(r.Reason) ? "--" : r.Reason,
                    r.Status,
                    r.LeaveRequestId.ToString()
                })
                .ToArray();

            ViewData["AdminLeaveRequests"] = allLeaveRequests;

            // Loads simple reporting counts for the admin leave report section.
            ViewData["TotalLeaveRequests"] = _context.LeaveRequests.Count().ToString();
            ViewData["ApprovedLeaveRequests"] = _context.LeaveRequests.Count(r => r.Status == "Approved").ToString();
            ViewData["PendingLeaveRequests"] = _context.LeaveRequests.Count(r => r.Status == "Pending").ToString();
            ViewData["RejectedLeaveRequests"] = _context.LeaveRequests.Count(r => r.Status == "Rejected").ToString();

            return View("~/Views/Admin/Leave.cshtml");
        }

        [HttpPost]
        public IActionResult ApproveLeave(int leaveRequestId)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var leaveRequest = _context.LeaveRequests.FirstOrDefault(r => r.LeaveRequestId == leaveRequestId);
            if (leaveRequest == null)
                return RedirectToAction("Pending");

            leaveRequest.Status = "Approved";
            leaveRequest.AdminId = HttpContext.Session.GetInt32("UserId");

            _context.SaveChanges();

            return RedirectToAction("Pending");
        }

        [HttpPost]
        public IActionResult RejectLeave(int leaveRequestId)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var leaveRequest = _context.LeaveRequests.FirstOrDefault(r => r.LeaveRequestId == leaveRequestId);
            if (leaveRequest == null)
                return RedirectToAction("Pending");

            leaveRequest.Status = "Rejected";
            leaveRequest.AdminId = HttpContext.Session.GetInt32("UserId");

            _context.SaveChanges();

            return RedirectToAction("Pending");
        }
    }
}