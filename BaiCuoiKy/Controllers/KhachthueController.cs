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

        // Action hiển thị danh sách yêu thích
        public async Task<IActionResult> Favorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Tro)
                    .ThenInclude(t => t.AnhPhongs) // Lấy ảnh để hiển thị trên Card
                .Select(f => f.Tro)
                .ToListAsync();

            return View(favorites);
        }
    }
}