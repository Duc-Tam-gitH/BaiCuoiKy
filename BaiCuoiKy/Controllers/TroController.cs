using BaiCuoiKy.Models;
using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.IO;  // Thêm using này cho Path, Directory, FileStream

namespace BaiCuoiKy.Controllers
{
    public class TroController : Controller
    {
        private readonly AppDbContext _context;  // Thêm dòng này - cần inject context

        // Constructor - thêm vào
        public TroController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị form thêm phòng trọ
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Xử lý thêm phòng trọ
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateTroViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var tro = new Tro  
                {
                    TieuDe = model.TieuDe,
                    DiaChi = model.DiaChi,
                    Gia = model.Gia,
                    MoTa = model.MoTa,
                    DienTich = model.DienTich,
                    UserId = userId,
                    TrangThai = false, // Mặc định chờ duyệt
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

                            var anhPhong = new AnhPhong
                            {
                                TroId = tro.Id,
                                Url = $"/images/rooms/{fileName}"
                            };

                            _context.AnhPhongs.Add(anhPhong);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Thêm phòng trọ thành công!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // Thêm action Index để hiển thị danh sách
        public async Task<IActionResult> Index()
        {
            var tros = await _context.Tros
                .Include(t => t.User)
                .Include(t => t.AnhPhongs)
                .OrderByDescending(t => t.NgayDang)
                .ToListAsync();
            return View(tros);
        }
    }
}