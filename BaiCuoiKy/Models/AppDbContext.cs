using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BaiCuoiKy.Models
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ========================
        // DbSet
        // ========================
        public DbSet<User> Users { get; set; }
        public DbSet<Tro> Rooms { get; set; }
        public DbSet<AnhPhong> RoomImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Favorite> Favorites { get; set; } // 🔥 thêm mới
        public DbSet<Tro> Tros { get; set; }
        public DbSet<AnhPhong> AnhPhongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================
            // Decimal
            // ========================
            modelBuilder.Entity<Tro>()
                .Property(t => t.Gia)
                .HasPrecision(18, 2);

            // ========================
            // RELATIONSHIP
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
           .OnDelete(DeleteBehavior.Restrict);

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
                .HasKey(f => new { f.UserId, f.TroId });

            modelBuilder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade); // hoặc Restrict

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Tro)
                .WithMany(t => t.Favorites)
                .HasForeignKey(f => f.TroId)
                .OnDelete(DeleteBehavior.NoAction); // 🔥 FIX lỗi
        }
    }
}