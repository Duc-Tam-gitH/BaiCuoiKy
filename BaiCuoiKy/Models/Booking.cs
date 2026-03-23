namespace BaiCuoiKy.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public DateTime NgayDat { get; set; }
        public required string TrangThai { get; set; } // Pending, Approved, Cancel

        // FK
        public int UserId { get; set; }
        public required User User { get; set; }

        public int TroId { get; set; }
        public required Tro Tro { get; set; }
    }
}
