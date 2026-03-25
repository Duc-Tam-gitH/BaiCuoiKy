namespace BaiCuoiKy.Models
{
    public class AnhPhong
    {
        public int Id { get; set; }

        public required string Url { get; set; }

        // FK
        public int TroId { get; set; }
        public  Tro? Tro { get; set; }
    }
}
