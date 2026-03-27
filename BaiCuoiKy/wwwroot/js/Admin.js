$(document).ready(function () {
    // Xử lý đăng xuất
    $('#logoutButton').click(function () {
        if (confirm('Bạn có chắc chắn muốn đăng xuất khỏi hệ thống?')) {
            $.ajax({
                url: '/Admin/Logout',
                type: 'POST',
                success: function (response) {
                    if (response.success) {
                        alert(response.message);
                        // Chuyển hướng về trang login
                        window.location.href = '/Account/Login';
                    }
                },
                error: function () {
                    alert('Có lỗi xảy ra, vui lòng thử lại');
                }
            });
        }
    });

    // Thêm hiệu ứng active cho menu khi click (hỗ trợ cho việc load AJAX nếu cần)
    $('.nav-item').click(function () {
        $('.nav-item').removeClass('active');
        $(this).addClass('active');
    });

    // Tooltip cho các nút nếu cần
    $('.logout-icon').tooltip({
        title: 'Đăng xuất',
        placement: 'left'
    });
});

// Hàm cập nhật tên người dùng (có thể gọi từ nơi khác)
function updateUserDisplay(newName, newInitials) {
    $('#avatarInitials').text(newInitials);
    $('.user-name').text(newName);
    alert(`Đã cập nhật thông tin người dùng: ${newName}`);
}