namespace BaiCuoiKy.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public required User User { get; set; }

        public int TroId { get; set; }
        public required Tro Tro { get; set; }
    }
}
