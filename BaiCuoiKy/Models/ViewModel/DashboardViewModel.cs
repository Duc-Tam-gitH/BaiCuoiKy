using System.Collections.Generic;

namespace BaiCuoiKy.Models.ViewModel
{
    public class DashboardViewModel
    {
        public string UserName { get; set; }
        public string UserInitials { get; set; }
        public string UserRole { get; set; }
        public string ActiveSection { get; set; }
        public DashboardStatistics Statistics { get; set; }
        public List<RoomViewModel> RecentRooms { get; set; }
        public List<UserViewModel> RecentUsers { get; set; }
        public List<PostViewModel> RecentPosts { get; set; }
        public List<ActivityViewModel> RecentActivities { get; set; }
    }

    public class DashboardStatistics
    {
        // Thống kê phòng trọ
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int RentedRooms { get; set; }
        public int MaintenanceRooms { get; set; }

        // Thống kê người dùng
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedUsers { get; set; }
        public int NewUsersThisMonth { get; set; }

        // Thống kê bài đăng
        public int TotalPosts { get; set; }
        public int PublishedPosts { get; set; }
        public int PendingPosts { get; set; }
        public int ExpiredPosts { get; set; }

        // Thống kê booking
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
    }

    public class RoomViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Area { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public string LandlordName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsLocked { get; set; }
        public List<string> Roles { get; set; }
    }

    public class PostViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
    }

    public class ActivityViewModel
    {
        public string Description { get; set; }
        public string TimeAgo { get; set; }
        public string Icon { get; set; }
        public string Type { get; set; }
    }
}
