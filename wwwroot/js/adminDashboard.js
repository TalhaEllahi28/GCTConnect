// =============================
// Global variables
// =============================
let currentPage = 'dashboard';
let sidebarCollapsed = false;

// =============================
// DOM elements
// =============================
const sidebar = document.querySelector('.sidebar');
const mainContent = document.querySelector('.main-content');
const sidebarToggle = document.querySelector('.sidebar-toggle');
const pageTitle = document.querySelector('.page-title');

// =============================
// Navigation functionality
// =============================
function showPage(pageName) {
    // Hide all pages

    // Update page title
    const titles = {
        dashboard: 'Admin Dashboard',
        users: 'User Management',
        batches: 'Batch Management',
        departments: 'Department Management',
        groups: 'Group Management',
        announcements: 'Announcement Management',
        reports: 'System Reports',
        settings: 'System Settings'
    };

    pageTitle.textContent = titles[pageName] || 'Admin Panel';
    currentPage = pageName;

    // Close sidebar on mobile after navigation
    if (window.innerWidth <= 768) {
        toggleSidebar();
    }
}

// =============================
// Sidebar toggle functionality
// =============================
function toggleSidebar() {
    sidebar.classList.toggle('active');
    sidebarCollapsed = !sidebarCollapsed;

    // Update main content margin on desktop
    if (window.innerWidth > 768) {
        mainContent.classList.toggle('expanded', sidebarCollapsed);
    }
}

// =============================
// Modal functionality
// =============================
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }
}

function hideModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('active');
        document.body.style.overflow = 'auto';
    }
}

// Close modal when clicking outside
window.addEventListener('click', (e) => {
    if (e.target.classList.contains('modal')) {
        hideModal(e.target.id);
    }
});

// Escape key to close modals
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        document.querySelectorAll('.modal.active').forEach(modal => {
            hideModal(modal.id);
        });
    }
});

// =============================
// User management
// =============================
function editUser(userId) {
    console.log('Edit user:', userId);
    showNotification('Edit user functionality would be implemented here', 'info');
}

function deleteUser(userId) {
    if (confirm('Are you sure you want to delete this user?')) {
        console.log('Delete user:', userId);
        showNotification('User deleted successfully', 'success');
    }
}

// =============================
// Batch management
// =============================
function viewBatchDetails(batchId) {
    console.log('View batch details:', batchId);
    showNotification('Batch details view would be implemented here', 'info');
}

function deleteBatch(batchId) {
    if (confirm('Are you sure you want to delete this batch? This action cannot be undone.')) {
        console.log('Delete batch:', batchId);
        showNotification('Batch deleted successfully', 'success');
    }
}



// =============================
// Group management
// =============================
function viewGroup(groupId) {
    console.log('View group:', groupId);
    showNotification('Group details view would be implemented here', 'info');
}

function editGroup(groupId) {
    console.log('Edit group:', groupId);
    showNotification('Edit group functionality would be implemented here', 'info');
}

function deleteGroup(groupId) {
    if (confirm('Are you sure you want to delete this group?')) {
        console.log('Delete group:', groupId);
        showNotification('Group deleted successfully', 'success');
    }
}

// =============================
// Announcement management
// =============================
function editAnnouncement(announcementId) {
    console.log('Edit announcement:', announcementId);
    showNotification('Edit announcement functionality would be implemented here', 'info');
}

// =============================
// Report generation
// =============================
function generateReport(reportType) {
    console.log('Generate report:', reportType);
    showNotification(`Generating ${reportType} report...`, 'info');

    setTimeout(() => {
        showNotification(`${reportType} report generated successfully`, 'success');
    }, 2000);
}

// =============================
// Settings
// =============================
function saveSettings() {
    console.log('Save settings');
    showNotification('Settings saved successfully', 'success');
}

function resetSettings() {
    if (confirm('Are you sure you want to reset all settings to default?')) {
        console.log('Reset settings');
        showNotification('Settings reset to default', 'info');
    }
}

// =============================
// Notification system
// =============================
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <i class="fas fa-${getNotificationIcon(type)}"></i>
            <span>${message}</span>
        </div>
        <button class="notification-close" onclick="this.parentElement.remove()">
            <i class="fas fa-times"></i>
        </button>
    `;

    if (!document.getElementById('notification-styles')) {
        const styles = document.createElement('style');
        styles.id = 'notification-styles';
        styles.textContent = `
            .notification {
                position: fixed;
                top: 20px;
                right: 20px;
                background: white;
                padding: 1rem;
                border-radius: 0.5rem;
                box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
                border-left: 4px solid;
                display: flex;
                align-items: center;
                gap: 1rem;
                z-index: 3000;
                animation: slideIn 0.3s ease;
                max-width: 400px;
            }
            .notification-success { border-left-color: #10b981; }
            .notification-error { border-left-color: #ef4444; }
            .notification-warning { border-left-color: #f59e0b; }
            .notification-info { border-left-color: #3b82f6; }
            .notification-content { display: flex; align-items: center; gap: 0.5rem; flex: 1; }
            .notification-close { background: none; border: none; color: #6b7280; cursor: pointer; padding: 0.25rem; border-radius: 0.25rem; }
            .notification-close:hover { background: #f3f4f6; }
            @keyframes slideIn {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
        `;
        document.head.appendChild(styles);
    }

    document.body.appendChild(notification);

    setTimeout(() => {
        if (notification.parentElement) notification.remove();
    }, 5000);
}

function getNotificationIcon(type) {
    const icons = {
        success: 'check-circle',
        error: 'exclamation-circle',
        warning: 'exclamation-triangle',
        info: 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// =============================
// Search functionality
// =============================
function setupSearch() {
    const searchInputs = document.querySelectorAll('.search-input');
    searchInputs.forEach(input => {
        input.addEventListener('input', (e) => {
            const searchTerm = e.target.value.toLowerCase();
            const table = e.target.closest('.card')?.querySelector('.data-table');
            if (table) {
                table.querySelectorAll('tbody tr').forEach(row => {
                    row.style.display = row.textContent.toLowerCase().includes(searchTerm) ? '' : 'none';
                });
            }
        });
    });
}

// =============================
// Filter functionality
// =============================
function setupFilters() {
    const roleFilter = document.querySelector('#role-filter');
    const deptFilter = document.querySelector('#dept-filter');

    function filterTable() {
        const role = roleFilter?.value.toLowerCase() || '';
        const dept = deptFilter?.value.toLowerCase() || '';
        const rows = document.querySelectorAll('.data-table tbody tr');

        rows.forEach(row => {
            const roleText = row.querySelector('.role-badge')?.textContent.toLowerCase() || '';
            const deptText = row.cells[3]?.textContent.toLowerCase() || '';
            const matches =
                (!role || roleText === role) &&
                (!dept || deptText.includes(dept));
            row.style.display = matches ? '' : 'none';
        });
    }

    roleFilter?.addEventListener('change', filterTable);
    deptFilter?.addEventListener('change', filterTable);
}

// =============================
// Form validation & submission
// =============================
function validateForm(form) {
    const requiredFields = form.querySelectorAll('[required]');
    let isValid = true;

    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            field.classList.add('error');
            isValid = false;
        } else {
            field.classList.remove('error');
        }
    });

    return isValid;
}

function handleFormSubmit(formType, formElement) {
    if (!validateForm(formElement)) {
        showNotification('Please fill in all required fields', 'error');
        return;
    }

    const formData = new FormData(formElement);
    const data = Object.fromEntries(formData.entries());
    console.log(`${formType} form submitted:`, data);

    showNotification(`${formType} created successfully!`, 'success');
    formElement.reset();

    const modal = formElement.closest('.modal');
    if (modal) {
        hideModal(modal.id);
    }
}

// =============================
// Responsive handling
// =============================
function handleResize() {
    if (window.innerWidth <= 768) {
        sidebar.classList.remove('active');
        mainContent.classList.remove('expanded');
    } else {
        sidebar.classList.remove('active');
        mainContent.classList.toggle('expanded', sidebarCollapsed);
    }
}



// =============================
// Export functions for global use
// =============================
//window.showPage = showPage;
//window.toggleSidebar = toggleSidebar;
//window.showModal = showModal;
//window.hideModal = hideModal;
//window.editUser = editUser;
//window.deleteUser = deleteUser;
//window.viewBatchDetails = viewBatchDetails;
//window.deleteBatch = deleteBatch;
//window.editDepartment = editDepartment;
//window.deleteDepartment = deleteDepartment;
//window.viewGroup = viewGroup;
//window.editGroup = editGroup;
//window.deleteGroup = deleteGroup;
//window.editAnnouncement = editAnnouncement;
//window.generateReport = generateReport;
//window.saveSettings = saveSettings;
//window.resetSettings = resetSettings;
