
        // Hiệu ứng cuộn trang mượt mà
    document.addEventListener("DOMContentLoaded", function() {
            const animatedElements = document.querySelectorAll('[data-anim="up"]');

            const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.classList.add('anim-in');
                }, index * 100); // Tạo độ trễ xếp tầng cho các mục
            }
        });
            }, {threshold: 0.1 });

            animatedElements.forEach(el => observer.observe(el));
        