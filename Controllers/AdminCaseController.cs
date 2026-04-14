using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Controllers
{
    public class AdminCaseController : Controller
    {
        private readonly ICaseService _caseService;

        public AdminCaseController(ICaseService caseService)
        {
            _caseService = caseService;
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

            SetAdminViewData("Cases");

            var cases = await _caseService.GetAllCasesAsync();
            return View("~/Views/Admin/Cases.cshtml", cases);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(CaseStatusUpdateViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid case update request.";
                return RedirectToAction(nameof(Index));
            }

            int adminId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (adminId == 0)
                return RedirectToAction("Login", "Account");

            bool updated = await _caseService.UpdateCaseStatusAsync(adminId, model);

            TempData[updated ? "SuccessMessage" : "ErrorMessage"] =
                updated
                ? "Case status updated successfully."
                : "Unable to update case status.";

            return RedirectToAction(nameof(Index));
        }
    }
}