using Microsoft.AspNetCore.Http;

namespace BaiCuoiKy.Models.ViewModel
{
    public class EditTroViewModel
    {
        public int Id { get; set; }

        public required string TieuDe { get; set; }

        public required string DiaChi { get; set; }

        public decimal Gia { get; set; }

        public required string MoTa { get; set; }

        public double DienTich { get; set; }

        public bool TrangThai { get; set; }

        public int? CategoryId { get; set; }

        // Danh sách ảnh hiện có
        public List<string> ExistingImages { get; set; } = new List<string>();

        // Danh sách ảnh cần xóa
        public List<string> DeletedImages { get; set; } = new List<string>();

        // Ảnh mới upload
        public List<IFormFile> NewImages { get; set; } = new List<IFormFile>();

        // Danh sách danh mục để hiển thị dropdown
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}