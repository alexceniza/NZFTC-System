using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.Models;
using NZFTC_Portal.ViewModels;
using System.Linq;

namespace NZFTC_Portal.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILeaveService _leaveService;
        private readonly IPayrollService _payrollService;
        private readonly ICaseService _caseService;

        public EmployeeController(
            AppDbContext context,
            ILeaveService leaveService,
            IPayrollService payrollService,
            ICaseService caseService)
        {
            _context = context;
            _leaveService = leaveService;
            _payrollService = payrollService;
            _caseService = caseService;
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
        public async Task<IActionResult> Dashboard()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            SetEmployeeViewData("Dashboard");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            int employeeId = userId.Value;

            var leaveBalance = await _leaveService.GetEmployeeLeaveBalanceAsync(employeeId);
            var latestPayslip = await _payrollService.GetLatestPayslipAsync(employeeId);

            var today = DateOnly.FromDateTime(DateTime.Now);

            var upcomingHolidays = await _context.Holidays
                .Where(h => h.HolidayDate >= today)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            var model = new EmployeeDashboardViewModel
            {
                AnnualLeave = leaveBalance.LeaveBalances
                    .FirstOrDefault(x => x.LeaveType == "Annual Leave"),

                SickLeave = leaveBalance.LeaveBalances
                    .FirstOrDefault(x => x.LeaveType == "Sick Leave"),

                PersonalLeave = leaveBalance.LeaveBalances
                    .FirstOrDefault(x => x.LeaveType == "Personal Leave"),

                LatestPayslip = latestPayslip,

                HolidayPreview = upcomingHolidays
                    .Take(3)
                    .ToList(),

                HolidayFullList = upcomingHolidays
            };

            return View(model);
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
        public async Task<IActionResult> MyInfo()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            SetEmployeeViewData("My Info");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employee == null)
                return NotFound();

            var employeeRecord = await _context.EmployeeRecords
                .FirstOrDefaultAsync(r => r.EmployeeId == employee.UserId);

            if (employeeRecord == null)
                return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            var employeeCases = await _caseService.GetEmployeeCasesAsync(userId);

            var model = new EmployeeMyInfoViewModel
            {
                EmployeeId = employee.EmployeeCode,
                FullName = user.FullName,
                ContactNumber = employeeRecord.PhoneNumber,
                EmergencyContact = employeeRecord.EmergencyContact,
                EmailAddress = user.Email,
                Department = employee.Department,
                Position = employee.Position,
                JoinDate = employee.JoinDate.ToString("dd/MM/yyyy"),
                Status = employee.EmploymentStatus,
                Cases = employeeCases.ToList()
            };

            return View(model);
        }

        // Employee self-update
        [HttpPost]
        [ValidateAntiForgeryToken]
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