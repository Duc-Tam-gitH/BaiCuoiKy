namespace BaiCuoiKy.Models
{
    public class Tro
    {
        public int Id { get; set; }

        public required string TieuDe { get; set; }
        public required string MoTa { get; set; }

        public decimal Gia { get; set; }
        public double DienTich { get; set; }

        public required string DiaChi { get; set; }

        public bool TrangThai { get; set; } // true = đã duyệt

        public DateTime NgayDang { get; set; } = DateTime.Now;

        // FK
        public string UserId { get; set; }
        public required ApplicationUser User { get; set; }

        // 🔥 THÊM DANH MỤC
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        // Quan hệ
        public ICollection<AnhPhong> AnhPhongs { get; set; } = new List<AnhPhong>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    }
}