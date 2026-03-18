namespace BaiCuoiKy.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public DateTime BookingDate { get; set; }

        public string Status { get; set; } // Pending, Approved, Rejected

        // Foreign key
        public int UserId { get; set; }
        public User User { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }
    }
}
