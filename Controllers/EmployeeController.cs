using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Models;
using System.Linq;

namespace NZFTC_Portal.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // Check if logged-in user is employee
        private bool IsEmployee()
        {
            return HttpContext.Session.GetString("Role") == "Employee";
        }

        // Reusable header setup
        private void SetEmployeeViewData(string activeTab)
        {
            var fullName = HttpContext.Session.GetString("FullName") ?? "Employee";
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            ViewData["PortalUserName"] = $"{fullName} - EMP{userId}";
            ViewData["PortalRole"] = "Employee";
            ViewData["ActiveTab"] = activeTab;
            ViewData["PortalNavItems"] = new[] { "Dashboard", "Leave", "Payroll", "My Info" };
        }

        // Optional test page
        public IActionResult Index()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            var employees = _context.Users.ToList();
            return View(employees);
        }

        // Employee dashboard
        public IActionResult Dashboard()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            SetEmployeeViewData("Dashboard");

            // Gets all holidays in date order for dashboard display.
            var holidays = _context.Holidays
                .OrderBy(h => h.HolidayDate)
                .ToList();

            // Sends the first 3 holidays to the dashboard preview section.
            ViewData["HolidayPreview"] = holidays
                .Take(3)
                .Select(h => new[]
                {
                    h.HolidayName,
                    h.HolidayDate.ToString("dd/MM/yyyy")
                })
                .ToArray();

            // Sends the full holiday list to the View All modal.
            ViewData["HolidayFullList"] = holidays
                .Select(h => new[]
                {
                    h.HolidayName,
                    h.HolidayDate.ToString("dd/MM/yyyy")
                })
                .ToArray();

            return View();
        }

        // Employee leave page
        public IActionResult Leave()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            SetEmployeeViewData("Leave");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Loads the logged-in employee leave history.
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

            return View();
        }

        // Employee leave submission
        [HttpPost]
        public IActionResult SubmitLeave(string leaveType, DateOnly startDate, DateOnly endDate, string leaveReason)
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Checks that the logged-in employee exists before creating the leave request.
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee == null)
            {
                TempData["LeaveError"] = "Employee account not found.";
                return RedirectToAction("Leave");
            }

            // Basic validation for leave submission.
            if (string.IsNullOrWhiteSpace(leaveType))
            {
                TempData["LeaveError"] = "Please select a leave type.";
                return RedirectToAction("Leave");
            }

            if (endDate < startDate)
            {
                TempData["LeaveError"] = "End date cannot be earlier than start date.";
                return RedirectToAction("Leave");
            }

            var cleanedLeaveType = leaveType.Trim();
            var cleanedLeaveReason = string.IsNullOrWhiteSpace(leaveReason) ? null : leaveReason.Trim();

            // Prevents the exact same leave request from being submitted twice.
            var duplicateLeaveRequest = _context.LeaveRequests.Any(r =>
                r.EmployeeId == employee.UserId &&
                r.LeaveType == cleanedLeaveType &&
                r.StartDate == startDate &&
                r.EndDate == endDate &&
                r.Status != "Rejected");

            if (duplicateLeaveRequest)
            {
                TempData["LeaveError"] = "This leave request has already been submitted.";
                return RedirectToAction("Leave");
            }

            // Prevents overlapping leave dates against existing pending or approved requests.
            var overlappingLeaveRequest = _context.LeaveRequests.Any(r =>
                r.EmployeeId == employee.UserId &&
                r.Status != "Rejected" &&
                startDate <= r.EndDate &&
                endDate >= r.StartDate);

            if (overlappingLeaveRequest)
            {
                TempData["LeaveError"] = "These dates overlap with an existing leave request.";
                return RedirectToAction("Leave");
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
            return RedirectToAction("Leave");
        }

        // Employee payroll page
        public IActionResult Payroll()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            SetEmployeeViewData("Payroll");
            return View();
        }

        // Employee information page
        public IActionResult MyInfo()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            SetEmployeeViewData("My Info");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Gets the employee row linked to the logged-in user.
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee == null)
                return NotFound();

            // Gets the personal record row linked to the employee.
            var employeeRecord = _context.EmployeeRecords.FirstOrDefault(r => r.EmployeeId == employee.UserId);
            if (employeeRecord == null)
                return NotFound();

            // Gets the base user row so the page can show full name and email.
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            // Populates the exact ViewData keys expected by MyInfo.cshtml.
            ViewData["EmployeeId"] = employee.EmployeeCode;
            ViewData["EmployeeFullName"] = user.FullName;
            ViewData["EmployeeContactNumber"] = employeeRecord.PhoneNumber;
            ViewData["EmployeeEmergencyContact"] = employeeRecord.EmergencyContact;
            ViewData["EmployeeEmailAddress"] = user.Email;
            ViewData["EmployeeDepartment"] = employee.Department;
            ViewData["EmployeePosition"] = employee.Position;
            ViewData["EmployeeJoinDate"] = employee.JoinDate.ToString("dd/MM/yyyy");
            ViewData["EmployeeStatus"] = employee.EmploymentStatus;

            return View(employeeRecord);
        }

        // Employee self-update
        [HttpPost]
        public IActionResult UpdateProfile(string updateField, string contactNumber, string emergencyContact)
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Gets the employee row linked to the logged-in user.
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction("MyInfo");
            }

            // Gets the employee record row that stores editable profile details.
            var employeeRecord = _context.EmployeeRecords.FirstOrDefault(r => r.EmployeeId == employee.UserId);
            if (employeeRecord == null)
            {
                TempData["Error"] = "Employee record not found.";
                return RedirectToAction("MyInfo");
            }

            // Updates only the field that the user actually clicked Save on.
            if (updateField == "contactNumber")
            {
                employeeRecord.PhoneNumber = contactNumber;
            }
            else if (updateField == "emergencyContact")
            {
                employeeRecord.EmergencyContact = emergencyContact;
            }

            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("MyInfo");
        }

        // Holiday list
        public IActionResult Holidays()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            SetEmployeeViewData("Holidays");

            var holidays = _context.Holidays
                .OrderBy(h => h.HolidayDate)
                .ToList();

            return View(holidays);
        }

        // Employee support page
        public IActionResult Support()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            SetEmployeeViewData("Support");
            return View();
        }
    }
}