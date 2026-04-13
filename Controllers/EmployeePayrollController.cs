using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;

namespace NZFTC_Portal.Controllers
{
    public class EmployeePayrollController : Controller
    {
        private readonly IPayrollService _payrollService;

        public EmployeePayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
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
        public async Task<IActionResult> Index()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            SetEmployeeViewData("Payroll");

            int employeeId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (employeeId == 0)
                return RedirectToAction("Login", "Account");

            var payrolls = await _payrollService.GetEmployeePayrollRecordsAsync(employeeId);

            return View("~/Views/Employee/Payroll.cshtml", payrolls);
        }
    }
}