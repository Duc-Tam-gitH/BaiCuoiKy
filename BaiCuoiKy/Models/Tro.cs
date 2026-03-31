using System.ComponentModel.DataAnnotations;

namespace BaiCuoiKy.Models
{
    public enum TrangThaiPhong
    {
        DangTrong = 0,   // Đang trống
        DangXuLy = 1,    // Đang xử lý
        DaChoThue = 2    // Đã cho thuê
    }

    public class Tro
    {
        public int Id { get; set; }

        [Required]
        public string TieuDe { get; set; }

        [Required]
        public string MoTa { get; set; }

        public decimal Gia { get; set; }
        public double DienTich { get; set; }

        [Required]
        public string DiaChi { get; set; }

        public DateTime NgayDang { get; set; } = DateTime.Now;

        // ⭐ Trạng thái phòng (thay cho duyệt)
        public TrangThaiPhong TrangThai { get; set; } = TrangThaiPhong.DangTrong;

        // ⭐ Soft delete
        public bool IsDeleted { get; set; } = false;

        // FK User
        public string UserId { get; set; }
        public required ApplicationUser User { get; set; }

        // DANH MỤC
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public int? ViewCount { get; set; } = 0;

        // Quan hệ
        public ICollection<AnhPhong> AnhPhongs { get; set; } = new List<AnhPhong>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}