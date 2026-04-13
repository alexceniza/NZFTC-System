using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Controllers
{
    public class EmployeeLeaveController : Controller
    {
        private readonly ILeaveService _leaveService;

        public EmployeeLeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
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

            SetEmployeeViewData("Leave");

            int employeeId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (employeeId == 0)
                return RedirectToAction("Login", "Account");

            var leaveRequests = await _leaveService.GetEmployeeLeaveRequestsAsync(employeeId);
            return View("~/Views/Employee/Leave.cshtml", leaveRequests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveRequestCreateViewModel model)
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            int employeeId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (employeeId == 0)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                SetEmployeeViewData("Leave");
                var leaveRequests = await _leaveService.GetEmployeeLeaveRequestsAsync(employeeId);
                TempData["ErrorMessage"] = "Please complete all required fields correctly.";
                return View("~/Views/Employee/Leave.cshtml", leaveRequests);
            }

            try
            {
                await _leaveService.SubmitLeaveRequestAsync(employeeId, model);
                TempData["SuccessMessage"] = "Leave request submitted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                SetEmployeeViewData("Leave");
                var leaveRequests = await _leaveService.GetEmployeeLeaveRequestsAsync(employeeId);
                TempData["ErrorMessage"] = ex.Message;
                return View("~/Views/Employee/Leave.cshtml", leaveRequests);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Balance()
        {
            if (!IsEmployee())
                return RedirectToAction("Login", "Account");

            int employeeId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (employeeId == 0)
                return RedirectToAction("Login", "Account");

            var balance = await _leaveService.GetEmployeeLeaveBalanceAsync(employeeId);
            return Ok(balance);
        }
    }
}