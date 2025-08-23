// GCTConnect Landing Page Interactions
document.addEventListener('DOMContentLoaded', function () {
    // Get DOM elements
    //const loginBtn = document.getElementById('loginBtn');
    const floatingCards = document.querySelectorAll('.floating-card');

    //// Login button click handler
    //loginBtn.addEventListener('click', function (e) {
    //    e.preventDefault();

    //    // Add loading state
    //    const originalText = this.querySelector('span').textContent;
    //    this.querySelector('span').textContent = 'Connecting...';
    //    this.style.pointerEvents = 'none';

    //    // Simulate login process (replace with actual login logic)
    //    setTimeout(() => {
    //        // For now, just show an alert - replace with actual login functionality
    //        alert('Login functionality will be implemented in the next phase!');

    //        // Reset button
    //        this.querySelector('span').textContent = originalText;
    //        this.style.pointerEvents = 'auto';
    //    }, 1500);
    //});

    // Add staggered animation to floating cards
    floatingCards.forEach((card, index) => {
        card.style.animationDelay = `${index * 2}s`;
    });

    // Enhanced interactions for better UX
    if (window.matchMedia('(prefers-reduced-motion: no-preference)').matches) {
        // Add subtle mouse movement effect
        document.addEventListener('mousemove', function (e) {
            const cards = document.querySelectorAll('.floating-card');
            const mouseX = e.clientX / window.innerWidth;
            const mouseY = e.clientY / window.innerHeight;

            cards.forEach((card, index) => {
                const speed = (index + 1) * 0.5;
                const x = (mouseX - 0.5) * speed;
                const y = (mouseY - 0.5) * speed;
                card.style.transform = `translate(${x}px, ${y}px)`;
            });
        });
    }

    // Add intersection observer for animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    // Observe elements for animation
    const animatedElements = document.querySelectorAll('.badge, .main-title, .main-description, .stats-container, .login-card');
    animatedElements.forEach(el => {
        observer.observe(el);
    });

    //// Add keyboard navigation for accessibility
    //loginBtn.addEventListener('keydown', function (e) {
    //    if (e.key === 'Enter' || e.key === ' ') {
    //        e.preventDefault();
    //        this.click();
    //    }
    //});
});