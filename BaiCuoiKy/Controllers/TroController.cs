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
                KhuVuc = "", 
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
                    KhuVuc = model.KhuVuc, 
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

            bool isFavorited = false;
            if (User.Identity?.IsAuthenticated == true) 
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                isFavorited = await _context.Favorites.AnyAsync(f => f.TroId == id && f.UserId == currentUserId);
            }
            ViewBag.IsFavorited = isFavorited;

            return View(room);
        }
        // 2. THÊM ACTION MỚI NÀY VÀO CUỐI CONTROLLER ĐỂ XỬ LÝ NÚT BẤM TIM:
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int troId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tìm xem đã có trong danh sách yêu thích chưa
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.TroId == troId && f.UserId == userId);

            if (favorite != null)
            {
                // Nếu đã tim rồi -> Bỏ tim
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorited = false });
            }
            else
            {
                // Nếu chưa tim -> Thêm tim
                _context.Favorites.Add(new Favorite { TroId = troId, UserId = userId });
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorited = true });
            }
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

            // ✅ Bổ sung để Navbar không bị lỗi
            var categories = await _context.Categories.Where(c => c.TrangThai == true).ToListAsync();
            ViewBag.Categories = categories;

            var model = new EditTroViewModel
            {
                Id = tro.Id,
                TieuDe = tro.TieuDe,
                DiaChi = tro.DiaChi,
                KhuVuc = tro.KhuVuc,
                Gia = tro.Gia,
                MoTa = tro.MoTa,
                DienTich = tro.DienTich,
                TrangThai = tro.TrangThai,
                CategoryId = tro.CategoryId,
                Categories = categories // Gán danh sách vào model để dùng cho DropdownList
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
                // Nếu lỗi, phải load lại danh sách Categories để Dropdown không bị trống
                model.Categories = await _context.Categories.Where(c => c.TrangThai == true).ToListAsync();
                return View("EditRoom", model);
            }

            var tro = await _context.Tros.FindAsync(model.Id);
            if (tro == null) return NotFound();

            // Kiểm tra quyền (Chủ phòng hoặc Admin)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tro.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Cập nhật dữ liệu
            tro.TieuDe = model.TieuDe;
            tro.DiaChi = model.DiaChi;
            tro.KhuVuc = model.KhuVuc; 
            tro.Gia = model.Gia;
            tro.MoTa = model.MoTa;
            tro.DienTich = model.DienTich;
            tro.TrangThai = model.TrangThai;
            tro.CategoryId = model.CategoryId;

            try
            {
                _context.Update(tro);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật phòng trọ thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                model.Categories = await _context.Categories.Where(c => c.TrangThai == true).ToListAsync();
                return View("EditRoom", model);
            }

            // ĐIỀU HƯỚNG SAU KHI LƯU
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin", new { section = "rooms" });
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
            // KIỂM TRA QUYỀN TRƯỚC KHI XEM TRANG XÓA
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tro.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Bạn không có quyền xóa phòng này!";
                return RedirectToAction("Index", "Home");
            }

            return View(tro);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tro = await _context.Tros.FindAsync(id);
            if (tro == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tro.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Bạn không có quyền xóa phòng này!";
                return RedirectToAction("Index", "Home");
            }

            _context.Tros.Remove(tro);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa phòng trọ!!!";
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin", new { section = "rooms" });
            }
            return RedirectToAction("Manage");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BookRoom(int troId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name ?? "Một khách hàng";

            // 1. Lấy thông tin phòng để tính tiền cọc
            var tro = await _context.Tros.FindAsync(troId);
            if (tro == null || tro.TrangThai == TrangThaiPhong.DaChoThue)
            {
                TempData["Error"] = "Phòng không tồn tại hoặc đã được thuê!";
                return RedirectToAction("Details", new { id = troId });
            }

            // 2. Chống Spam: Kiểm tra xem khách đã gửi yêu cầu phòng này chưa
            var daDatPhong = await _context.Bookings
                .AnyAsync(b => b.TroId == troId && b.UserId == userId && (b.TrangThai == "ChoDuyet" || b.TrangThai == "ChoThanhToan"));

            if (daDatPhong)
            {
                TempData["Error"] = "Bạn đã gửi yêu cầu đặt phòng này rồi. Vui lòng chờ Admin duyệt!";
                return RedirectToAction("Details", new { id = troId });
            }

            // 3. Tạo Đơn đặt phòng (Booking)
            var booking = new Booking
            {
                TroId = troId,
                UserId = userId,
                NgayDat = DateTime.Now,
                NgayNhan = DateTime.Now.AddDays(1), // Mặc định báo nhận phòng vào ngày mai
                TienCoc = (decimal)(tro.Gia * 0.2m), // 🔥 TỰ ĐỘNG TÍNH CỌC 20%
                TrangThai = "ChoDuyet"
            };
            _context.Bookings.Add(booking);

            // 4. Bắn thông báo cho Admin/Chủ trọ
            if (tro.UserId != userId)
            {
                var notification = new Notification
                {
                    UserId = tro.UserId,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Message = $"Khách hàng {userName} vừa yêu cầu đặt phòng '{tro.TieuDe}'. Vui lòng kiểm duyệt!",
                    Url = "/Admin/Dashboard?section=bookings"
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Gửi yêu cầu đặt phòng thành công! Đang chờ duyệt.";

            // 5. Chuyển hướng sang trang quản lý Booking của khách hàng
            return RedirectToAction("Bookings", "Khachthue");
        }
    }
}