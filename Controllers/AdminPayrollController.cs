using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.ViewModels;
using System.Security.Claims;

namespace NZFTC_Portal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPayrollController : Controller
    {
        private readonly IPayrollService _payrollService;

        public AdminPayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var payrolls = await _payrollService.GetAllPayrollRecordsAsync();
            return View(payrolls);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PayrollCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PayrollCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            bool created = await _payrollService.CreatePayrollRecordAsync(model, adminId);

            if (!created)
            {
                ModelState.AddModelError(string.Empty, "Unable to create payroll record.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Payroll record created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}