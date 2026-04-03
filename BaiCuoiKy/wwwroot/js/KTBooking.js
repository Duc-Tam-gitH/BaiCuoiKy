/**
 * Xử lý hiển thị Modal Thanh Toán và tạo mã QR VietQR động
 */
function openPaymentModal(bookingId, roomName, amount) {
    // 1. Chỉ gán chữ (Tên phòng và Số tiền)
    document.getElementById('roomNameDisplay').innerText = roomName;
    document.getElementById('amountDisplay').innerText = amount.toLocaleString('vi-VN') + " VNĐ";

    // 2. Gán ID đơn hàng
    document.getElementById('bookingIdInput').value = bookingId;

    // TUYỆT ĐỐI KHÔNG CÓ LỆNH GÁN ẢNH qrImage.src Ở ĐÂY NỮA NHÉ!

    // 3. Mở Modal
    var myModal = new bootstrap.Modal(document.getElementById('paymentModal'));
    myModal.show();
}

/**
 * Mở Modal Trả Phòng và thiết lập các giá trị ngày mặc định
 */
function openCheckoutModal(id, checkinDate, roomName) {
    document.getElementById('checkoutBookingId').value = id;
    document.getElementById('checkoutRoomName').innerText = roomName;
    document.getElementById('checkinDateHidden').value = checkinDate;

    // Thiết lập ngày trả mặc định là ngày hiện tại
    let today = new Date().toISOString().split('T')[0];
    document.getElementById('checkoutDateInput').value = today;
    document.getElementById('checkoutDateInput').min = checkinDate;

    checkStayDuration(); // Chạy kiểm tra ngay khi mở để hiển thị thông báo
    new bootstrap.Modal(document.getElementById('checkoutModal')).show();
}

/**
 * Tính toán thời gian ở và hiển thị cảnh báo nếu chưa ở đủ 30 ngày (logic mất cọc)
 */
function checkStayDuration() {
    let checkin = new Date(document.getElementById('checkinDateHidden').value);
    let checkout = new Date(document.getElementById('checkoutDateInput').value);
    let notice = document.getElementById('checkoutNotice');

    // Tính toán số ngày chênh lệch (làm tròn lên)
    let diffDays = Math.ceil(Math.abs(checkout - checkin) / (1000 * 60 * 60 * 24));

    notice.classList.remove('d-none', 'alert-warning', 'alert-success');

    if (diffDays < 30) {
        notice.classList.add('alert-warning');
        notice.innerHTML = `<i class="fas fa-exclamation-triangle me-2"></i> Bạn mới ở được <strong>${diffDays} ngày</strong>. Vì chưa đủ 30 ngày, bạn sẽ <strong>mất tiền cọc</strong> theo quy định.`;
    } else {
        notice.classList.add('alert-success');
        notice.innerHTML = `<i class="fas fa-check-circle me-2"></i> Bạn đã ở đủ <strong>${diffDays} ngày</strong>. Bạn đủ điều kiện để nhận lại tiền cọc.`;
    }
}