function createDepartment() {
    var departmentName = document.getElementById("departmentName").value;
    if (!departmentName) {
        alert("Please enter a department name.");
        return;
    }
    // Send data to server (using fetch or AJAX)
    console.log("Creating department:", departmentName);
    $.ajax({
        url: '/admin/add-department',
        type: 'POST',
        data: { department: departmentName },
        success: function (response) {
            showDepartmentNotification(response.message, "success")
            hideModal('addDepartmentModal');
            setTimeout(() => {
                location.reload(); // Simple way to refresh the list
            }, 2000);
        },
        error: function () {
            showError('Failed to load department departments');
        }
    });



}
function saveDepartment() {
    var depId = document.getElementById("departmentId").value;
    var departmentName = document.getElementById("departmentName").value;

    if (!departmentName) {
        alert("Please enter a department name.");
        return;
    }

    if (depId) {
        // Update existing department
        $.ajax({
            url: '/admin/edit-department',
            type: 'POST',
            data: { depId: depId, name: departmentName },
            success: function (response) {
                if (response.success) {
                    showDepartmentNotification(response.message, "success");
                    hideModal('addDepartmentModal');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    showError(response.message);
                }
            },
            error: function () {
                showError('Failed to update department');
            }
        });
    } else {
        document.getElementById("departmentModalTitle").innerText = "Add New Department";
        $.ajax({
            url: '/admin/add-department',
            type: 'POST',
            data: { department: departmentName },
            success: function (response) {
                if (response.success || response.message) {
                    showDepartmentNotification(response.message, "success");
                    hideModal('addDepartmentModal');
                    setTimeout(() => location.reload(), 1500);
                }
            },
            error: function () {
                showError('Failed to create department');
            }
        });
    }
}

function editDepartment(departmentId) {
    $.ajax({
        url: '/admin/edit-department',
        type: 'GET',
        data: { depId: departmentId },
        success: function (response) {
            if (response.success) {
                // Populate modal with department data
                document.getElementById("departmentId").value = response.department.departmentId;
                document.getElementById("departmentName").value = response.department.name;

                // Update modal title & button
                document.getElementById("departmentModalTitle").innerText = "Edit Department";
                document.getElementById("saveDepartmentBtn").innerText = "Update Department";

                showModal('addDepartmentModal');
            } else {
                showDepartmentNotification(response.message, "error");
            }
        },
        error: function () {
            showError('Failed to load department details');
        }
    });
}

function deleteDepartment(deptId) {
    if (confirm("Are you sure you want to delete this department? This will delete that department related user,  groups and even messages in the related groups!")) {
        $.ajax({
            url: '/admin/delete-department',
            type: 'DELETE',
            data: { depId: deptId },
            success: function (response) {
                if (response.success) {
                    showDepartmentNotification(response.message, "success");
                    setTimeout(() => location.reload(), 1500);
                } else {
                    showDepartmentNotification(response.message, "error");
                }
            },
            error: function () {
                showDepartmentNotification('Failed to delete department',"error");
            }
        });
    }
}

function showDepartmentNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `department-notification department-notification-${type}`;
    notification.innerHTML = `
            <i class="fas fa-${type === 'success' ? 'check-circle' : 'info-circle'}"></i>
            <span>${message}</span>
            <button onclick="this.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        `;

    // Add to page
    document.body.appendChild(notification);

    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}