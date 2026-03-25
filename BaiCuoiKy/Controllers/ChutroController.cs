using BaiCuoiKy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BaiCuoiKy.Controllers
{
    // Chỉ cho phép người dùng có Role "Chutro" hoặc "Admin" vào đây
    [Authorize(Roles = "Chutro,Admin")]
    public class ChutroController : Controller
    {
        private readonly AppDbContext _context;

        public ChutroController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // TRANG DANH SÁCH PHÒNG CỦA TÔI
        // ==========================================
        public async Task<IActionResult> MyRooms()
        {
            // Lấy ID người dùng hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Chỉ hiển thị những phòng do chủ này đăng
            var list = await _context.Tros
                .Include(t => t.AnhPhongs)
                .Where(t => t.UserId.ToString() == userId)
                .ToListAsync();

            return View(list);
        }

        // ==========================================
        // TRANG ĐĂNG TIN (GET)
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // XỬ LÝ LƯU TIN ĐĂNG (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tro tro, List<IFormFile> files)
        {
            // 1. Lấy và gán UserId từ Identity
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                tro.UserId = userId;
            }

            // 2. Làm sạch ModelState để không bị lỗi Logic ràng buộc
            ModelState.Remove("User");
            ModelState.Remove("AnhPhongs");
            ModelState.Remove("Bookings");
            ModelState.Remove("Reviews");
            ModelState.Remove("Favorites");

            if (ModelState.IsValid)
            {
                try
                {
                    // 3. Lưu thông tin phòng trước
                    _context.Add(tro);
                    await _context.SaveChangesAsync();

                    // 4. Xử lý Upload ảnh (nếu có)
                    if (files != null && files.Count > 0)
                    {
                        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        if (!Directory.Exists(webRootPath)) Directory.CreateDirectory(webRootPath);

                        foreach (var file in files)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string uploadPath = Path.Combine(webRootPath, fileName);

                            using (var stream = new FileStream(uploadPath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            // Thêm ảnh vào Database
                            _context.AnhPhongs.Add(new AnhPhong
                            {
                                Url = "/images/" + fileName,
                                TroId = tro.Id,
                                Tro = tro // Giải quyết lỗi CS9035 'required'
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(MyRooms));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }
            return View(tro);
        }
    }
}