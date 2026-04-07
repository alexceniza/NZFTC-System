using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Models;

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
        public IActionResult Dashboard()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Dashboard");
            return View();
        }

        // Admin leave management page
        public IActionResult Leave()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Leave");
            return View();
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

            // Loads employee and user rows first, then shapes them for the table in memory.
            var employeeDirectory = _context.Employees
                .Join(
                    _context.Users,
                    employee => employee.UserId,
                    user => user.UserId,
                    (employee, user) => new
                    {
                        employee.EmployeeCode,
                        user.FullName,
                        user.Email,
                        employee.Department,
                        employee.Position,
                        employee.JoinDate,
                        employee.EmploymentStatus
                    })
                .OrderBy(row => row.EmployeeCode)
                .AsEnumerable()
                .Select(row => new[]
                {
            row.EmployeeCode,
            row.FullName,
            row.Email,
            row.Department,
            row.Position,
            row.JoinDate.ToString("dd/MM/yyyy"),
            row.EmploymentStatus
                })
                .ToArray();

            // Sends the employee directory rows to the Employees.cshtml table.
            ViewData["EmployeeDirectory"] = employeeDirectory;

            return View();
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