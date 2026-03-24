namespace BaiCuoiKy.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }   // ❌ bỏ required

        public int TroId { get; set; }
        public Tro Tro { get; set; }     // ❌ bỏ required
    }
}
