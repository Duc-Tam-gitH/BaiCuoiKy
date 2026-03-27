using BaiCuoiKy.Models;
using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaiCuoiKy.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // =========================
        // REGISTER
        // =========================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // 1. Gán mặc định Role là Khachthue vì bạn đã bỏ Chutro
            model.SelectedRole = "Khachthue";

            // Xóa kiểm tra hợp lệ cho Role vì mình đã gán cứng ở trên
            ModelState.Remove("SelectedRole");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. Tạo đối tượng User
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Address = "" // Để trống hoặc gán mặc định
            };

            // 3. Thực hiện tạo tài khoản
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // 🔥 QUAN TRỌNG: Tự động tạo Role "Khachthue" nếu máy bạn chưa có trong DB
                if (!await _roleManager.RoleExistsAsync("Khachthue"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Khachthue"));
                }

                // 4. Gán quyền Khachthue cho người dùng mới
                await _userManager.AddToRoleAsync(user, "Khachthue");

                // 5. Đăng nhập và chuyển hướng về trang chủ
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // 6. Nếu có lỗi (mật khẩu yếu, trùng email...), hiển thị ra màn hình
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // =========================
        // LOGIN
        // =========================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
            return View(model);
        }

        // =========================
        // LOGOUT
        // =========================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
