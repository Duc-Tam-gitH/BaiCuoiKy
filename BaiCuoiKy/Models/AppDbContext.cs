using BaiCuoiKy.Models.ViewModel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BaiCuoiKy.Models
{
    // Cần truyền ApplicationUser vào để Identity biết dùng Model tùy chỉnh của bạn
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ========================
        // DbSet
        // ========================

        // Lưu ý: Không cần khai báo public DbSet<ApplicationUser> Users 
        // vì IdentityDbContext đã có sẵn tập hợp Users rồi.

        public DbSet<Tro> Tros { get; set; }
        public DbSet<AnhPhong> AnhPhongs { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Category> Categories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Bắt buộc phải có dòng này đầu tiên

            // ========================
            // Cấu hình kiểu dữ liệu
            // ========================
            modelBuilder.Entity<Tro>()
                .Property(t => t.Gia)
                .HasPrecision(18, 2);

            // ========================
            // RELATIONSHIPS (QUAN HỆ)
            // ========================

            // User - Tro (1 - N)
            modelBuilder.Entity<Tro>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tros)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tro - AnhPhong (1 - N)
            modelBuilder.Entity<AnhPhong>()
                .HasOne(a => a.Tro)
                .WithMany(t => t.AnhPhongs)
                .HasForeignKey(a => a.TroId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Booking (1 - N)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Dùng Restrict để tránh lỗi Multiple Cascade Path

            // Tro - Booking (1 - N)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Tro)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TroId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Review (1 - N)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tro - Review (1 - N)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Tro)
                .WithMany(t => t.Reviews)
                .HasForeignKey(r => r.TroId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========================
            // Favorite (N - N)
            // ========================
            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.UserId, f.TroId }); // Khóa chính hỗn hợp

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Tro)
                .WithMany(t => t.Favorites)
                .HasForeignKey(f => f.TroId)
                .OnDelete(DeleteBehavior.NoAction); // FIX lỗi Multiple Cascade Path trong SQL Server
            // ========================
            // Tro - Category (N - 1)
            // Thiết lập quan hệ giữa Tro và Category
            modelBuilder.Entity<Tro>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tros)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}