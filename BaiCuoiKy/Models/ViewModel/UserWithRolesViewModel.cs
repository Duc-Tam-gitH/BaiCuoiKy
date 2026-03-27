using System.Collections.Generic;


namespace BaiCuoiKy.Models.ViewModel
{
    public class UserWithRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int TotalTros { get; set; }
        public int TotalBookings { get; set; }
        public bool IsLocked { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsLocked { get; set; }
        

        public List<string> Roles { get; set; } = new List<string>();
        public List<string> UserRoles { get; set; } = new List<string>();
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}
