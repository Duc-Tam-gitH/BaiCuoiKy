using Microsoft.AspNetCore.Http;

namespace BaiCuoiKy.Models.ViewModel
{
    public class CreateTroViewModel
    {
        public required string TieuDe { get; set; }
        public required string DiaChi { get; set; }
        public decimal Gia { get; set; }
        public required string MoTa { get; set; }
        public double DienTich { get; set; }

        // Upload ảnh
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}