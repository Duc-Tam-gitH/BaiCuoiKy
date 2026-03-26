using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BaiCuoiKy.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Các thuộc tính bổ sung (nếu bạn đã thêm trước đó)
        public string? FullName { get; set; }
        public string? Address { get; set; }

        // 🔥 THÊM CÁC DÒNG NÀY ĐỂ HẾT LỖI TRONG AppDbContext
        // Một User có thể đăng nhiều tin Trọ
        public virtual ICollection<Tro> Tros { get; set; } = new List<Tro>();

        // Một User có thể có nhiều đơn đặt phòng (Booking)
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        // Một User có thể có nhiều đánh giá (Review)
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Một User có thể có nhiều tin yêu thích (Favorite)
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}