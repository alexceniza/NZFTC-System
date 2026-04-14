using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Controllers
{
    public class AdminLeaveController : Controller
    {
        private readonly ILeaveService _leaveService;

        public AdminLeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
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
        public async Task<IActionResult> Pending()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Leave");

            var pendingRequests = await _leaveService.GetPendingLeaveRequestsAsync();

            var model = pendingRequests.Select(lr => new AdminRecentLeaveRequestViewModel
            {
                LeaveRequestId = lr.LeaveRequestId,
                EmployeeName = lr.Employee != null && lr.Employee.User != null
                    ? lr.Employee.User.FullName
                    : "Unknown Employee",
                LeaveType = lr.LeaveType,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                Status = lr.Status
            }).ToList();

            return View("~/Views/Admin/Leave.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decide(AdminLeaveDecisionViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid leave action.";
                return RedirectToAction(nameof(Pending));
            }

            int adminId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (adminId == 0)
                return RedirectToAction("Login", "Account");

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
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetAdminViewData("Leave");

            var model = await _leaveService.GetAdminLeaveReportAsync(filter);
            return View(model);
        }
    }
}