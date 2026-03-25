using Microsoft.AspNetCore.Identity;

namespace BaiCuoiKy.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Giữ lại FullName vì IdentityUser không có sẵn trường này
        public required string FullName { get; set; }

       
        // Quan hệ
        public ICollection<Tro> Tros { get; set; } = new List<Tro>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
