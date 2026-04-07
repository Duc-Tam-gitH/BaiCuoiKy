// ==========================================
// Gallery functions
// ==========================================
function changeMainImage(url) {
    document.getElementById('mainRoomImage').src = url;
}

function showImageModal(url) {
    document.getElementById('modalImage').src = url;
    new bootstrap.Modal(document.getElementById('imageModal')).show();
}

// ==========================================
// User actions
// ==========================================
function contactOwner() {
    const phone = '@(Model.User?.PhoneNumber ?? "")';
    if (phone) {
        window.location.href = `tel:${phone}`;
    } else {
        toastr.warning('Chủ trọ chưa cập nhật số điện thoại');
    }
}

function bookRoom(roomId) {
    window.location.href = window.bookingUrl + '?roomId=' + roomId;
}

// Xử lý nút Yêu thích (Thả tim)
function toggleFavorite(btn) {
    console.log("Đã bấm nút thả tim!"); // In ra để test xem hàm có chạy không

    const troId = btn.getAttribute("data-id");
    const url = btn.getAttribute("data-url"); // Lấy link chuẩn từ HTML
    const icon = btn.querySelector("i");

    // Thêm hiệu ứng loading
    icon.classList.remove("fa-heart", "far", "fas");
    icon.classList.add("fa-spinner", "fa-spin", "fas");

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: `troId=${troId}`
    })
        .then(response => response.json())
        .then(data => {
            // Xóa loading, trả lại icon tim
            icon.classList.remove("fa-spinner", "fa-spin");
            icon.classList.add("fa-heart");

            if (data.success) {
                if (data.isFavorited) {
                    btn.classList.add("active");
                    icon.classList.remove("far");
                    icon.classList.add("fas");
                } else {
                    btn.classList.remove("active");
                    icon.classList.remove("fas");
                    icon.classList.add("far");
                }
            } else {
                alert("Có lỗi xảy ra, vui lòng thử lại sau!");
            }
        })
        .catch(error => {
            console.error('Lỗi khi fetch:', error);
            icon.classList.remove("fa-spinner", "fa-spin");
            icon.classList.add("fa-heart", "far");
        });
}

// ==========================================
// Admin actions
// ==========================================
function approveRoom(id) {
    if (confirm('Xác nhận duyệt bài đăng này?')) {
        document.getElementById('approveRoomId').value = id;
        document.getElementById('approveForm').submit();
    }
}

let currentRoomId = 0;

function showRejectModal(id, title) {
    currentRoomId = id;
    document.getElementById('rejectRoomId').value = id;
    new bootstrap.Modal(document.getElementById('rejectModal')).show();
}

function confirmReject() {
    const reason = document.getElementById('rejectReason').value;
    document.getElementById('rejectRoomReason').value = reason;
    document.getElementById('rejectForm').submit();
}

function hideRoom(id) {
    if (confirm('Xác nhận gỡ duyệt bài đăng này?')) {
        document.getElementById('hideRoomId').value = id;
        document.getElementById('hideRoomForm').submit();
    }
}

function deleteRoom(id, title) {
    if (confirm(`Bạn có chắc chắn muốn xóa phòng trọ "${title}"? Hành động này không thể hoàn tác!`)) {
        const form = document.createElement('form');
        form.method = 'post';
        form.action = '@Url.Action("DeleteRoom", "Admin")';
        form.innerHTML = `
            <input type="hidden" name="id" value="${id}" />
            @Html.AntiForgeryToken()
        `;
        document.body.appendChild(form);
        form.submit();
    }
}

function updateStatus(id, status) {
    fetch('/Admin/UpdateTrangThai', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            id: id,
            trangThai: parseInt(status)
        })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                alert("Cập nhật thành công!");
                location.reload();
            }
        });
}

// ==========================================
// Toast messages
// ==========================================
if (window.successMessage) toastr.success(window.successMessage);
if (window.errorMessage) toastr.error(window.errorMessage);