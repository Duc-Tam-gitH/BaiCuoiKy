using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace BaiCuoiKy.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Phone { get; set; }

        public string Role { get; set; } // "Customer" | "Owner"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Room> Rooms { get; set; } // Chủ trọ đăng
        public ICollection<Review> Reviews { get; set; }
    }
}
