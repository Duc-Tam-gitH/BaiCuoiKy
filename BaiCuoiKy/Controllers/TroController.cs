using BaiCuoiKy.Models;
using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.IO;

namespace BaiCuoiKy.Controllers
{
    public class TroController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TroController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== CREATE ====================

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var model = new CreateTroViewModel
            {
                TieuDe = string.Empty,
                DiaChi = string.Empty,
                MoTa = string.Empty,
                Gia = 0,
                DienTich = 0,
                TrangThai = true,
                CategoryId = null,
                Images = new List<IFormFile>(),
                Categories = await _context.Categories
                    .Where(c => c.TrangThai == true)
                    .OrderBy(c => c.ThuTu)
                    .ToListAsync()
            };

            return View("CreateRoom", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateTroViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin người dùng!";
                    return RedirectToAction("Index", "Home");
                }

                var tro = new Tro
                {
                    TieuDe = model.TieuDe,
                    DiaChi = model.DiaChi,
                    Gia = model.Gia,
                    MoTa = model.MoTa,
                    DienTich = model.DienTich,
                    UserId = userId,
                    User = user,
                    TrangThai = model.TrangThai,
                    CategoryId = model.CategoryId,
                    NgayDang = DateTime.Now,
                    AnhPhongs = new List<AnhPhong>()
                };

                _context.Tros.Add(tro);
                await _context.SaveChangesAsync();

                // Xử lý upload ảnh
                if (model.Images != null && model.Images.Any())
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "rooms");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var image in model.Images)
                    {
                        if (image.Length > 0)
                        {
                            var fileName = $"{tro.Id}_{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            _context.AnhPhongs.Add(new AnhPhong
                            {
                                TroId = tro.Id,
                                Url = $"/images/rooms/{fileName}"
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Thêm phòng trọ thành công!";
                return RedirectToAction("Manage");  // ✅ Sửa
            }

            model.Categories = await _context.Categories
                .Where(c => c.TrangThai == true)
                .OrderBy(c => c.ThuTu)
                .ToListAsync();

            return View("CreateRoom", model);
        }

        // ==================== READ (QUẢN LÝ) ====================

        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var Admin = await _context.Tros
                .Include(t => t.User)
                .Include(t => t.AnhPhongs)
                .Include(t => t.Category)
                .OrderByDescending(t => t.NgayDang)
                .ToListAsync();

            return View("ManageSystem", Admin);  // ✅ Dùng view Index.cshtml
        }

        // ==================== READ (CHI TIẾT) ====================

        public async Task<IActionResult> Details(int id)
        {
            var tro = await _context.Tros
                .Include(t => t.User)
                .Include(t => t.AnhPhongs)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tro == null)
            {
                return NotFound();
            }

            return View(tro);
        }

        // ==================== UPDATE ====================

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var tro = await _context.Tros
                .Include(t => t.AnhPhongs)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tro == null)
            {
                return NotFound();
            }

            var model = new EditTroViewModel
            {
                Id = tro.Id,
                TieuDe = tro.TieuDe,
                DiaChi = tro.DiaChi,
                Gia = tro.Gia,
                MoTa = tro.MoTa,
                DienTich = tro.DienTich,
                TrangThai = tro.TrangThai,
                CategoryId = tro.CategoryId,
                ExistingImages = tro.AnhPhongs?.Select(a => a.Url).ToList() ?? new List<string>(),
                Categories = await _context.Categories
                    .Where(c => c.TrangThai == true)
                    .OrderBy(c => c.ThuTu)
                    .ToListAsync()
            };

            return View("EditRoom", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(EditTroViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tro = await _context.Tros
                    .Include(t => t.AnhPhongs)
                    .FirstOrDefaultAsync(t => t.Id == model.Id);

                if (tro == null)
                {
                    return NotFound();
                }

                tro.TieuDe = model.TieuDe;
                tro.DiaChi = model.DiaChi;
                tro.Gia = model.Gia;
                tro.MoTa = model.MoTa;
                tro.DienTich = model.DienTich;
                tro.TrangThai = model.TrangThai;
                tro.CategoryId = model.CategoryId;

                // Xử lý xóa ảnh
                if (model.DeletedImages != null && model.DeletedImages.Any())
                {
                    var imagesToDelete = tro.AnhPhongs
                        .Where(a => model.DeletedImages.Contains(a.Url))
                        .ToList();

                    foreach (var img in imagesToDelete)
                    {
                        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.Url.TrimStart('/'));
                        if (System.IO.File.Exists(physicalPath))
                        {
                            System.IO.File.Delete(physicalPath);
                        }
                        _context.AnhPhongs.Remove(img);
                    }
                }

                // Xử lý upload ảnh mới
                if (model.NewImages != null && model.NewImages.Any())
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "rooms");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var image in model.NewImages)
                    {
                        if (image.Length > 0)
                        {
                            var fileName = $"{tro.Id}_{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            _context.AnhPhongs.Add(new AnhPhong
                            {
                                TroId = tro.Id,
                                Url = $"/images/rooms/{fileName}"
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật phòng trọ thành công!";
                return RedirectToAction("Manage");  // ✅ Sửa
            }

            model.Categories = await _context.Categories
                .Where(c => c.TrangThai == true)
                .OrderBy(c => c.ThuTu)
                .ToListAsync();

            return View("EditRoom", model);
        }

        // ==================== DELETE ====================

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var tro = await _context.Tros
                .Include(t => t.AnhPhongs)
                .Include(t => t.Bookings)
                .Include(t => t.Reviews)
                .Include(t => t.Favorites)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tro == null)
            {
                return NotFound();
            }

            return View(tro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tro = await _context.Tros
                .Include(t => t.AnhPhongs)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tro == null)
            {
                return NotFound();
            }

            if (tro.AnhPhongs != null && tro.AnhPhongs.Any())
            {
                foreach (var anhPhong in tro.AnhPhongs)
                {
                    var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", anhPhong.Url.TrimStart('/'));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }
            }

            _context.Tros.Remove(tro);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa phòng trọ thành công!";
            return RedirectToAction("Manage");  // ✅ Sửa
        }
    }
}