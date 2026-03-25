using BaiCuoiKy.Models;
using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BaiCuoiKy.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ======================
        // REGISTER
        // ======================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ✅ Chỉ cho phép 2 role hợp lệ
            var validRoles = new[] { "ChuTro", "KhachThue" };

            string role = validRoles.Contains(model.SelectedRole)
                ? model.SelectedRole
                : "KhachThue";

            var user = new ApplicationUser // Đổi từ IdentityUser thành ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName, // Bắt buộc phải có vì model có 'required'
                Phone = model.Phone        // Bắt buộc phải có vì model có 'required'
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // ✅ Gán role (role phải được tạo sẵn ở Program.cs)
                await _userManager.AddToRoleAsync(user, role);

                // ✅ Login luôn
                await _signInManager.SignInAsync(user, false);

                // ✅ Redirect theo role
                return RedirectByRole(role);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // ======================
        // LOGIN
        // ======================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var roles = await _userManager.GetRolesAsync(user);

                return RedirectByRole(roles.FirstOrDefault());
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        // ======================
        // LOGOUT
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ======================
        // ACCESS DENIED
        // ======================
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ======================
        // HELPER: REDIRECT ROLE
        // ======================
        private IActionResult RedirectByRole(string role)
        {
            switch (role)
            {
                case "Admin":
                    return RedirectToAction("Index", "Admin");

                case "ChuTro":
                    return RedirectToAction("MyRooms", "Chutro");

                case "KhachThue":
                default:
                    return RedirectToAction("Index", "Home");
            }
        }
    }
}