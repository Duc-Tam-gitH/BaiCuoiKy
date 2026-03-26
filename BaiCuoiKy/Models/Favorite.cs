namespace BaiCuoiKy.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }   // ❌ bỏ required

        public int TroId { get; set; }
        public Tro Tro { get; set; }     // ❌ bỏ required
    }
}
