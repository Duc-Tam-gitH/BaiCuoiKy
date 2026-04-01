using BaiCuoiKy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BaiCuoiKy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ==================== TRANG CHỦ ====================
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 🔔 Notifications
            if (userId != null)
            {
                ViewBag.Notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                ViewBag.NotiCount = await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);
            }

            // 📂 Load danh mục cho dropdown
            ViewBag.Categories = await _context.Categories
                .Where(c => c.TrangThai == true)
                .ToListAsync();

            try
            {
                var danhSachTro = await _context.Tros
                    .Include(t => t.AnhPhongs)
                    .Where(t => t.TrangThai == TrangThaiPhong.DangTrong
                             || t.TrangThai == TrangThaiPhong.DangXuLy)
                    .OrderByDescending(t => t.NgayDang)
                    .Take(8)
                    .ToListAsync();

                return View(danhSachTro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load home index data");
                return View(new List<Tro>());
            }
        }

        // ==================== TÌM KIẾM ====================
        [HttpGet]
        public async Task<IActionResult> Search(string keyword, string khuvuc, string loai, string gia)
        {
            // 📂 Load danh mục cho dropdown (quan trọng)
            ViewBag.Categories = await _context.Categories
                .Where(c => c.TrangThai == true)
                .ToListAsync();

            var query = _context.Tros
                .Include(t => t.AnhPhongs)
                .Include(t => t.Category)
                .Where(t => t.TrangThai == TrangThaiPhong.DangTrong
                         || t.TrangThai == TrangThaiPhong.DangXuLy);

            // 🔍 Tìm theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => t.TieuDe.Contains(keyword) || t.MoTa.Contains(keyword));
            }

            // 📍 Lọc khu vực
            if (!string.IsNullOrEmpty(khuvuc))
            {
                query = query.Where(t => t.DiaChi.Contains(khuvuc));
            }

            // 📂 Lọc theo danh mục (từ dropdown)
            if (!string.IsNullOrEmpty(loai))
            {
                query = query.Where(t => t.Category != null
                                      && t.Category.TenDanhMuc.Contains(loai));
            }

            // 💰 Lọc theo giá
            if (!string.IsNullOrEmpty(gia))
            {
                switch (gia)
                {
                    case "1":
                        query = query.Where(t => t.Gia < 2000000);
                        break;
                    case "2":
                        query = query.Where(t => t.Gia >= 2000000 && t.Gia <= 5000000);
                        break;
                    case "3":
                        query = query.Where(t => t.Gia > 5000000);
                        break;
                }
            }

            var results = await query
                .OrderByDescending(t => t.NgayDang)
                .ToListAsync();

            // 🔁 giữ lại giá trị filter
            ViewBag.Keyword = keyword;
            ViewBag.KhuVuc = khuvuc;
            ViewBag.Loai = loai;
            ViewBag.Gia = gia;

            return View(results);
        }

        // ==================== CHI TIẾT ====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Thêm đoạn này để Navbar có dữ liệu hiển thị
            ViewBag.Categories = await _context.Categories
                .Where(c => c.TrangThai == true)
                .ToListAsync();

            var tro = await _context.Tros
                .Include(t => t.User)
                .Include(t => t.AnhPhongs)
                .Include(t => t.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (tro == null) return NotFound();

            return View(tro);
        }

        // ==================== ERROR ====================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}