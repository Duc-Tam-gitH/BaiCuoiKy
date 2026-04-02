using BaiCuoiKy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaiCuoiKy.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== 1. HIỂN THỊ DANH SÁCH ====================
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();

            // Trỏ đúng về file Category.cshtml trong thư mục Home (vì bạn đang lưu file ở đó)
            return View("~/Views/Home/Category.cshtml", categories);
        }

        // ==================== 2. THÊM DANH MỤC ====================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCategory(string TenDanhMuc, string MoTa)
        {
            if (!string.IsNullOrWhiteSpace(TenDanhMuc))
            {
                var newCategory = new Category
                {
                    TenDanhMuc = TenDanhMuc,
                    MoTa = MoTa,
                    TrangThai = true
                };

                _context.Categories.Add(newCategory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm danh mục mới thành công!";
            }
            return RedirectToAction("Index"); // Đổi thành "Index" của CategoryController
        }

        // ==================== 3. CẬP NHẬT DANH MỤC ====================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int Id, string TenDanhMuc, string MoTa)
        {
            var category = await _context.Categories.FindAsync(Id);

            if (category != null && !string.IsNullOrWhiteSpace(TenDanhMuc))
            {
                category.TenDanhMuc = TenDanhMuc;
                category.MoTa = MoTa;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
            }
            return RedirectToAction("Index");
        }

        // ==================== 4. XÓA DANH MỤC ====================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa danh mục thành công!";
            }
            return RedirectToAction("Index");
        }
    }
}