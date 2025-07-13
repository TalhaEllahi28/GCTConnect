// Global variables
let currentPage = 'dashboard';
let sidebarCollapsed = false;

// DOM elements
const sidebar = document.querySelector('.sidebar');
const mainContent = document.querySelector('.main-content');
const sidebarToggle = document.querySelector('.sidebar-toggle');
const pageTitle = document.querySelector('.page-title');

// Navigation functionality
function showPage(pageName) {
    // Hide all pages
    document.querySelectorAll('.page').forEach(page => {
        page.classList.remove('active');
    });

    // Show selected page
    const targetPage = document.getElementById(pageName);
    if (targetPage) {
        targetPage.classList.add('active');
    }

    // Update navigation
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
    });

    // Find and activate the correct nav item
    const activeNavItem = document.querySelector(`[onclick="showPage('${pageName}')"]`).closest('.nav-item');
    if (activeNavItem) {
        activeNavItem.classList.add('active');
    }

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

// Sidebar toggle functionality
function toggleSidebar() {
    sidebar.classList.toggle('active');
    sidebarCollapsed = !sidebarCollapsed;

    // Update main content margin on desktop
    if (window.innerWidth > 768) {
        mainContent.classList.toggle('expanded', sidebarCollapsed);
    }
}

// Modal functionality
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

// User management functions
function editUser(userId) {
    console.log('Edit user:', userId);
    // Add edit user functionality here
    showNotification('Edit user functionality would be implemented here', 'info');
}

function deleteUser(userId) {
    if (confirm('Are you sure you want to delete this user?')) {
        console.log('Delete user:', userId);
        // Add delete user functionality here
        showNotification('User deleted successfully', 'success');
    }
}

// Batch management functions
function viewBatchDetails(batchId) {
    console.log('View batch details:', batchId);
    // Add view batch details functionality here
    showNotification('Batch details view would be implemented here', 'info');
}

function deleteBatch(batchId) {
    if (confirm('Are you sure you want to delete this batch? This action cannot be undone.')) {
        console.log('Delete batch:', batchId);
        // Add delete batch functionality here
        showNotification('Batch deleted successfully', 'success');
    }
}

// Department management functions
function editDepartment(deptId) {
    console.log('Edit department:', deptId);
    // Add edit department functionality here
    showNotification('Edit department functionality would be implemented here', 'info');
}

function deleteDepartment(deptId) {
    if (confirm('Are you sure you want to delete this department?')) {
        console.log('Delete department:', deptId);
        // Add delete department functionality here
        showNotification('Department deleted successfully', 'success');
    }
}

// Group management functions
function viewGroup(groupId) {
    console.log('View group:', groupId);
    // Add view group functionality here
    showNotification('Group details view would be implemented here', 'info');
}

function editGroup(groupId) {
    console.log('Edit group:', groupId);
    // Add edit group functionality here
    showNotification('Edit group functionality would be implemented here', 'info');
}

function deleteGroup(groupId) {
    if (confirm('Are you sure you want to delete this group?')) {
        console.log('Delete group:', groupId);
        // Add delete group functionality here
        showNotification('Group deleted successfully', 'success');
    }
}

// Announcement management functions
function editAnnouncement(announcementId) {
    console.log('Edit announcement:', announcementId);
    // Add edit announcement functionality here
    showNotification('Edit announcement functionality would be implemented here', 'info');
}

function deleteAnnouncement(announcementId) {
    if (confirm('Are you sure you want to delete this announcement?')) {
        console.log('Delete announcement:', announcementId);
        // Add delete announcement functionality here
        showNotification('Announcement deleted successfully', 'success');
    }
}

// Report generation functions
function generateReport(reportType) {
    console.log('Generate report:', reportType);
    // Add report generation functionality here
    showNotification(`Generating ${reportType} report...`, 'info');

    // Simulate report generation
    setTimeout(() => {
        showNotification(`${reportType} report generated successfully`, 'success');
    }, 2000);
}

// Settings functions
function saveSettings() {
    console.log('Save settings');
    // Add save settings functionality here
    showNotification('Settings saved successfully', 'success');
}

function resetSettings() {
    if (confirm('Are you sure you want to reset all settings to default?')) {
        console.log('Reset settings');
        // Add reset settings functionality here
        showNotification('Settings reset to default', 'info');
    }
}

// Notification system
function showNotification(message, type = 'info') {
    // Create notification element
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

    // Add styles if not already added
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
            
            .notification-success {
                border-left-color: #10b981;
            }
            
            .notification-error {
                border-left-color: #ef4444;
            }
            
            .notification-warning {
                border-left-color: #f59e0b;
            }
            
            .notification-info {
                border-left-color: #3b82f6;
            }
            
            .notification-content {
                display: flex;
                align-items: center;
                gap: 0.5rem;
                flex: 1;
            }
            
            .notification-close {
                background: none;
                border: none;
                color: #6b7280;
                cursor: pointer;
                padding: 0.25rem;
                border-radius: 0.25rem;
                transition: background-color 0.2s ease;
            }
            
            .notification-close:hover {
                background: #f3f4f6;
            }
            
            @keyframes slideIn {
                from {
                    transform: translateX(100%);
                    opacity: 0;
                }
                to {
                    transform: translateX(0);
                    opacity: 1;
                }
            }
        `;
        document.head.appendChild(styles);
    }

    // Add to DOM
    document.body.appendChild(notification);

    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
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

// Search functionality
function setupSearch() {
    const searchInputs = document.querySelectorAll('.search-input');

    searchInputs.forEach(input => {
        input.addEventListener('input', (e) => {
            const searchTerm = e.target.value.toLowerCase();
            const table = e.target.closest('.card').querySelector('.data-table');

            if (table) {
                const rows = table.querySelectorAll('tbody tr');
                rows.forEach(row => {
                    const text = row.textContent.toLowerCase();
                    row.style.display = text.includes(searchTerm) ? '' : 'none';
                });
            }
        });
    });
}

// Filter functionality
function setupFilters() {
    const filterSelects = document.querySelectorAll('.filter-select');

    filterSelects.forEach(select => {
        select.addEventListener('change', (e) => {
            const filterValue = e.target.value.toLowerCase();
            const table = e.target.closest('.card').querySelector('.data-table');

            if (table) {
                const rows = table.querySelectorAll('tbody tr');
                rows.forEach(row => {
                    if (!filterValue) {
                        row.style.display = '';
                    } else {
                        const text = row.textContent.toLowerCase();
                        row.style.display = text.includes(filterValue) ? '' : 'none';
                    }
                });
            }
        });
    });
}

// Form validation
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

// Form submission handlers
function handleFormSubmit(formType, formElement) {
    if (!validateForm(formElement)) {
        showNotification('Please fill in all required fields', 'error');
        return;
    }

    // Simulate form submission
    const formData = new FormData(formElement);
    const data = Object.fromEntries(formData.entries());

    console.log(`${formType} form submitted:`, data);

    // Show success message
    showNotification(`${formType} created successfully!`, 'success');

    // Reset form
    formElement.reset();

    // Hide modal
    const modal = formElement.closest('.modal');
    if (modal) {
        hideModal(modal.id);
    }
}

// Real-time updates (simulated)
function startRealTimeUpdates() {
    // Simulate real-time activity updates
    setInterval(() => {
        updateActivityFeed();
        updateStats();
    }, 30000); // Update every 30 seconds
}

function updateActivityFeed() {
    const activities = [
        { icon: 'user-plus', text: 'New user registered', time: 'Just now', type: 'purple' },
        { icon: 'calendar-plus', text: 'Batch created', time: '2 minutes ago', type: 'green' },
        { icon: 'bullhorn', text: 'Announcement posted', time: '5 minutes ago', type: 'gold' },
        { icon: 'users', text: 'Group updated', time: '10 minutes ago', type: 'blue' }
    ];

    const randomActivity = activities[Math.floor(Math.random() * activities.length)];

    // Add new activity to the top of the list
    const activityList = document.querySelector('.activity-list');
    if (activityList) {
        const activityItem = document.createElement('div');
        activityItem.className = 'activity-item';
        activityItem.innerHTML = `
            <div class="activity-icon ${randomActivity.type}">
                <i class="fas fa-${randomActivity.icon}"></i>
            </div>
            <div class="activity-content">
                <p>${randomActivity.text}</p>
                <small>${randomActivity.time}</small>
            </div>
        `;

        activityList.insertBefore(activityItem, activityList.firstChild);

        // Remove oldest activity if more than 5
        if (activityList.children.length > 5) {
            activityList.removeChild(activityList.lastChild);
        }
    }
}

function updateStats() {
    // Simulate stat updates
    const statCards = document.querySelectorAll('.stat-card');

    statCards.forEach(card => {
        const statValue = card.querySelector('h3');
        if (statValue) {
            const currentValue = parseInt(statValue.textContent.replace(/,/g, ''));
            const newValue = currentValue + Math.floor(Math.random() * 5);
            statValue.textContent = newValue.toLocaleString();
        }
    });
}

// Responsive handling
function handleResize() {
    if (window.innerWidth <= 768) {
        sidebar.classList.remove('active');
        mainContent.classList.remove('expanded');
    } else {
        sidebar.classList.remove('active');
        mainContent.classList.toggle('expanded', sidebarCollapsed);
    }
}

// Initialize application
document.addEventListener('DOMContentLoaded', () => {
    // Initialize search and filters
    setupSearch();
    setupFilters();

    // Add form submission handlers
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', (e) => {
            e.preventDefault();

            const formType = form.className.split('-')[0];
            handleFormSubmit(formType, form);
        });
    });

    // Add error styles for form validation
    const style = document.createElement('style');
    style.textContent = `
        .form-input.error,
        .form-select.error,
        .form-textarea.error {
            border-color: #ef4444;
            box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
        }
    `;
    document.head.appendChild(style);

    // Start real-time updates
    startRealTimeUpdates();

    // Add resize listener
    window.addEventListener('resize', handleResize);

    // Add sidebar toggle listener
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', toggleSidebar);
    }

    // Initialize with dashboard page
    showPage('dashboard');

    console.log('GCT Connect Admin Panel initialized');
});

// Export functions for global access
window.showPage = showPage;
window.toggleSidebar = toggleSidebar;
window.showModal = showModal;
window.hideModal = hideModal;
window.editUser = editUser;
window.deleteUser = deleteUser;
window.viewBatchDetails = viewBatchDetails;
window.deleteBatch = deleteBatch;
window.editDepartment = editDepartment;
window.deleteDepartment = deleteDepartment;
window.viewGroup = viewGroup;
window.editGroup = editGroup;
window.deleteGroup = deleteGroup;
window.editAnnouncement = editAnnouncement;
window.deleteAnnouncement = deleteAnnouncement;
window.generateReport = generateReport;
window.saveSettings = saveSettings;
window.resetSettings = resetSettings;