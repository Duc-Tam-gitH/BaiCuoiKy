using Microsoft.AspNetCore.Http;
using BaiCuoiKy.Models;

namespace BaiCuoiKy.Models.ViewModel
{
    public class EditTroViewModel
    {
        public int Id { get; set; }

        public required string TieuDe { get; set; }

        public required string DiaChi { get; set; }
        public string KhuVuc { get; set; }

        public decimal Gia { get; set; }

        public required string MoTa { get; set; }

        public double DienTich { get; set; }

        
        public TrangThaiPhong TrangThai { get; set; }

        public int? CategoryId { get; set; }

        // Ảnh hiện có
        public List<string> ExistingImages { get; set; } = new List<string>();

        // Ảnh cần xóa
        public List<string> DeletedImages { get; set; } = new List<string>();

        // Ảnh mới
        public List<IFormFile> NewImages { get; set; } = new List<IFormFile>();

        // Danh mục
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}