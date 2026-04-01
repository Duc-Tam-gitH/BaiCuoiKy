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
        public async Task<IActionResult> Bookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách các yêu cầu thuê phòng của User hiện tại
            var myBookings = await _context.Bookings
                .Include(b => b.Tro) // Để lấy tên phòng, giá phòng
                    .ThenInclude(t => t.AnhPhongs)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.NgayDat)
                .ToListAsync();

            return View(myBookings); // Sẽ tìm file Views/Khachthue/Bookings.cshtml
        }
    }
}