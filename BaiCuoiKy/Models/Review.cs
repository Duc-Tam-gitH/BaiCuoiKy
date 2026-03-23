namespace BaiCuoiKy.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int Rating { get; set; } // 1-5
        public required string Comment { get; set; }
        public DateTime NgayDanhGia { get; set; }

        // FK
        public int UserId { get; set; }
        public required User User { get; set; }

        public int TroId { get; set; }
        public required Tro Tro { get; set; }
    }
}
