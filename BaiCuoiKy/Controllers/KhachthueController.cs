using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BaiCuoiKy.Models;

namespace BaiCuoiKy.Controllers
{
    [Authorize(Roles = "Khachthue")]
    public class KhachthueController : Controller
    {
        private readonly AppDbContext _context;

        public KhachthueController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Trang danh sách yêu thích
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

            return View(favorites);
        }

        // 2. Trang quản lý đặt phòng
        public async Task<IActionResult> Bookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var danhSachDatPhong = await _context.Bookings
                .Include(b => b.Tro)
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.NgayDat)
                .ToListAsync();

            return View(danhSachDatPhong);
        }

        // 3. Xác nhận thanh toán cọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                booking.TrangThai = "ChoXacNhan";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã gửi xác nhận thanh toán. Vui lòng chờ Admin kiểm tra tài khoản!";
            }
            return RedirectToAction("Bookings");
        }

        // 4. Xác nhận yêu cầu trả phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout(int bookingId, DateTime NgayTra)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings
                .Include(b => b.Tro)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null) return NotFound();

            // Logic tính toán thời gian ở (cần đủ 30 ngày để hoàn cọc)
            double duration = (NgayTra - booking.NgayNhan).TotalDays;

            booking.TrangThai = "ChoXacNhanTraPhong";

            // Tạo thông báo cho Admin/Chủ trọ
            var notification = new Notification
            {
                UserId = booking.Tro.UserId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Message = $"🔔 Yêu cầu trả phòng: '{booking.Tro.TieuDe}'. Ngày trả: {NgayTra:dd/MM/yyyy}. " +
                          (duration < 30 ? "⚠️ Cảnh báo: Ở chưa đủ 1 tháng (mất cọc)." : "✅ Đã ở đủ trên 1 tháng."),
                Url = "/Admin/Dashboard?section=bookings"
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Yêu cầu trả phòng đã được gửi! Admin sẽ sớm liên hệ xác nhận bàn giao.";
            return RedirectToAction("Bookings");
        }
    }
}