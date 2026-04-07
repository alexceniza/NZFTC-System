using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.Services;
using NZFTC_Portal.ViewModels;
using System.Security.Claims;

namespace NZFTC_Portal.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeLeaveController : Controller
    {
        private readonly ILeaveService _leaveService;

        public EmployeeLeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        public async Task<IActionResult> Index()
        {
            int employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var leaveRequests = await _leaveService.GetEmployeeLeaveRequestsAsync(employeeId);
            return View(leaveRequests);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new LeaveRequestCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveRequestCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _leaveService.SubmitLeaveRequestAsync(employeeId, model);
                TempData["SuccessMessage"] = "Leave request submitted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
    }
}