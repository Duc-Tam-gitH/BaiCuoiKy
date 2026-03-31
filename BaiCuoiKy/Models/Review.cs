using System.ComponentModel.DataAnnotations;
namespace BaiCuoiKy.Models
{

    public class Review
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; }

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int TroId { get; set; }
        public Tro? Tro { get; set; }

        public bool IsHidden { get; set; } = false; // ⭐ thêm
    }
}
