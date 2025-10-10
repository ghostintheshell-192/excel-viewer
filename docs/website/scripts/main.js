/* =============================================
   SheetAtlas Website JavaScript
   Smooth interactions and enhanced UX
   ============================================= */

document.addEventListener('DOMContentLoaded', function() {
    // Smooth scrolling for navigation links
    initSmoothScrolling();

    // Navigation background on scroll
    initNavbarScrollEffect();

    // Animate elements on scroll
    initScrollAnimations();

    // Copy to clipboard functionality
    initCopyButtons();

    // Enhanced download tracking
    initDownloadTracking();
});

// Smooth scrolling for internal links
function initSmoothScrolling() {
    const links = document.querySelectorAll('a[href^="#"]');

    links.forEach(link => {
        link.addEventListener('click', function(e) {
            const href = this.getAttribute('href');

            // Skip if it's just "#"
            if (href === '#') return;

            e.preventDefault();

            const target = document.querySelector(href);
            if (target) {
                const offsetTop = target.offsetTop - 80; // Account for fixed navbar

                window.scrollTo({
                    top: offsetTop,
                    behavior: 'smooth'
                });
            }
        });
    });
}

// Add background to navbar on scroll
function initNavbarScrollEffect() {
    const navbar = document.querySelector('.navbar');

    if (!navbar) return;

    window.addEventListener('scroll', function() {
        if (window.scrollY > 50) {
            navbar.style.backgroundColor = 'rgba(255, 255, 255, 0.95)';
            navbar.style.boxShadow = '0 2px 10px rgba(0, 0, 0, 0.1)';
        } else {
            navbar.style.backgroundColor = 'var(--bg-primary)';
            navbar.style.boxShadow = 'none';
        }
    });
}

// Animate elements when they come into view
function initScrollAnimations() {
    // Check if Intersection Observer is supported
    if (!('IntersectionObserver' in window)) return;

    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Observe elements that should animate
    const animatedElements = document.querySelectorAll('.feature-card, .doc-card, .download-card, .screenshot-item');

    animatedElements.forEach((el, index) => {
        // Set initial state
        el.style.opacity = '0';
        el.style.transform = 'translateY(30px)';
        el.style.transition = `opacity 0.6s ease-out ${index * 0.1}s, transform 0.6s ease-out ${index * 0.1}s`;

        observer.observe(el);
    });
}

// Copy text to clipboard functionality
function initCopyButtons() {
    // Add copy buttons to code blocks if any
    const codeBlocks = document.querySelectorAll('pre code');

    codeBlocks.forEach(block => {
        const button = document.createElement('button');
        button.className = 'copy-btn';
        button.textContent = 'Copy';
        button.setAttribute('aria-label', 'Copy code to clipboard');

        button.addEventListener('click', function() {
            const text = block.textContent;

            if (navigator.clipboard && window.isSecureContext) {
                navigator.clipboard.writeText(text).then(() => {
                    showCopyFeedback(button);
                });
            } else {
                // Fallback for older browsers
                const textArea = document.createElement('textarea');
                textArea.value = text;
                textArea.style.position = 'fixed';
                textArea.style.left = '-999999px';
                textArea.style.top = '-999999px';
                document.body.appendChild(textArea);
                textArea.focus();
                textArea.select();

                try {
                    document.execCommand('copy');
                    showCopyFeedback(button);
                } catch (err) {
                    console.error('Failed to copy text: ', err);
                }

                textArea.remove();
            }
        });

        const container = block.parentNode;
        container.style.position = 'relative';
        container.appendChild(button);
    });
}

// Show feedback when text is copied
function showCopyFeedback(button) {
    const originalText = button.textContent;
    button.textContent = 'Copied!';
    button.style.backgroundColor = 'var(--success-color)';

    setTimeout(() => {
        button.textContent = originalText;
        button.style.backgroundColor = '';
    }, 2000);
}

// Track download button clicks
function initDownloadTracking() {
    const downloadButtons = document.querySelectorAll('.btn-download, .btn-download-alt');

    downloadButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            const platform = this.closest('.download-card')?.querySelector('h3')?.textContent || 'Unknown';
            const filename = this.href.split('/').pop() || 'Unknown file';

            // Track the download (you can replace this with your analytics)
            console.log(`Download started: ${platform} - ${filename}`);

            // You could send this to Google Analytics, Plausible, etc.
            // gtag('event', 'download', {
            //     'platform': platform,
            //     'filename': filename
            // });
        });
    });
}

// Add loading animation to buttons
function addButtonLoadingState(button) {
    button.style.opacity = '0.7';
    button.style.pointerEvents = 'none';

    const originalText = button.textContent;
    button.textContent = 'Loading...';

    // Remove loading state after a delay
    setTimeout(() => {
        button.style.opacity = '1';
        button.style.pointerEvents = 'auto';
        button.textContent = originalText;
    }, 1500);
}

// Handle external links
document.addEventListener('click', function(e) {
    const link = e.target.closest('a[href^="http"]');

    if (link && !link.hostname.includes(window.location.hostname)) {
        // Add visual feedback for external links
        if (link.classList.contains('btn')) {
            addButtonLoadingState(link);
        }
    }
});

// Keyboard accessibility improvements
document.addEventListener('keydown', function(e) {
    // Skip to main content with Tab key
    if (e.key === 'Tab' && !e.shiftKey && e.target === document.body) {
        const mainContent = document.querySelector('main, .hero, #features');
        if (mainContent) {
            e.preventDefault();
            mainContent.focus();
        }
    }
});

// Mobile menu toggle (if needed in future)
function initMobileMenu() {
    const menuToggle = document.querySelector('.menu-toggle');
    const navLinks = document.querySelector('.nav-links');

    if (!menuToggle || !navLinks) return;

    menuToggle.addEventListener('click', function() {
        navLinks.classList.toggle('mobile-open');
        this.classList.toggle('active');

        // Update aria-expanded
        const isExpanded = navLinks.classList.contains('mobile-open');
        this.setAttribute('aria-expanded', isExpanded);
    });

    // Close menu when clicking outside
    document.addEventListener('click', function(e) {
        if (!menuToggle.contains(e.target) && !navLinks.contains(e.target)) {
            navLinks.classList.remove('mobile-open');
            menuToggle.classList.remove('active');
            menuToggle.setAttribute('aria-expanded', 'false');
        }
    });
}

// Add subtle parallax effect to hero section
function initParallaxEffect() {
    const hero = document.querySelector('.hero');

    if (!hero) return;

    window.addEventListener('scroll', function() {
        const scrolled = window.pageYOffset;
        const rate = scrolled * -0.5;

        hero.style.transform = `translateY(${rate}px)`;
    });
}

// Enhanced error handling for images
function initImageErrorHandling() {
    const images = document.querySelectorAll('img');

    images.forEach(img => {
        img.addEventListener('error', function() {
            // Create a placeholder for broken images
            const placeholder = document.createElement('div');
            placeholder.className = 'image-placeholder';
            placeholder.style.cssText = `
                width: ${this.width || 300}px;
                height: ${this.height || 200}px;
                background-color: var(--gray-200);
                display: flex;
                align-items: center;
                justify-content: center;
                border-radius: var(--radius-md);
                color: var(--text-muted);
                font-size: var(--font-size-sm);
            `;
            placeholder.textContent = 'Image not available';

            this.parentNode.replaceChild(placeholder, this);
        });
    });
}

// Initialize image error handling when DOM is ready
document.addEventListener('DOMContentLoaded', initImageErrorHandling);

// Performance: Lazy load images if Intersection Observer is available
function initLazyLoading() {
    if (!('IntersectionObserver' in window)) return;

    const images = document.querySelectorAll('img[data-src]');

    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                observer.unobserve(img);
            }
        });
    });

    images.forEach(img => imageObserver.observe(img));
}

// Add focus management for better accessibility
function initFocusManagement() {
    // Skip to main content link
    const skipLink = document.createElement('a');
    skipLink.href = '#main-content';
    skipLink.textContent = 'Skip to main content';
    skipLink.className = 'skip-link';
    skipLink.style.cssText = `
        position: absolute;
        top: -40px;
        left: 6px;
        background: var(--primary-color);
        color: white;
        padding: 8px;
        text-decoration: none;
        border-radius: 4px;
        transition: top 0.3s ease;
    `;

    skipLink.addEventListener('focus', function() {
        this.style.top = '6px';
    });

    skipLink.addEventListener('blur', function() {
        this.style.top = '-40px';
    });

    document.body.insertBefore(skipLink, document.body.firstChild);

    // Add main-content id to hero section
    const hero = document.querySelector('.hero');
    if (hero) {
        hero.id = 'main-content';
        hero.setAttribute('tabindex', '-1');
    }
}

// Initialize focus management
document.addEventListener('DOMContentLoaded', initFocusManagement);