using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace BaiCuoiKy.Models
{
    public class Tro
    {
            public int Id { get; set; }

            public string Title { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public decimal Price { get; set; }

            public double Area { get; set; }

            public string Address { get; set; } = string.Empty;

            public string District { get; set; } = string.Empty;

            public string City { get; set; } = string.Empty;

            public bool IsAvailable { get; set; } = true;

            public DateTime CreatedAt { get; set; } = DateTime.Now;

            // Foreign key
            public int OwnerId { get; set; }
            public User Owner { get; set; }

            // Navigation
            public ICollection<Anhphong> Images { get; set; }
            public ICollection<Review> Reviews { get; set; }
    }
}
