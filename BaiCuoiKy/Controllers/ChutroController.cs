using BaiCuoiKy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BaiCuoiKy.Controllers
{
    [Authorize(Roles = "ChuTro,Admin")]
    public class ChutroController : Controller
    {
        private readonly AppDbContext _context;

        public ChutroController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DANH SÁCH PHÒNG CỦA TÔI
        // ==========================================
        public async Task<IActionResult> MyRooms()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var list = await _context.Tros
                .Include(t => t.AnhPhongs)
                .Where(t => t.UserId == userId) // ✅ FIX
                .ToListAsync();

            return View(list);
        }

        // ==========================================
        // GET: TẠO PHÒNG
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // POST: TẠO PHÒNG
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tro tro, List<IFormFile> files)
        {
            // ✅ FIX: Gán trực tiếp string UserId
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            tro.UserId = userId;

            // Làm sạch ModelState
            ModelState.Remove("User");
            ModelState.Remove("AnhPhongs");
            ModelState.Remove("Bookings");
            ModelState.Remove("Reviews");
            ModelState.Remove("Favorites");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lưu phòng
                    _context.Tros.Add(tro);
                    await _context.SaveChangesAsync();

                    // Upload ảnh
                    if (files != null && files.Count > 0)
                    {
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        foreach (var file in files)
                        {
                            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(folderPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            _context.AnhPhongs.Add(new AnhPhong
                            {
                                Url = "/images/" + fileName,
                                TroId = tro.Id
                                // ❌ KHÔNG cần Tro = tro nữa
                            });
                        }

                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(MyRooms));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            return View(tro);
        }
    }
}