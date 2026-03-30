// Biến để kiểm tra form đã thay đổi chưa
let formChanged = false;

$(document).ready(function () {
    // Lắng nghe sự kiện thay đổi trên tất cả input, select, textarea
    $('#createForm input, #createForm select, #createForm textarea').on('change input', function () {
        formChanged = true;
    });

    // Đặc biệt cho file input
    $('input[name="Images"]').on('change', function () {
        formChanged = true;
        previewImages(this);
    });

    // Xử lý nút lưu
    $('#btnSubmit').on('click', function () {
        // Kiểm tra form có hợp lệ không
        if ($('#createForm').valid()) {
            if (formChanged) {
                Swal.fire({
                    title: 'Xác nhận lưu',
                    text: 'Bạn có chắc chắn muốn lưu phòng trọ này?',
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#0d6efd',
                    cancelButtonColor: '#6c757d',
                    confirmButtonText: 'Đồng ý',
                    cancelButtonText: 'Hủy'
                }).then((result) => {
                    if (result.isConfirmed) {
                        $('#createForm').submit();
                    }
                });
            } else {
                Swal.fire({
                    title: 'Không có thay đổi',
                    text: 'Bạn chưa nhập thông tin nào để lưu!',
                    icon: 'info',
                    confirmButtonText: 'Đóng'
                });
            }
        } else {
            Swal.fire({
                title: 'Thông tin chưa hợp lệ',
                text: 'Vui lòng kiểm tra lại các trường dữ liệu!',
                icon: 'error',
                confirmButtonText: 'Đóng'
            });
        }
    });

    // Xử lý nút quay lại
    $('#btnBack').on('click', function () {
        if (formChanged) {
            Swal.fire({
                title: 'Xác nhận rời khỏi',
                text: 'Bạn có thay đổi chưa lưu. Bạn có chắc muốn rời khỏi?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Rời khỏi',
                cancelButtonText: 'Ở lại'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = '@Url.Action("Index", "Tro")';
                }
            });
        } else {
            window.location.href = '@Url.Action("Index", "Tro")';
        }
    });

    // Hàm preview ảnh
    function previewImages(input) {
        var preview = $('#imagePreview');
        preview.empty();

        var files = input.files;
        if (files.length > 0) {
            for (var i = 0; i < files.length; i++) {
                var file = files[i];
                var reader = new FileReader();

                reader.onload = function (e) {
                    var col = $('<div class="col-md-3 mb-2"></div>');
                    var img = $('<img>').attr('src', e.target.result)
                        .addClass('img-thumbnail')
                        .css('height', '150px')
                        .css('object-fit', 'cover');
                    col.append(img);
                    preview.append(col);
                }

                reader.readAsDataURL(file);
            }
        }
    }
});

// Ngăn chặn submit form khi nhấn Enter
$(document).on('keypress', function (e) {
    if (e.which === 13 && e.target.tagName !== 'TEXTAREA') {
        e.preventDefault();
        return false;
    }
});