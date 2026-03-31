
        // Gallery functions
    function changeMainImage(url) {
        document.getElementById('mainRoomImage').src = url;
        }

    function showImageModal(url) {
        document.getElementById('modalImage').src = url;
    new bootstrap.Modal(document.getElementById('imageModal')).show();
        }

    // User actions
    function contactOwner() {
            const phone = '@(Model.User?.PhoneNumber ?? "")';
    if (phone) {
        window.location.href = `tel:${phone}`;
            } else {
        toastr.warning('Chủ trọ chưa cập nhật số điện thoại');
            }
        }

    function bookRoom(roomId) {
        window.location.href = '@Url.Action("Create", "Booking")?roomId=' + roomId;
        }

    // Admin actions
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

    // Toast messages
    @if (TempData["Success"] != null)
    {
        <text>toastr.success('@TempData["Success"]');</text>
    }
    @if (TempData["Error"] != null)
    {
        <text>toastr.error('@TempData["Error"]');</text>
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