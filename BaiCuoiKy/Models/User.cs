using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace BaiCuoiKy.Models
{
    public class User
    {
        public int Id { get; set; }

        public required string Username { get; set; }
        public required string Password { get; set; }

        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }

        public required string Role { get; set; } // Admin, ChuTro, Khach

        // Quan hệ
        public required ICollection<Tro> Tros { get; set; }
        public required ICollection<Booking> Bookings { get; set; }
        public required ICollection<Review> Reviews { get; set; }
    }
}
