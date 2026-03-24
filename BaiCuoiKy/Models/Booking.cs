using System;

namespace BaiCuoiKy.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;
        public DateTime NgayNhan { get; set; }

        public string TrangThai { get; set; } = "ChoDuyet";
        

        // FK
        public int UserId { get; set; }
        public User User { get; set; }

        public int TroId { get; set; }
        public Tro Tro { get; set; }
    }
}