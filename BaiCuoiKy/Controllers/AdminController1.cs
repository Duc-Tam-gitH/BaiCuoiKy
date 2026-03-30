using BaiCuoiKy.Models;
using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


//admin @gmail.com
//Admin@123

namespace BaiCuoiKy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        // Hàm xử lý Duyệt hoặc Từ chối bài đăng
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, bool isApproved, string? reason)
        {
            var tro = await _context.Tros.FirstOrDefaultAsync(t => t.Id == id);
            if (tro == null) return NotFound();

            // 1. Cập nhật trạng thái bài đăng (TrangThai: true = Duyệt, false = Từ chối)
            tro.TrangThai = isApproved;

            // 2. Tạo nội dung thông báo dựa trên hành động
            string message = isApproved
                ? $"✅ Tin đăng '{tro.TieuDe}' của bạn đã được phê duyệt và hiển thị trên hệ thống!"
                : $"❌ Tin đăng '{tro.TieuDe}' bị từ chối. Lý do: {reason ?? "Không đạt tiêu chuẩn"}";

            // 3. Lưu thông báo vào Database cho Chủ trọ
            var notification = new Notification
            {
                UserId = tro.UserId, // Gửi cho chủ bài đăng
                Message = message,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Message"] = isApproved ? "Đã duyệt bài thành công!" : "Đã từ chối bài đăng.";
            return RedirectToAction("ManagePosts"); // Hoặc trang quản lý của Admin
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "A";
            var parts = name.Trim().Split(' ');
            if (parts.Length >= 2)
                return parts[0][0].ToString().ToUpper() + parts[1][0].ToString().ToUpper();
            return name[0].ToString().ToUpper();
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString, string roleFilter)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentRoleFilter = roleFilter;

            var users = _userManager.Users.AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.UserName.Contains(searchString) ||
                                         u.Email.Contains(searchString) ||
                                         u.FullName.Contains(searchString));
            }

            var userList = new System.Collections.Generic.List<ManagerUsersViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new ManagerUsersViewModel
                {
                    User = user,
                    Roles = roles.ToList(),
                    TotalTros = await _context.Tros.CountAsync(t => t.UserId == user.Id),
                    TotalBookings = await _context.Bookings.CountAsync(b => b.UserId == user.Id)
                });
            }

            // Lọc theo role
            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                userList = userList.Where(u => u.Roles.Contains(roleFilter)).ToList();
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "name_desc":
                    userList = userList.OrderByDescending(u => u.User.UserName).ToList();
                    break;
                case "email":
                    userList = userList.OrderBy(u => u.User.Email).ToList();
                    break;
                case "email_desc":
                    userList = userList.OrderByDescending(u => u.User.Email).ToList();
                    break;
                case "fullname":
                    userList = userList.OrderBy(u => u.User.FullName).ToList();
                    break;
                case "fullname_desc":
                    userList = userList.OrderByDescending(u => u.User.FullName).ToList();
                    break;
                case "date":
                    userList = userList.OrderBy(u => u.User.LockoutEnd).ToList();
                    break;
                case "date_desc":
                    userList = userList.OrderByDescending(u => u.User.LockoutEnd).ToList();
                    break;
                default:
                    userList = userList.OrderBy(u => u.User.UserName).ToList();
                    break;
            }

            // Lấy danh sách roles để hiển thị filter
            var rolesList = _roleManager.Roles.Select(r => r.Name).ToList();
            rolesList.Insert(0, "All");
            ViewBag.RolesList = rolesList;

            return View(userList);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            var model = new EditUserViewModel
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
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin cơ bản
                user.FullName = model.FullName;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;
                user.Email = model.Email;
                user.UserName = model.UserName;

                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    // Xử lý khóa/mở khóa tài khoản
                    if (model.IsLocked && !await _userManager.IsLockedOutAsync(user))
                    {
                        await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.MaxValue);
                    }
                    else if (!model.IsLocked && await _userManager.IsLockedOutAsync(user))
                    {
                        await _userManager.SetLockoutEndDateAsync(user, null);
                    }

                    
                    // Cập nhật roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToAdd = new List<string>();
                    var rolesToRemove = new List<string>();

                    if (model.SelectedRoles != null)
                    {
                        rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
                        rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();
                    }
                    else
                    {
                        rolesToRemove = currentRoles.ToList();
                    }

                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                    TempData["Success"] = "Cập nhật tài khoản thành công!";
                    return RedirectToAction("Dashboard", new { section = "users" });

                }


                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Load lại roles nếu có lỗi
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            model.Roles = allRoles;

            // Load lại user roles hiện tại
            if (!string.IsNullOrEmpty(model.Id))
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    model.UserRoles = currentRoles.ToList();
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.MaxValue);
                TempData["Success"] = "Đã khóa tài khoản thành công!";
            }
            return RedirectToAction("Dashboard", new { section = "users" });
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["Success"] = "Đã mở khóa tài khoản thành công!";
            }
            return RedirectToAction("Dashboard", new { section = "users" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Xóa tài khoản thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa tài khoản này!";
                }
            }
            return RedirectToAction("Dashboard", new { section = "users" });
        }

        public async Task<IActionResult> ManageSystem()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserInitials = GetInitials(user.FullName ?? user.UserName);
            ViewBag.UserName = user.FullName ?? user.UserName;
            ViewBag.UserRole = "Admin";
            ViewBag.ActiveSection = "overview";

            // Lấy dữ liệu thống kê
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalTros = await _context.Tros.CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.TotalPosts = await _context.Tros.CountAsync();

            return View("ManageSystem");
        }

        public async Task<IActionResult> Dashboard(string section = "overview")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserInitials = GetInitials(user.FullName ?? user.UserName);
            ViewBag.UserName = user.FullName ?? user.UserName;
            ViewBag.UserRole = "Admin";
            ViewBag.ActiveSection = section;

            switch (section)
            {
                case "overview":
                    ViewBag.TotalUsers = await _userManager.Users.CountAsync();
                    ViewBag.TotalTros = await _context.Tros.CountAsync();
                    ViewBag.TotalBookings = await _context.Bookings.CountAsync();
                    ViewBag.TotalPosts = await _context.Tros.CountAsync();
                    return View("ManageSystem");

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
                            TotalTros = await _context.Tros.CountAsync(t => t.UserId == u.Id),
                            TotalBookings = await _context.Bookings.CountAsync(b => b.UserId == u.Id),
                            IsLocked = isLocked
                        });
                    }
                    ViewBag.Users = userList;
                    return View("Users");

                case "rooms":
                    // Lấy danh sách phòng trọ kèm thông tin chủ trọ
                    var rooms = await _context.Tros
                        .Include(t => t.User) // Lấy thông tin chủ trọ
                        .Include(t => t.AnhPhongs)
                        .OrderByDescending(t => t.NgayDang)
                        .ToListAsync();

                    ViewBag.Rooms = rooms;
                    return View("_Rooms");

                case "posts":
                    // Tạo view Posts.cshtml sau
                    ViewBag.Message = "Tính năng đang phát triển";
                    return View("Posts");

                case "settings":
                    // Tạo view Settings.cshtml sau
                    ViewBag.Message = "Tính năng đang phát triển";
                    return View("Settings");

                default:
                    return RedirectToAction("ManageSystem");
            }
        }
        //Chỉnh sửa thông tin tài khoản người dùng

    }

}


