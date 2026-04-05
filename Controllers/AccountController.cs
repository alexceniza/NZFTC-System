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

        // Login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login form submission
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _authService.Login(email, password);

            // Invalid login
            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // Route based on role FROM DATABASE
            if (user.Role == "Admin" || user.Role == "Administrator")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            if (user.Role == "Employee")
            {
                return RedirectToAction("Dashboard", "Employee");
            }

            return RedirectToAction("Login");
        }

        // Logout action
        public IActionResult Logout()
        {
            _authService.Logout();
            return RedirectToAction("Login");
        }
    }
}