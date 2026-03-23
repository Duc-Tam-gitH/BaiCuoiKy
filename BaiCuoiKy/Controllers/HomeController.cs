using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BaiCuoiKy.Models;
using System.Diagnostics;

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

        // Shows latest approved rooms with images (limit to 20)
        public async Task<IActionResult> Index()
        {
            try
            {
                var danhSachTro = await _context.Tros
                    .Include(t => t.AnhPhongs)
                    .Where(t => t.TrangThai) // only approved listings
                    .OrderByDescending(t => t.NgayDang != default ? t.NgayDang : DateTime.MinValue)
                    .ThenByDescending(t => t.Id)
                    .Take(20)
                    .ToListAsync();

                return View(danhSachTro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load home index data");
                // Optionally return an empty list so the view can render gracefully
                return View(new List<Tro>());
            }
        }

        // Use conventional "Details" name to match common routing / views
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tro = await _context.Tros
                .Include(t => t.AnhPhongs)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (tro == null) return NotFound();

            return View(tro);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}