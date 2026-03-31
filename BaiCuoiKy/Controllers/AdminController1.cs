using BaiCuoiKy.Models;
using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            var users = _userManager.Users.AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.UserName.Contains(searchString) ||
                                         u.Email.Contains(searchString) ||
                                         u.FullName.Contains(searchString));
            }

            var userList = new List<ManagerUsersViewModel>();

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

            // FILTER ROLE
            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                userList = userList.Where(u => u.Roles.Contains(roleFilter)).ToList();
            }

            // SORT
            userList = sortOrder switch
            {
                "name_desc" => userList.OrderByDescending(u => u.User.UserName).ToList(),
                "email" => userList.OrderBy(u => u.User.Email).ToList(),
                "email_desc" => userList.OrderByDescending(u => u.User.Email).ToList(),
                "fullname" => userList.OrderBy(u => u.User.FullName).ToList(),
                "fullname_desc" => userList.OrderByDescending(u => u.User.FullName).ToList(),
                _ => userList.OrderBy(u => u.User.UserName).ToList()
            };

            var rolesList = _roleManager.Roles.Select(r => r.Name).ToList();
            rolesList.Insert(0, "All");
            ViewBag.RolesList = rolesList;

            return View(userList);
        }

        // =====================================================
        // GET: FORM SỬA USER
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
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
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // UPDATE INFO
            user.FullName = model.FullName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return View(model);

            // LOCK / UNLOCK
            if (model.IsLocked)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            else
                await _userManager.SetLockoutEndDateAsync(user, null);

            // UPDATE ROLE
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = model.SelectedRoles?.Except(currentRoles) ?? new List<string>();
            var rolesToRemove = currentRoles.Except(model.SelectedRoles ?? new List<string>());

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Dashboard", new { section = "users" });
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
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return Json(new { success = false });

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}