using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BaiCuoiKy.Models;
using System.Diagnostics;
using System.Security.Claims;

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

        // Action Index duy nhất - hiển thị trang chủ
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy thông báo nếu đã đăng nhập
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

            try
            {
                // Lấy danh sách phòng trọ đã duyệt
                var danhSachTro = await _context.Tros
                .Include(t => t.AnhPhongs)
                // .Include(t => t.Category)  // Comment
                .Where(t => t.TrangThai == true)
                .OrderByDescending(t => t.NgayDang)
                .Take(8)
                .ToListAsync();

                return View(danhSachTro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load home index data");
                // ✅ Trả về danh sách rỗng đúng kiểu
                return View(new List<Tro>());
            }
        }

        // ==================== TÌM KIẾM ====================

        [HttpGet]
        public async Task<IActionResult> Search(string keyword, string khuvuc, string loai, string gia)
        {
            var query = _context.Tros
                .Include(t => t.AnhPhongs)
                // .Include(t => t.Category)  // Comment
                .Where(t => t.TrangThai == true);

            // Lọc theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => t.TieuDe.Contains(keyword) || t.MoTa.Contains(keyword));
            }

            // Lọc theo khu vực
            if (!string.IsNullOrEmpty(khuvuc))
            {
                query = query.Where(t => t.DiaChi.Contains(khuvuc));
            }

            // Lọc theo loại hình
            if (!string.IsNullOrEmpty(loai))
            {
                query = query.Where(t => t.Category != null && t.Category.TenDanhMuc.Contains(loai));
            }

            // Lọc theo giá
            if (!string.IsNullOrEmpty(gia))
            {
                switch (gia)
                {
                    case "1": // Dưới 2 triệu
                        query = query.Where(t => t.Gia < 2000000);
                        break;
                    case "2": // 2 - 5 triệu
                        query = query.Where(t => t.Gia >= 2000000 && t.Gia <= 5000000);
                        break;
                    case "3": // Trên 5 triệu
                        query = query.Where(t => t.Gia > 5000000);
                        break;
                }
            }

            var results = await query
                .OrderByDescending(t => t.NgayDang)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.KhuVuc = khuvuc;
            ViewBag.Loai = loai;
            ViewBag.Gia = gia;

            return View(results);
        }

        // ==================== CHI TIẾT PHÒNG TRỌ ====================

        // Action Details - xem chi tiết phòng trọ từ trang chủ
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tro = await _context.Tros
                .Include(t => t.User)
                .Include(t => t.AnhPhongs)
                //.Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (tro == null) return NotFound();

            return View(tro);
        }

        // ==================== ERROR ====================

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}