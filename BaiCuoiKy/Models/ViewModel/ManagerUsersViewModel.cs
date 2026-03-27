using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace BaiCuoiKy.Models.ViewModel
{
    public class ManagerUsersViewModel
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

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Khóa tài khoản")]
        public bool IsLocked { get; set; }

        public List<string> Roles { get; set; }
        public List<string> UserRoles { get; set; }
        public List<string> SelectedRoles { get; set; }
    }
}
