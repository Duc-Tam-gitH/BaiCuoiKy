
        // Xử lý checkbox role để thêm class active
        document.querySelectorAll('.admin-role-card input').forEach(checkbox => {
            const card = checkbox.closest('.admin-role-card');

    // Khởi tạo trạng thái ban đầu
    if (checkbox.checked) {
        card.classList.add('active');
            }

    // Xử lý khi click
    checkbox.addEventListener('change', function() {
                if (this.checked) {
        card.classList.add('active');
                } else {
        card.classList.remove('active');
                }
            });
        });

        // Hiệu ứng ripple cho button
        document.querySelectorAll('.admin-btn').forEach(btn => {
        btn.addEventListener('click', function (e) {
            let ripple = document.createElement('span');
            ripple.classList.add('ripple');
            this.appendChild(ripple);

            let x = e.clientX - e.target.offsetLeft;
            let y = e.clientY - e.target.offsetTop;

            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';

            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
        });
