using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NZFTC_Portal.Models;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Controllers
{
    public class AdminEmployeeController : Controller
    {
        private readonly AppDbContext _context;

        public AdminEmployeeController(AppDbContext context)
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

            ViewData["PortalUserName"] = $"{fullName} - ADM{userId}";
            ViewData["PortalRole"] = "Admin";
            ViewData["ActiveTab"] = activeTab;
            ViewData["PortalNavItems"] = new[] { "Dashboard", "Leave", "Payroll", "Employees", "Cases" };
        }

        // =========================
        // GET: Employee List
        // =========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Employees");

            var employees = await _context.Employees
                .Include(e => e.User)
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new AdminEmployeeViewModel
                {
                    EmployeeId = e.UserId,
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.User.FullName,
                    Email = e.User.Email,
                    Department = e.Department,
                    Position = e.Position,
                    JoinDate = e.JoinDate,
                    EmploymentStatus = e.EmploymentStatus
                })
                .ToListAsync();

            return View("~/Views/Admin/Employees.cshtml", employees);
        }

        // =========================
        // POST: Create Employee
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminEmployeeCreateViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Employees");

            // Validation
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please complete all required employee fields.";
                return RedirectToAction(nameof(Index));
            }

            // Check duplicates
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                TempData["ErrorMessage"] = "Email already exists.";
                return RedirectToAction(nameof(Index));
            }

            bool employeeCodeExists = await _context.Employees.AnyAsync(e => e.EmployeeCode == model.EmployeeCode);
            if (employeeCodeExists)
            {
                TempData["ErrorMessage"] = "Employee code already exists.";
                return RedirectToAction(nameof(Index));
            }

            // =========================
            // Create USER
            // =========================
            var passwordHasher = new PasswordHasher<User>();

            var newUser = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = model.Role
            };

            newUser.PasswordHash = passwordHasher.HashPassword(newUser, "SD106-2");

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // =========================
            // Create EMPLOYEE
            // =========================
            var newEmployee = new Employee
            {
                UserId = newUser.UserId,
                EmployeeCode = model.EmployeeCode,
                Department = model.Department ?? string.Empty,
                Position = model.Position ?? string.Empty,
                JoinDate = DateOnly.FromDateTime(DateTime.Now),
                EmploymentStatus = "Active"
            };

            _context.Employees.Add(newEmployee);

            // =========================
            // Create EMPLOYEE RECORD
            // =========================
            var newEmployeeRecord = new EmployeeRecord
            {
                EmployeeId = newUser.UserId,
                PhoneNumber = model.ContactNumber,
                EmergencyContact = model.EmergencyContact,
                Address = model.EmploymentHistory ?? string.Empty,
                TrainingRecord = model.TrainingRecord ?? string.Empty,
                PerformanceEvaluation = model.ConfirmationCode ?? string.Empty
            };

            _context.EmployeeRecords.Add(newEmployeeRecord);

            // Save all
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee added successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}