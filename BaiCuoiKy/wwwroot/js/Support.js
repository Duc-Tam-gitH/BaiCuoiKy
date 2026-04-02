
        // Hiệu ứng load nội dung nhẹ nhàng
    document.addEventListener("DOMContentLoaded", function() {
            const animatedElements = document.querySelectorAll('[data-anim="up"]');
            const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.classList.add('anim-in');
                }, index * 100);
            }
        });
            }, {threshold: 0.1 });
            animatedElements.forEach(el => observer.observe(el));
        });
