using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaiCuoiKy.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;
        public DateTime NgayNhan { get; set; }

        // 🟢 GIỮ NGUYÊN KIỂU STRING ĐỂ KHÔNG LỖI CODE CŨ
        // Chúng ta sẽ dùng các mốc chữ: "ChoDuyet", "ChoThanhToan", "ChoXacNhan", "HoanTat", "DaHuy"
        public string TrangThai { get; set; } = "ChoDuyet";

        // 🔥 CỘT MỚI: DÙNG ĐỂ LƯU SỐ TIỀN KHÁCH CẦN CỌC (20%)
        public decimal TienCoc { get; set; }

        // FK
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public int TroId { get; set; }
        [ForeignKey("TroId")]
        public virtual Tro Tro { get; set; }
    }
}