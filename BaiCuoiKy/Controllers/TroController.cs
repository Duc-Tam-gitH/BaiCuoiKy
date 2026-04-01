using BaiCuoiKy.Models;
using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace BaiCuoiKy.Controllers
{
    public class TroController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TroController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================================================
        // 🟢 1. TẠO PHÒNG TRỌ
        // =========================================================

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var model = new CreateTroViewModel
            {
                TieuDe = "",
                DiaChi = "",
                MoTa = "",
                Gia = 0,
                DienTich = 0,

                
                TrangThai = TrangThaiPhong.DangTrong,

                Categories = await _context.Categories
                    .Where(c => c.TrangThai == true)
                    .ToListAsync()
            };

            return View("CreateRoom", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateTroViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy user!";
                    return RedirectToAction("Index", "Home");
                }

                var tro = new Tro
                {
                    TieuDe = model.TieuDe,
                    DiaChi = model.DiaChi,
                    Gia = model.Gia,
                    MoTa = model.MoTa,
                    DienTich = model.DienTich,

                    // ✅ FIX: enum
                    TrangThai = model.TrangThai,

                    CategoryId = model.CategoryId,
                    UserId = userId,
                    User = user,
                    NgayDang = DateTime.Now
                };

                _context.Tros.Add(tro);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Tạo phòng thành công!";
                return RedirectToAction("ManageSystem", "Admin");
            }

            model.Categories = await _context.Categories
                .Where(c => c.TrangThai == true)
                .ToListAsync();

            return View("CreateRoom", model);
        }

        // =========================================================
        // 🟡 2. DANH SÁCH QUẢN LÝ
        // =========================================================

        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var data = await _context.Tros
                .Include(t => t.User)
                .Include(t => t.AnhPhongs)
                .Include(t => t.Category)
                .OrderByDescending(t => t.NgayDang)
                .ToListAsync();

            return View("ManageSystem", data);
        }

        // =========================================================
        // 🔵 3. CHI TIẾT PHÒNG
        // =========================================================

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var room = await _context.Tros
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.AnhPhongs)
                .Include(t => t.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (room == null)
                return RedirectToAction("Index", "Home");

            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.IsOwner = User.FindFirstValue(ClaimTypes.NameIdentifier) == room.UserId;
            ViewBag.CanEdit = ViewBag.IsAdmin || ViewBag.IsOwner;

            return View(room);
        }

        // =========================================================
        // 🟠 4. SỬA PHÒNG
        // =========================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tro = await _context.Tros.FindAsync(id);
            if (tro == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tro.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa phòng này!";
                return RedirectToAction("Index", "Home");
            }

            var model = new EditTroViewModel
            {
                Id = tro.Id,
                TieuDe = tro.TieuDe,
                DiaChi = tro.DiaChi,
                Gia = tro.Gia,
                MoTa = tro.MoTa,
                DienTich = tro.DienTich,
                TrangThai = tro.TrangThai,
                CategoryId = tro.CategoryId,
                Categories = await _context.Categories.Where(c => c.TrangThai == true).ToListAsync()
            };

            return View("EditRoom", model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.Where(c => c.TrangThai == true).ToListAsync();
                return View("EditRoom", model);
            }

            var tro = await _context.Tros.FindAsync(model.Id);
            if (tro == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tro.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            tro.TieuDe = model.TieuDe;
            tro.DiaChi = model.DiaChi;
            tro.Gia = model.Gia;
            tro.MoTa = model.MoTa;
            tro.DienTich = model.DienTich;
            tro.TrangThai = model.TrangThai;
            tro.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật thành công!";

            // Trở về đúng trang tùy theo Role
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("ManageSystem", "Admin");
            }
            return RedirectToAction("Manage");
        }

        // =========================================================
        // 🔴 5. XÓA PHÒNG
        // =========================================================

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var tro = await _context.Tros
                .Include(t => t.AnhPhongs)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tro == null) return NotFound();

            return View(tro);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tro = await _context.Tros.FindAsync(id);
            if (tro == null) return NotFound();

            _context.Tros.Remove(tro);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa!";
            return RedirectToAction("_Rooms", "Admin");
        }
    }
}