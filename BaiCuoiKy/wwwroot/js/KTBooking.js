/**
 * Xử lý hiển thị Modal Thanh Toán và tạo mã QR VietQR động
 */
function openPaymentModal(bookingId, amount, roomName) {
    // 1. Gán ID booking và hiển thị tên phòng/số tiền (giữ nguyên logic cũ của bạn nếu có)
    document.getElementById('bookingIdInput').value = bookingId;
    document.getElementById('roomNameDisplay').innerText = roomName;
    document.getElementById('amountDisplay').innerText = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);

    // 2. CẤU HÌNH THÔNG TIN NGÂN HÀNG CỦA BẠN (HOẶC ADMIN)
    const bankId = "MB"; // Mã ngân hàng (VD: MB, VCB, TCB, ACB, TPB...)
    const accountNo = "0932096623"; // Số tài khoản nhận tiền
    const accountName = "NGUYEN PHAM DUC TAM"; // Tên chủ tài khoản (Viết hoa không dấu)
    const template = "compact2"; // Giao diện QR (compact, compact2, print)

    // 3. Xử lý dữ liệu để đưa vào URL
    // Chuyển số tiền thành chuỗi số nguyên chuẩn (xóa dấu phẩy/chấm nếu có)
    const cleanAmount = String(amount).replace(/[^0-9]/g, '');

    // Tạo nội dung chuyển khoản (Bỏ dấu tiếng Việt để ngân hàng không bị lỗi font)
    let description = `Coc phong ${roomName} mã ${bookingId}`;
    description = description.normalize('NFD').replace(/[\u0300-\u036f]/g, ''); // Bỏ dấu
    description = description.replace(/[^a-zA-Z0-9 ]/g, "").replace(/\s+/g, "%20"); // Xóa ký tự đặc biệt và thay khoảng trắng bằng %20

    // 4. Tạo URL VietQR
    const qrUrl = `https://img.vietqr.io/image/${bankId}-${accountNo}-${template}.png?amount=${cleanAmount}&addInfo=${description}&accountName=${encodeURIComponent(accountName)}`;

    // 5. Cập nhật thẻ img tĩnh thành ảnh QR động
    document.getElementById('qrImage').src = qrUrl;

    // Hiển thị modal (nếu bạn dùng bootstrap modal qua JS)
    const modal = new bootstrap.Modal(document.getElementById('paymentModal'));
    modal.show();
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