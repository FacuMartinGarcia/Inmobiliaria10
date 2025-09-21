
(function () {
    const wrapper = document.getElementById('appWrapper');
    const btn = document.getElementById('btnSidebarToggle');
    const KEY = 'sidebar-collapsed';

    // Restaurar estado al cargar
    if (localStorage.getItem(KEY) === 'true') {
        wrapper?.classList.add('sidebar-collapsed');
    }

    // Toggle on click
    btn?.addEventListener('click', () => {
        wrapper?.classList.toggle('sidebar-collapsed');
        const isCollapsed = wrapper?.classList.contains('sidebar-collapsed');
        localStorage.setItem(KEY, String(isCollapsed));
    });
})();
