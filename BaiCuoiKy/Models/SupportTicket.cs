using System.ComponentModel.DataAnnotations;

namespace BaiCuoiKy.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Trạng thái xử lý: false = Chưa xử lý, true = Đã xử lý
        public bool IsResolved { get; set; } = false;
    }
}