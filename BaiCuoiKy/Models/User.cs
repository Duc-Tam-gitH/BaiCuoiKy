using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace BaiCuoiKy.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Role { get; set; } // "Customer" | "Owner" 

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Tro> Rooms { get; set; } // Chủ trọ đăng
        public ICollection<Review> Reviews { get; set; }
    }
}
