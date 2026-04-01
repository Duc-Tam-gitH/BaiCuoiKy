using BaiCuoiKy.Models;
using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BaiCuoiKy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AdminController> _logger; // Thêm dòng này

        public AdminController(
         UserManager<ApplicationUser> userManager,
         RoleManager<IdentityRole> roleManager,
         AppDbContext context, 
            SignInManager<ApplicationUser> signInManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
                _roleManager = roleManager; 
                _context = context;
                _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult ManageSystem()
        {
            return RedirectToAction("Dashboard", new { section = "overview" });
        }
        // =====================================================
        // DTO: NHẬN REQUEST CẬP NHẬT TRẠNG THÁI PHÒNG
        // =====================================================
        public class UpdateTrangThaiDto
        {
            public int Id { get; set; }
            public int TrangThai { get; set; }
        }

        // =====================================================
        // API: CẬP NHẬT TRẠNG THÁI PHÒNG (Đang trống / Đang xử lý / Đã cho thuê)
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> UpdateTrangThai([FromBody] UpdateTrangThaiDto model)
        {
            var tro = await _context.Tros.FirstOrDefaultAsync(t => t.Id == model.Id);
            if (tro == null) return Json(new { success = false });

            tro.TrangThai = (TrangThaiPhong)model.TrangThai;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // =====================================================
        // HELPER: LẤY CHỮ CÁI ĐẦU TÊN USER
        // =====================================================
        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "A";
            var parts = name.Trim().Split(' ');
            if (parts.Length >= 2)
                return parts[0][0].ToString().ToUpper() + parts[1][0].ToString().ToUpper();
            return name[0].ToString().ToUpper();
        }

        // =====================================================
        // DANH SÁCH USER + SEARCH + FILTER + SORT
        // =====================================================
        public async Task<IActionResult> Index(string sortOrder, string searchString, string roleFilter)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentRoleFilter = roleFilter;

            var usersQuery = _userManager.Users.AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(searchString) ||
                                                   u.Email.Contains(searchString) ||
                                                   u.FullName.Contains(searchString));
            }

            // 🔥 SỬA TẠI ĐÂY: Ép kiểu ToListAsync() để đóng DataReader cũ trước khi duyệt vòng lặp
            var users = await usersQuery.ToListAsync();

            var userList = new List<ManagerUsersViewModel>();

            foreach (var user in users)
            {
                // Bây giờ gọi các lệnh async bên trong này sẽ không bị lỗi "Open DataReader" nữa
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new ManagerUsersViewModel
                {
                    User = user,
                    Roles = roles.ToList(),
                    TotalTros = await _context.Tros.CountAsync(t => t.UserId == user.Id),
                    TotalBookings = await _context.Bookings.CountAsync(b => b.UserId == user.Id)
                });
            }

            // FILTER ROLE (Giữ nguyên logic bên dưới)
            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                userList = userList.Where(u => u.Roles.Contains(roleFilter)).ToList();
            }

            // SORT (Giữ nguyên logic bên dưới)
            userList = sortOrder switch
            {
                "name_desc" => userList.OrderByDescending(u => u.User.UserName).ToList(),
                "email" => userList.OrderBy(u => u.User.Email).ToList(),
                "email_desc" => userList.OrderByDescending(u => u.User.Email).ToList(),
                "fullname" => userList.OrderBy(u => u.User.FullName).ToList(),
                "fullname_desc" => userList.OrderByDescending(u => u.User.FullName).ToList(),
                _ => userList.OrderBy(u => u.User.UserName).ToList()
            };

            var rolesList = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            rolesList.Insert(0, "All");
            ViewBag.RolesList = rolesList;

            return View("Users",userList);
        }

        // =====================================================
        // GET: FORM SỬA USER
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            return View(new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                IsLocked = await _userManager.IsLockedOutAsync(user),
                Roles = allRoles,
                UserRoles = userRoles.ToList()
            });
        }

        // =====================================================
        // POST: CẬP NHẬT USER + ROLE + LOCK
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View("EditUser", model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // Cập nhật thông tin
            user.FullName = model.FullName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.UserName;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) return View("EditUser", model);

            // Khóa/Mở khóa
            if (model.IsLocked)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            else
                await _userManager.SetLockoutEndDateAsync(user, null);

            // Cập nhật quyền (Roles)
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = model.SelectedRoles?.Except(currentRoles) ?? new List<string>();
            var rolesToRemove = currentRoles.Except(model.SelectedRoles ?? new List<string>());

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            TempData["Success"] = "Cập nhật thành công!";
            if (rolesToAdd.Any())
                await _userManager.AddToRolesAsync(user, rolesToAdd);

            if (rolesToRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            TempData["Success"] = "Cập nhật thành công!";

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id == currentUserId)
            {
                // Nếu người bị sửa chính là người đang đăng nhập -> Refresh lại Cookie
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["Success"] = "Cập nhật thành công!";

            // Logic chuyển hướng như cũ...
            var safeSelectedRoles = model.SelectedRoles ?? new List<string>();
            if (safeSelectedRoles.Contains("Admin"))
            {
                return RedirectToAction("ManageSystem", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        

        // =====================================================
        // KHÓA USER
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            return RedirectToAction("Dashboard", new { section = "users" });
        }

        // =====================================================
        // MỞ KHÓA USER
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
                await _userManager.SetLockoutEndDateAsync(user, null);

            return RedirectToAction("Dashboard", new { section = "users" });
        }

        // =====================================================
        // XÓA USER
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
                await _userManager.DeleteAsync(user);

            return RedirectToAction("Dashboard", new { section = "users" });
        }

        // =====================================================
        // DASHBOARD ADMIN (OVERVIEW / USERS / ROOMS)
        // =====================================================
        public async Task<IActionResult> Dashboard(string section = "overview")
        {
            ViewBag.ActiveSection = section;

            switch (section)
            {
                case "bookings":
                    var allBookings = await _context.Bookings
                        .Include(b => b.User)  // Để biết ai đặt
                        .Include(b => b.Tro)   // Để biết đặt phòng nào
                        .OrderByDescending(b => b.NgayDat)
                        .ToListAsync();

                    ViewBag.Bookings = allBookings;
                    return View("_Bookings"); 
                case "rooms":
                    var rooms = await _context.Tros
                        .Include(t => t.User)
                        .Include(t => t.AnhPhongs)
                        .OrderByDescending(t => t.NgayDang)
                        .ToListAsync();

                    ViewBag.Rooms = rooms;
                    return View("_Rooms");

                case "users":
                    var users = await _userManager.Users.ToListAsync();
                    var userList = new List<ManagerUsersViewModel>();

                    foreach (var u in users)
                    {
                        var roles = await _userManager.GetRolesAsync(u);
                        var isLocked = await _userManager.IsLockedOutAsync(u);

                        userList.Add(new ManagerUsersViewModel
                        {
                            User = u,
                            Roles = roles.ToList(),
                            IsLocked = isLocked
                        });
                    }

                    ViewBag.Users = userList;
                    return View("Users");

                default:
                    ViewBag.TotalUsers = await _userManager.Users.CountAsync();
                    ViewBag.TotalTros = await _context.Tros.CountAsync();
                    return View("ManageSystem");
            }
        }

        // =====================================================
        // XEM CHI TIẾT PHÒNG (ADMIN)
        // =====================================================
        public async Task<IActionResult> RoomDetail(int id)
        {
            var room = await _context.Tros
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.AnhPhongs)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (room == null) return NotFound();

            ViewBag.IsAdmin = true;
            ViewBag.CanEdit = true;

            return View("~/Views/Tro/Details.cshtml", room);
        }

        // =====================================================
        // (GỢI Ý) XÓA / ẨN ĐÁNH GIÁ - BẠN SẼ DÙNG SAU
        // =====================================================


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsHidden = !review.IsHidden; // Đảo trạng thái
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Tro", new { id = review.TroId });
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                int troId = review.TroId; // Lưu lại ID phòng trước khi xóa bình luận

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tro", new { id = troId });
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> ApproveBooking(int bookingId)
        {
            var booking = await _context.Bookings.Include(b => b.Tro).FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking != null)
            {
                // 1. Đổi trạng thái để khách thấy nút Thanh toán
                booking.TrangThai = "ChoThanhToan";

                // 2. TẠO THÔNG BÁO CHO KHÁCH HÀNG (Dòng này giúp ảnh bạn gửi không bị trống)
                var notification = new Notification
                {
                    UserId = booking.UserId, // Gửi cho người đặt
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Message = $"Yêu cầu đặt phòng '{booking.Tro.TieuDe}' đã được duyệt. Hãy thanh toán tiền cọc!",
                    Url = "/Khachthue/Bookings" // Trỏ thẳng về trang có nút QR
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Dashboard", new { section = "bookings" });
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmBookingFinal(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tro)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return NotFound();

            // 1. Cập nhật trạng thái Booking
            booking.TrangThai = "HoanTat";

            // 2. Cập nhật trạng thái Phòng thành Đã cho thuê
            var tro = booking.Tro;
            if (tro != null)
            {
                tro.TrangThai = TrangThaiPhong.DaChoThue;
            }

            await _context.SaveChangesAsync();

            // 3. Gửi Email thông báo thành công
            try
            {
                await SendSuccessEmail(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi gửi mail: " + ex.Message);
            }

            return RedirectToAction("Dashboard", new { section = "bookings" });
        }

        private async Task SendSuccessEmail(Booking booking)
        {
            var email = booking.User.Email;
            var subject = "Xác nhận đặt phòng thành công - House88";
            var body = $@"
        <h3>Chào {booking.User.FullName},</h3>
        <p>Chúc mừng! Đơn đặt cọc cho phòng <b>{booking.Tro.TieuDe}</b> của bạn đã được xác nhận thành công.</p>
        <p><b>Thông tin chi tiết:</b></p>
        <ul>
            <li>Mã đặt phòng: #{booking.Id}</li>
            <li>Số tiền cọc đã nhận: {booking.TienCoc:N0} VNĐ</li>
            <li>Địa chỉ phòng: {booking.Tro.DiaChi}</li>
        </ul>
        <p>Cảm ơn bạn đã tin dùng dịch vụ của House88. Vui lòng liên hệ Admin để nhận phòng.</p>
    ";

            // Đoạn này dùng SmtpClient để gửi (Bạn cần cài System.Net.Mail)
            using (var message = new System.Net.Mail.MailMessage())
            {
                message.To.Add(new System.Net.Mail.MailAddress(email));
                message.From = new System.Net.Mail.MailAddress("your-email@gmail.com", "House88");
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new System.Net.NetworkCredential("your-email@gmail.com", "your-app-password");
                    await client.SendMailAsync(message);
                }
            }
        }
    }
}