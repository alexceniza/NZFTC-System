using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.Services;
using NZFTC_Portal.ViewModels;
using System.Security.Claims;

namespace NZFTC_Portal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminLeaveController : Controller
    {
        private readonly ILeaveService _leaveService;

        public AdminLeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        public async Task<IActionResult> Pending()
        {
            var pendingRequests = await _leaveService.GetPendingLeaveRequestsAsync();
            return View(pendingRequests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decide(AdminLeaveDecisionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToAction(nameof(Pending));
            }

            int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            bool updated = await _leaveService.ApproveOrDeclineAsync(
                model.LeaveRequestId,
                adminId,
                model.Decision
            );

            TempData[updated ? "SuccessMessage" : "ErrorMessage"] =
                updated
                ? $"Leave request {model.Decision.ToLower()} successfully."
                : "Unable to update leave request.";

            return RedirectToAction(nameof(Pending));
        }

        [HttpGet]
        public async Task<IActionResult> Report(AdminLeaveReportFilterViewModel filter)
        {
            var model = await _leaveService.GetAdminLeaveReportAsync(filter);
            return View(model);
        }
    }
}