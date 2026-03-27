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

            var userList = new System.Collections.Generic.List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserWithRolesViewModel
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
                    var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
                    var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();

                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                    TempData["Success"] = "Cập nhật tài khoản thành công!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Load lại roles nếu có lỗi
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            model.Roles = allRoles;

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
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }
    }
}


