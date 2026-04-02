
        // Hiệu ứng cuộn trang mượt mà (scroll animation) dựa vào CSS của bạn
    document.addEventListener("DOMContentLoaded", function() {
            const animatedElements = document.querySelectorAll('[data-anim="up"]');

            const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('anim-in');
            }
        });
            }, {threshold: 0.1 });

            animatedElements.forEach(el => observer.observe(el));
        });