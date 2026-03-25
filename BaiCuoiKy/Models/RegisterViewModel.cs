using System.ComponentModel.DataAnnotations;

namespace BaiCuoiKy.Models.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải từ {2} ký tự trở lên.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        // Thêm trường này để chọn vai trò khi đăng ký
        [Required(ErrorMessage = "Vui lòng chọn vai trò người dùng")]
        [Display(Name = "Bạn đăng ký với tư cách là:")]
        public string SelectedRole { get; set; } // Giá trị sẽ là "Chutro" hoặc "Khachthue"
    }
}