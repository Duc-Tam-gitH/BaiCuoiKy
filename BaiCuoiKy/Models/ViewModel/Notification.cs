namespace BaiCuoiKy.Models // Kiểm tra kỹ dòng namespace này
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}