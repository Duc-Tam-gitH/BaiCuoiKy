using Microsoft.EntityFrameworkCore;

namespace BaiCuoiKy.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tro> Rooms { get; set; }
        public DbSet<Anhphong> RoomImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Fix lỗi decimal Price
            modelBuilder.Entity<Tro>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);

            // (OPTIONAL) Có thể thêm config khác sau này ở đây
        }
    }
}