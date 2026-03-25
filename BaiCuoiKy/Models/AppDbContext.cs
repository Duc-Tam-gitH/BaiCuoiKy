using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BaiCuoiKy.Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Tro> Tros { get; set; }
        public DbSet<AnhPhong> AnhPhongs { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tro>()
                .Property(t => t.Gia)
                .HasPrecision(18, 2);

            // Tro - User
            modelBuilder.Entity<Tro>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tros)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AnhPhong
            modelBuilder.Entity<AnhPhong>()
                .HasOne(a => a.Tro)
                .WithMany(t => t.AnhPhongs)
                .HasForeignKey(a => a.TroId);

            // Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Tro)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TroId);

            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Tro)
                .WithMany(t => t.Reviews)
                .HasForeignKey(r => r.TroId);

            // Favorite (N-N)
            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.UserId, f.TroId });

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Tro)
                .WithMany(t => t.Favorites)
                .HasForeignKey(f => f.TroId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}