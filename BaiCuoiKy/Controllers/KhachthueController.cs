using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BaiCuoiKy.Models;

namespace BaiCuoiKy.Controllers
{
    [Authorize(Roles = "Khachthue")] // Chỉ cho phép user có Role "Khachthue" vào
    public class KhachthueController : Controller
    {
        private readonly AppDbContext _context;

        public KhachthueController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. TRANG DANH SÁCH YÊU THÍCH
        // URL: /Khachthue/Favorites
        // ==========================================
        public async Task<IActionResult> Favorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Tro)
                    .ThenInclude(t => t.AnhPhongs)
                .Select(f => f.Tro)
                .ToListAsync();

            return View(favorites); // Sẽ tìm file Views/Khachthue/Favorites.cshtml
        }

        // ==========================================
        // 2. TRANG QUẢN LÝ ĐẶT PHÒNG (BOOKINGS)
        // URL: /Khachthue/Bookings
        // ==========================================
        [Authorize]
        public async Task<IActionResult> Bookings()
        {
            // Lấy ID của khách hàng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách phòng đã đặt của người này
            var danhSachDatPhong = await _context.Bookings
                .Include(b => b.Tro)   // 🔥 QUAN TRỌNG: Phải có dòng này thì View mới gọi được item.Tro.TieuDe
                .Include(b => b.User)  // Nối bảng User
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.NgayDat)
                .ToListAsync();

            return View(danhSachDatPhong);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ConfirmPayment(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                // Chuyển sang trạng thái chờ Admin check tiền trong tài khoản
                booking.TrangThai = "ChoXacNhan";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã gửi thông báo xác nhận thanh toán cho Admin!";
            }
            return RedirectToAction("Bookings");
        }
    }
}