using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Controllers
{
    public class AdminPayrollController : Controller
    {
        private readonly IPayrollService _payrollService;

        public AdminPayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Payroll");

            var payrolls = await _payrollService.GetAllPayrollRecordsAsync();
            return View("~/Views/Admin/Payroll.cshtml", payrolls);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Payroll");
            return View(new PayrollCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PayrollCreateViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Payroll");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please complete all required payroll fields.";
                var payrolls = await _payrollService.GetAllPayrollRecordsAsync();
                return View("~/Views/Admin/Payroll.cshtml", payrolls);
            }

            int adminId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (adminId == 0)
                return RedirectToAction("Login", "Account");

            bool created = await _payrollService.CreatePayrollRecordAsync(model, adminId);

            if (!created)
            {
                TempData["ErrorMessage"] = "Unable to create payroll record.";
                var payrolls = await _payrollService.GetAllPayrollRecordsAsync();
                return View("~/Views/Admin/Payroll.cshtml", payrolls);
            }

            TempData["SuccessMessage"] = "Payroll record created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}