namespace BaiCuoiKy.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int Rating { get; set; } // 1 - 5

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign key
        public int UserId { get; set; }
        public User User { get; set; }

        public int RoomId { get; set; }
        public Tro Room { get; set; }
    }
}
