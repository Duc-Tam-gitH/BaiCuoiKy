using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaiCuoiKy.Models;

namespace BaiCuoiKy.Controllers
{
    public class KhachthueController : Controller
    {
        private readonly AppDbContext _context;

        public KhachthueController(AppDbContext context)
        {
            _context = context;
        }

        // 📌 Danh sách booking
        public IActionResult Bookings()
        {
            var data = _context.Bookings
                .Include(b => b.Tro)
                .Include(b => b.User)
                .ToList();

            return View(data);
        }
    }
}