(function () {
    'use strict';

    /* ---- Theme (light / dark) ---- */
    const THEME_KEY = 'planner-theme';

    function getPreferredTheme() {
        const stored = localStorage.getItem(THEME_KEY);
        if (stored === 'light' || stored === 'dark') return stored;
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(THEME_KEY, theme);
        document.querySelectorAll('.theme-toggle-btn').forEach(function (btn) {
            const icon = btn.querySelector('i');
            if (!icon) return;
            icon.className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
        });
    }

    applyTheme(getPreferredTheme());

    document.querySelectorAll('.theme-toggle-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const current = document.documentElement.getAttribute('data-theme') || 'light';
            applyTheme(current === 'dark' ? 'light' : 'dark');
        });
    });

    /* ---- Sidebar (mobile) ---- */
    const sidebar = document.getElementById('appSidebar');
    const overlay = document.getElementById('sidebarOverlay');
    const toggleBtn = document.getElementById('sidebarToggle');

    if (sidebar && toggleBtn) {
        function openSidebar() {
            sidebar.classList.add('active');
            overlay && overlay.classList.add('active');
            toggleBtn.setAttribute('aria-expanded', 'true');
            document.body.style.overflow = 'hidden';
        }

        function closeSidebar() {
            sidebar.classList.remove('active');
            overlay && overlay.classList.remove('active');
            toggleBtn.setAttribute('aria-expanded', 'false');
            document.body.style.overflow = '';
        }

        toggleBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            sidebar.classList.contains('active') ? closeSidebar() : openSidebar();
        });

        overlay && overlay.addEventListener('click', closeSidebar);

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && sidebar.classList.contains('active')) closeSidebar();
        });

        let resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                if (window.innerWidth > 768) closeSidebar();
            }, 150);
        });
    }

    /* ---- Command palette (Ctrl/Cmd + K) ---- */
    const palette = document.getElementById('commandPalette');
    const paletteInput = document.getElementById('commandPaletteInput');
    const paletteForm = document.getElementById('commandPaletteForm');

    function openPalette() {
        if (!palette) return;
        palette.classList.add('active');
        palette.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';
        setTimeout(function () { paletteInput && paletteInput.focus(); }, 80);
    }

    function closePalette() {
        if (!palette) return;
        palette.classList.remove('active');
        palette.setAttribute('aria-hidden', 'true');
        document.body.style.overflow = '';
        if (paletteInput) paletteInput.value = '';
    }

    document.querySelectorAll('[data-open-palette]').forEach(function (el) {
        el.addEventListener('click', function (e) {
            e.preventDefault();
            openPalette();
        });
    });

    if (palette) {
        palette.addEventListener('click', function (e) {
            if (e.target === palette || e.target.classList.contains('command-palette-backdrop')) {
                closePalette();
            }
        });
    }

    if (paletteForm) {
        paletteForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const q = (paletteInput && paletteInput.value.trim()) || '';
            const base = paletteForm.getAttribute('data-search-url') || '/Tasks/Search';
            window.location.href = q ? base + '?Query=' + encodeURIComponent(q) : base;
        });
    }

    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            palette && palette.classList.contains('active') ? closePalette() : openPalette();
        }
        if (e.key === 'Escape' && palette && palette.classList.contains('active')) {
            closePalette();
        }
    });

    /* ---- Staggered list animations ---- */
    document.querySelectorAll('.task-list-row, .kanban-card, .metric-card, .upcoming-item').forEach(function (el, i) {
        el.style.animationDelay = Math.min(i * 0.04, 0.4) + 's';
        el.classList.add('animate-in');
    });

    /* ---- Auto-dismiss alerts ---- */
    document.querySelectorAll('.alert-auto-dismiss').forEach(function (alert) {
        setTimeout(function () {
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-8px)';
            setTimeout(function () { alert.remove(); }, 300);
        }, 5000);
    });
}());
