document.addEventListener('DOMContentLoaded', () => {
    const sidebar = document.querySelector('.sidebar');
    const toggleBtn = document.querySelector('.sidebar-toggle');
    const mainContent = document.querySelector('.main-content');

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', () => {
            if (window.innerWidth <= 768) {
                sidebar.classList.toggle('open');
                // Toggle mobile overlay
                let overlay = document.querySelector('.sidebar-overlay');
                if (!overlay) {
                    overlay = document.createElement('div');
                    overlay.className = 'sidebar-overlay';
                    document.body.appendChild(overlay);
                    overlay.addEventListener('click', () => {
                        sidebar.classList.remove('open');
                        overlay.remove();
                    });
                } else {
                    overlay.remove();
                }
            } else {
                sidebar.classList.toggle('collapsed');
                if (mainContent) mainContent.classList.toggle('expanded');
            }
        });
    }
});