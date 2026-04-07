using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.Models;

namespace NZFTC_Portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // Loads the login page.
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Processes the login form submission.
        [HttpPost]
        public IActionResult Login(string email, string password, string role)
        {
            // Authenticates the user from the database using the submitted email and password.
            var user = _authService.Login(email, password);

            // Stops the login attempt if the account details are incorrect.
            if (user == null)
            {
                TempData["LoginError"] = "Incorrect details or role. Please try again.";
                return RedirectToAction("Login");
            }

            // Uses the selected role pill as an RBAC confirmation step.
            // The login only continues if the selected role matches the role stored for that user.
            if ((user.Role == "Admin" || user.Role == "Administrator") &&
                (role == "Admin" || role == "Administrator"))
            {
                // Sends confirmed admin users to the admin dashboard.
                return RedirectToAction("Dashboard", "Admin");
            }

            // Sends confirmed employee users to the employee dashboard.
            if (user.Role == "Employee" && role == "Employee")
            {
                return RedirectToAction("Dashboard", "Employee");
            }

            // Rejects login if the selected role pill does not match the account role.
            TempData["LoginError"] = "Incorrect details or role. Please try again.";
            return RedirectToAction("Login");
        }

        // Logs the current user out and clears their session through the auth service.
        public IActionResult Logout()
        {
            _authService.Logout();
            return RedirectToAction("Login");
        }
    }
}