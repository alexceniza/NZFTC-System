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