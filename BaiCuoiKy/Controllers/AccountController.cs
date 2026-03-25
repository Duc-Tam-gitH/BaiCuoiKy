using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BaiCuoiKy.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // ======================
        // REGISTER (ĐĂNG KÝ)
        // ======================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Logic phân quyền: Nếu trong Form bạn có ô chọn "Tôi là chủ trọ"
                    // Giả sử model có thuộc tính bool IsChutro
                    string assignedRole = !string.IsNullOrEmpty(model.SelectedRole) ? model.SelectedRole : "Khachthue";

                    // Kiểm tra nếu Role chưa tồn tại thì tạo mới (để tránh lỗi máy cá nhân)
                    if (!await _roleManager.RoleExistsAsync(assignedRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(assignedRole));
                    }

                    // Gán quyền cho User vừa tạo
                    await _userManager.AddToRoleAsync(user, assignedRole);

                    // Đăng nhập ngay sau khi đăng ký thành công
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // ======================
        // LOGIN (ĐĂNG NHẬP)
        // ======================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Lấy thông tin user vừa đăng nhập
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);

                    // Điều hướng dựa trên quyền
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    if (roles.Contains("Chutro"))
                    {
                        return RedirectToAction("MyRooms", "Chutro");
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // ======================
        // LOGOUT (ĐĂNG XUẤT)
        // ======================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Trang thông báo nếu truy cập trái phép
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}