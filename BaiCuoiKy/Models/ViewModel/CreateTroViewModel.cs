using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BaiCuoiKy.Models.ViewModel
{
    public class CreateTroViewModel
    {
        public required string TieuDe { get; set; } = string.Empty; 
        public required string DiaChi { get; set; } = string.Empty;
        public decimal Gia { get; set; }
        public required string MoTa { get; set; } = string.Empty;
        public double DienTich { get; set; }
        public bool TrangThai { get; set; }

        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}