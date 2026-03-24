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

        public required string Role { get; set; } // (nâng cấp sau -> enum)

        // Quan hệ
        public ICollection<Tro> Tros { get; set; } = new List<Tro>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
