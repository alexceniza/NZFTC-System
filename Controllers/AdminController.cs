using Microsoft.AspNetCore.Identity;
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

            // Loads the real admin code for the logged-in admin header label.
            var adminCode = _context.Admins
                .Where(a => a.UserId == userId)
                .Select(a => a.AdminCode)
                .FirstOrDefault() ?? $"ADM{userId}";

            ViewData["PortalUserName"] = $"{fullName} - {adminCode}";
            ViewData["PortalRole"] = "Admin";
            ViewData["ActiveTab"] = activeTab;
            ViewData["PortalNavItems"] = new[] { "Dashboard", "Leave", "Payroll", "Employees", "Cases" };
        }

        // Loads the staff directory rows for the admin employee table.
        private void LoadEmployeeDirectory()
        {
            // Loads employee accounts with their real employee table data.
            var employeeDirectory = _context.Employees
                .Join(
                    _context.Users,
                    employee => employee.UserId,
                    user => user.UserId,
                    (employee, user) => new
                    {
                        SortCode = employee.EmployeeCode,
                        DisplayCode = employee.EmployeeCode,
                        user.FullName,
                        user.Email,
                        employee.Department,
                        employee.Position,
                        JoinDate = employee.JoinDate.ToString("dd/MM/yyyy"),
                        employee.EmploymentStatus
                    })
                .AsEnumerable()
                .Select(row => new[]
                {
                    row.DisplayCode,
                    row.FullName,
                    row.Email,
                    row.Department,
                    row.Position,
                    row.JoinDate,
                    row.EmploymentStatus
                });

            // Loads admin accounts so they also appear in the same staff directory.
            var adminDirectory = _context.Admins
                .Join(
                    _context.Users,
                    admin => admin.UserId,
                    user => user.UserId,
                    (admin, user) => new
                    {
                        SortCode = admin.AdminCode,
                        DisplayCode = admin.AdminCode,
                        user.FullName,
                        user.Email,
                        Department = "Admin",
                        Position = "Administrator",
                        JoinDate = "--",
                        EmploymentStatus = "Active"
                    })
                .AsEnumerable()
                .Select(row => new[]
                {
                    row.DisplayCode,
                    row.FullName,
                    row.Email,
                    row.Department,
                    row.Position,
                    row.JoinDate,
                    row.EmploymentStatus
                });

            // Combines employees and admins into one staff directory for the page.
            var staffDirectory = employeeDirectory
                .Concat(adminDirectory)
                .OrderBy(row => row[0])
                .ToArray();

            ViewData["EmployeeDirectory"] = staffDirectory;
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

        // Admin leave management page
        public IActionResult Leave()
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

            return View();
        }

        // Admin leave approval
        [HttpPost]
        public IActionResult ApproveLeave(int leaveRequestId)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var leaveRequest = _context.LeaveRequests.FirstOrDefault(r => r.LeaveRequestId == leaveRequestId);
            if (leaveRequest == null)
                return RedirectToAction("Leave");

            leaveRequest.Status = "Approved";
            leaveRequest.AdminId = HttpContext.Session.GetInt32("UserId");

            _context.SaveChanges();

            return RedirectToAction("Leave");
        }

        // Admin leave rejection
        [HttpPost]
        public IActionResult RejectLeave(int leaveRequestId)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var leaveRequest = _context.LeaveRequests.FirstOrDefault(r => r.LeaveRequestId == leaveRequestId);
            if (leaveRequest == null)
                return RedirectToAction("Leave");

            leaveRequest.Status = "Rejected";
            leaveRequest.AdminId = HttpContext.Session.GetInt32("UserId");

            _context.SaveChanges();

            return RedirectToAction("Leave");
        }

        // Admin payroll management page
        public IActionResult Payroll()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Payroll");
            return View();
        }

        // Admin employee management page
        public IActionResult Employees()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Employees");
            LoadEmployeeDirectory();

            return View();
        }

        // Admin creates new employee or admin account
        [HttpPost]
        public IActionResult CreateEmployee(
            string employeeFullName,
            string employeeContact,
            string employeeEmergencyContact,
            string employeeEmail,
            string employeeRole,
            string employeeRoleId,
            string? employeeConfirmationCode,
            string employeeTraining,
            string employeeHistory,
            string employeeDepartment,
            string employeePosition)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Employees");
            LoadEmployeeDirectory();

            // Trims the incoming values before validation.
            employeeFullName = employeeFullName?.Trim() ?? "";
            employeeContact = employeeContact?.Trim() ?? "";
            employeeEmergencyContact = employeeEmergencyContact?.Trim() ?? "";
            employeeEmail = employeeEmail?.Trim() ?? "";
            employeeRole = employeeRole?.Trim() ?? "";
            employeeRoleId = employeeRoleId?.Trim() ?? "";
            employeeConfirmationCode = employeeConfirmationCode?.Trim() ?? "";
            employeeTraining = employeeTraining?.Trim() ?? "";
            employeeHistory = employeeHistory?.Trim() ?? "";
            employeeDepartment = employeeDepartment?.Trim() ?? "";
            employeePosition = employeePosition?.Trim() ?? "";

            // Basic required field validation.
            if (string.IsNullOrWhiteSpace(employeeFullName) ||
                string.IsNullOrWhiteSpace(employeeEmail) ||
                string.IsNullOrWhiteSpace(employeeRole) ||
                string.IsNullOrWhiteSpace(employeeRoleId))
            {
                TempData["Error"] = "Please complete all required account fields.";
                return View("Employees");
            }

            // Prevents duplicate email creation.
            if (_context.Users.Any(u => u.Email == employeeEmail))
            {
                TempData["Error"] = "That email address is already in use.";
                return View("Employees");
            }

            // Normalizes role values from the dropdown.
            var normalizedRole = employeeRole.Equals("admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "Employee";

            // Temporary password used for newly created accounts.
            const string tempPassword = "SD106-2";

            // Hashes the temporary password before saving to the database.
            var passwordHasher = new PasswordHasher<User>();

            // Creates the base user row first.
            var newUser = new User
            {
                FullName = employeeFullName,
                Email = employeeEmail,
                Role = normalizedRole,
                PasswordHash = string.Empty
            };

            newUser.PasswordHash = passwordHasher.HashPassword(newUser, tempPassword);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Admin account creation path.
            if (normalizedRole == "Admin")
            {
                // Requires the admin confirmation code.
                const string adminConfirmationCode = "NZFTCP0RTAL.";

                if (employeeConfirmationCode != adminConfirmationCode)
                {
                    _context.Users.Remove(newUser);
                    _context.SaveChanges();

                    TempData["Error"] = "Invalid admin confirmation code.";
                    return View("Employees");
                }

                if (_context.Admins.Any(a => a.AdminCode == employeeRoleId))
                {
                    _context.Users.Remove(newUser);
                    _context.SaveChanges();

                    TempData["Error"] = "That admin code is already in use.";
                    return View("Employees");
                }

                var newAdmin = new Admin
                {
                    UserId = newUser.UserId,
                    AdminCode = employeeRoleId
                };

                _context.Admins.Add(newAdmin);
                _context.SaveChanges();

                TempData["Success"] = $"Admin account created. Temporary password: {tempPassword}";
                return RedirectToAction("Employees");
            }

            // Employee account creation path.
            if (string.IsNullOrWhiteSpace(employeeContact) ||
                string.IsNullOrWhiteSpace(employeeEmergencyContact) ||
                string.IsNullOrWhiteSpace(employeeDepartment) ||
                string.IsNullOrWhiteSpace(employeePosition))
            {
                _context.Users.Remove(newUser);
                _context.SaveChanges();

                TempData["Error"] = "Please complete all required employee fields.";
                return View("Employees");
            }

            if (_context.Employees.Any(e => e.EmployeeCode == employeeRoleId))
            {
                _context.Users.Remove(newUser);
                _context.SaveChanges();

                TempData["Error"] = "That employee code is already in use.";
                return View("Employees");
            }

            var newEmployee = new Employee
            {
                UserId = newUser.UserId,
                EmployeeCode = employeeRoleId,
                Department = employeeDepartment,
                Position = employeePosition,
                JoinDate = DateOnly.FromDateTime(DateTime.Today),
                EmploymentStatus = "Active"
            };

            _context.Employees.Add(newEmployee);
            _context.SaveChanges();

            var newEmployeeRecord = new EmployeeRecord
            {
                EmployeeId = newUser.UserId,
                PhoneNumber = employeeContact,
                EmergencyContact = employeeEmergencyContact,
                EmploymentHistory = employeeHistory,
                TrainingRecord = employeeTraining,
                PerformanceEvaluation = null,
                Address = null
            };

            _context.EmployeeRecords.Add(newEmployeeRecord);
            _context.SaveChanges();

            TempData["Success"] = $"Employee account created. Temporary password: {tempPassword}";
            return RedirectToAction("Employees");
        }

        // Admin case management page
        public IActionResult Cases()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Cases");
            return View();
        }
    }
}