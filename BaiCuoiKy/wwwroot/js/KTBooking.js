/**
 * Xử lý hiển thị Modal Thanh Toán và tạo mã QR VietQR động
 */
function openPaymentModal(id, amount, roomName) {
    document.getElementById('bookingIdInput').value = id;
    document.getElementById('roomNameDisplay').innerText = "Phòng: " + roomName;

    // Định dạng tiền VNĐ để hiển thị cho người dùng dễ nhìn
    let formattedAmount = new Intl.NumberFormat('vi-VN').format(amount) + " VNĐ";
    document.getElementById('amountDisplay').innerText = formattedAmount;

    // Tự động tạo mã QR VietQR theo thông tin thanh toán của House88
    let bankId = "MB";
    let accountNo = "999988887777";
    let content = "CocPhong_" + id;
    document.getElementById('qrImage').src = `https://img.vietqr.io/image/${bankId}-${accountNo}-compact.png?amount=${amount}&addInfo=${content}`;

    new bootstrap.Modal(document.getElementById('paymentModal')).show();
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