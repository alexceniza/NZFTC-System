using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Controllers
{
    public class EmployeeCaseController : Controller
    {
        private readonly ICaseService _caseService;

        public EmployeeCaseController(ICaseService caseService)
        {
            _caseService = caseService;
        }

        private bool IsEmployee()
        {
            return HttpContext.Session.GetString("Role") == "Employee";
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            int employeeId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (employeeId == 0)
                return RedirectToAction("Login", "Account");

            var cases = await _caseService.GetEmployeeCasesAsync(employeeId);

            return View(cases);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CaseCreateViewModel model)
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            int employeeId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (employeeId == 0)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please complete all required fields.";
                return RedirectToAction("MyInfo", "Employee");
            }

            try
            {
                await _caseService.SubmitCaseAsync(employeeId, model);
                TempData["SuccessMessage"] = "Case submitted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("MyInfo", "Employee");
        }
    }
}