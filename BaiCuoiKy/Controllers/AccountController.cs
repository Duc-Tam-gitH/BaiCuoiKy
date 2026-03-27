using BaiCuoiKy.Models;
using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

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
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                RoleName = roles.FirstOrDefault() ?? "Thành viên"
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Message"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("Profile", model);
        }
        // GET: Quên mật khẩu
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action("ResetPassword", "Account",
                        new { token, email = model.Email }, Request.Scheme);

                    // --- ĐOẠN CODE GỬI EMAIL THẬT ---
                    try
                    {
                        var fromAddress = new MailAddress("hoangnam01645994528@gmail.com", "House88 Hỗ trợ");
                        var toAddress = new MailAddress(model.Email);
                        string fromPassword = "sslu ezce txez krua"; // Mật khẩu ứng dụng

                        string subject = "Đặt lại mật khẩu - House88";
                        string body = $"Chào bạn, vui lòng nhấn vào link sau để đổi mật khẩu: <a href='{callbackUrl}'>Nhấn vào đây</a>";

                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                        };

                        using (var message = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = subject,
                            Body = body,
                            IsBodyHtml = true
                        })
                        {
                            await smtp.SendMailAsync(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Nếu lỗi gửi mail, bạn có thể log lỗi ở đây
                        ModelState.AddModelError("", "Lỗi gửi mail: " + ex.Message);
                        return View(model);
                    }
                }

                // Sau khi gửi xong (hoặc nếu user ko tồn tại), chuyển hướng sang trang thông báo
                return RedirectToAction("ForgotPasswordConfirmation");
            }
            return View(model);
        }

        // GET: Trang đặt lại mật khẩu mới
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null) return RedirectToAction("Index", "Home");
            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        // POST: Lưu mật khẩu mới
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded) return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
    }
}
