namespace BaiCuoiKy.Models
{
    public class Anhphong
    {
            public int Id { get; set; }

            public string ImageUrl { get; set; }

            public int RoomId { get; set; }
            public Tro Room { get; set; }
    }
}
