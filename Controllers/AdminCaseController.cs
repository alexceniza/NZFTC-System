using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.ViewModels;
using System.Security.Claims;

namespace NZFTC_Portal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCaseController : Controller
    {
        private readonly ICaseService _caseService;

        public AdminCaseController(ICaseService caseService)
        {
            _caseService = caseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cases = await _caseService.GetAllCasesAsync();
            return View(cases);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(CaseStatusUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid case update request.";
                return RedirectToAction(nameof(Index));
            }

            int adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            bool updated = await _caseService.UpdateCaseStatusAsync(adminId, model);

            TempData[updated ? "SuccessMessage" : "ErrorMessage"] =
                updated
                ? "Case status updated successfully."
                : "Unable to update case status.";

            return RedirectToAction(nameof(Index));
        }
    }
}