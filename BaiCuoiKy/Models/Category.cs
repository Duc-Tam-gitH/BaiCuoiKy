using System.ComponentModel.DataAnnotations;

namespace BaiCuoiKy.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [Display(Name = "Tên danh mục")]
        public string TenDanhMuc { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int ThuTu { get; set; } = 0;

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation property
        public virtual ICollection<Tro> Tros { get; set; } = new List<Tro>();
    }
}