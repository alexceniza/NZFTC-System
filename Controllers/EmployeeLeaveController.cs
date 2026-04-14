using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Models;
using System.Linq;

namespace NZFTC_Portal.Controllers
{
    public class EmployeeLeaveController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeLeaveController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsEmployee()
        {
            return HttpContext.Session.GetString("Role") == "Employee";
        }

        private void SetEmployeeViewData(string activeTab)
        {
            var fullName = HttpContext.Session.GetString("FullName") ?? "Employee";
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            ViewData["PortalUserName"] = $"{fullName} - EMP{userId}";
            ViewData["PortalRole"] = "Employee";
            ViewData["ActiveTab"] = activeTab;
            ViewData["PortalNavItems"] = new[] { "Dashboard", "Leave", "Payroll", "My Info" };
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            SetEmployeeViewData("Leave");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            //Loads the logged-in employee leave history.
            var employeeLeaveRequests = _context.LeaveRequests
                .Where(r => r.EmployeeId == userId)
                .OrderByDescending(r => r.LeaveRequestId)
                .AsEnumerable()
                .Select(r => new[]
                {
                    $"LV-{r.LeaveRequestId:D3}",
                    r.LeaveType,
                    r.StartDate.ToString("dd/MM/yyyy"),
                    r.EndDate.ToString("dd/MM/yyyy"),
                    ((r.EndDate.DayNumber - r.StartDate.DayNumber) + 1).ToString(),
                    r.Status,
                    string.IsNullOrWhiteSpace(r.Reason) ? "--" : r.Reason
                })
                .ToArray();

            ViewData["EmployeeLeaveRequests"] = employeeLeaveRequests;

            return View("~/Views/Employee/Leave.cshtml");
        }

        [HttpPost]
        public IActionResult SubmitLeave(string leaveType, DateOnly startDate, DateOnly endDate, string leaveReason)
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            //Checks that the logged-in employee exists before creating the leave request.
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee == null)
            {
                TempData["LeaveError"] = "Employee account not found.";
                return RedirectToAction("Index");
            }

            //Basic validation for leave submission.
            if (string.IsNullOrWhiteSpace(leaveType))
            {
                TempData["LeaveError"] = "Please select a leave type.";
                return RedirectToAction("Index");
            }

            if (endDate < startDate)
            {
                TempData["LeaveError"] = "End date cannot be earlier than start date.";
                return RedirectToAction("Index");
            }

            var cleanedLeaveType = leaveType.Trim();
            var cleanedLeaveReason = string.IsNullOrWhiteSpace(leaveReason) ? null : leaveReason.Trim();

            //Prevents the exact same leave request from being submitted twice.
            var duplicateLeaveRequest = _context.LeaveRequests.Any(r =>
                r.EmployeeId == employee.UserId &&
                r.LeaveType == cleanedLeaveType &&
                r.StartDate == startDate &&
                r.EndDate == endDate &&
                r.Status != "Rejected");

            if (duplicateLeaveRequest)
            {
                TempData["LeaveError"] = "This leave request has already been submitted.";
                return RedirectToAction("Index");
            }

            //Prevents overlapping leave dates against existing pending or approved requests.
            var overlappingLeaveRequest = _context.LeaveRequests.Any(r =>
                r.EmployeeId == employee.UserId &&
                r.Status != "Rejected" &&
                startDate <= r.EndDate &&
                endDate >= r.StartDate);

            if (overlappingLeaveRequest)
            {
                TempData["LeaveError"] = "These dates overlap with an existing leave request.";
                return RedirectToAction("Index");
            }

            var newLeaveRequest = new LeaveRequest
            {
                EmployeeId = employee.UserId,
                AdminId = null,
                LeaveType = cleanedLeaveType,
                StartDate = startDate,
                EndDate = endDate,
                Reason = cleanedLeaveReason,
                Status = "Pending"
            };

            _context.LeaveRequests.Add(newLeaveRequest);
            _context.SaveChanges();

            TempData["LeaveSuccess"] = "Leave request submitted successfully.";
            return RedirectToAction("Index");
        }
    }
}