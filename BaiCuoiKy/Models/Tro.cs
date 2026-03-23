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

        public DateTime NgayDang { get; set; }

        // FK
        public int UserId { get; set; }
        public required User User { get; set; }

        // Quan hệ
        public ICollection<AnhPhong> AnhPhongs { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
