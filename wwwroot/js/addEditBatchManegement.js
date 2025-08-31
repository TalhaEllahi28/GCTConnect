// Global variables
let allDepartments = [];
let currentBatchDepartments = [];

// Initialize the batch management system
$(document).ready(function () {
    loadDepartments();
});

// Load all departments
function loadDepartments() {
    $.ajax({
        url: '/admin/get-departments',
        type: 'GET',
        success: function (response) {
            if (response.success) {
                allDepartments = response.departments;
            } else {
                showError('Failed to load departments');
            }
        },
        error: function () {
            showError('Failed to load departments');
        }
    });
}

// Show add batch modal
function showAddBatchModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('active');
    //    document.body.style.overflow = 'hidden';
    }
    $('#batchModalTitle').text('Add New Batch');
    $('#batchMode').val('add');
    $('#saveBatchBtn').text('Create Batch');
    $('#batchYear').prop('readonly', false);
    $('#existingDepartmentsInfo').hide();

    clearForm();
    renderDepartmentsList([], false);
    showModal('batchModal');
}

// Show edit batch modal
function showEditBatchModal(batchYear, modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('active');
    //    document.body.style.overflow = 'hidden';
    }
    $('#batchModalTitle').text('Edit Batch ' + batchYear);
    $('#batchMode').val('edit');
    $('#saveBatchBtn').text('Update Batch');
    $('#batchYear').val(batchYear).prop('readonly', true);
    $('#existingDepartmentsInfo').show();

    clearErrors();
    loadBatchDepartments(batchYear);
    showModal('batchModal');
}

// Load departments for existing batch
function loadBatchDepartments(batchYear) {
    $.ajax({
        url: '/admin/get-batch-departments',
        type: 'GET',
        data: { batchYear: batchYear },
        success: function (response) {
            if (response.success) {
                currentBatchDepartments = response.departments;
                displayCurrentDepartments();
                renderDepartmentsList(currentBatchDepartments.map(d => d.departmentId), true);
            } else {
                showError('Failed to load batch departments');
            }
        },
        error: function () {
            showError('Failed to load batch departments');
        }
    });
}

// Display current departments for the batch
function displayCurrentDepartments() {
    let html = '';
    currentBatchDepartments.forEach(dept => {
        html += `<span class="current-dept-tag">${dept.name} (${dept.studentsCount} students)</span>`;
    });
    $('#currentDepartmentsList').html(html);
}

// Render departments list with checkboxes
function renderDepartmentsList(excludeIds, isEditMode) {
    let html = '';

    allDepartments.forEach(dept => {
        const isExisting = excludeIds.includes(dept.departmentId);
        const isDisabled = isEditMode && isExisting ? 'disabled' : '';
        const isChecked = isEditMode && isExisting ? 'checked' : '';
        const labelClass = isEditMode && isExisting ? 'style="opacity: 0.5;"' : '';

        html += `
            <div class="department-item">
                <input type="checkbox" 
                       id="dept_${dept.departmentId}" 
                       value="${dept.departmentId}" 
                       ${isDisabled} 
                       ${isChecked}
                       onchange="clearDepartmentsError()">
                <label for="dept_${dept.departmentId}" ${labelClass}>
                    ${dept.name}
                    ${isEditMode && isExisting ? ' (Already added)' : ''}
                </label>
            </div>
        `;
    });

    $('#departmentsList').html(html);
}

// Save batch (add or edit)
function saveBatch() {
    const mode = $('#batchMode').val();
    const batchYear = parseInt($('#batchYear').val());
    const selectedDepartments = getSelectedDepartments();

    // Validate inputs
    if (!validateInputs(batchYear, selectedDepartments, mode)) {
        return;
    }

    // Show loading state
    $('#saveBatchBtn').prop('disabled', true).text('Saving...');

    if (mode === 'add') {
        // Check if batch exists first
        checkBatchAndAdd(batchYear, selectedDepartments);
    } else {
        // Edit mode
        editBatch(batchYear, selectedDepartments);
    }
}

// Check if batch exists and add if not
function checkBatchAndAdd(batchYear, selectedDepartments) {
    $.ajax({
        url: '/admin/check-batch-exists',
        type: 'POST',
        data: { batchYear: batchYear },
        success: function (response) {
            if (response.success) {
                if (response.exists) {
                    showBatchYearError('This batch year already exists. Use edit mode to add more departments.');
                    resetSaveButton('Create Batch');
                } else {
                    addBatch(batchYear, selectedDepartments);
                }
            } else {
                showError('Error checking batch existence');
                resetSaveButton('Create Batch');
            }
        },
        error: function () {
            showError('Error checking batch existence');
            resetSaveButton('Create Batch');
        }
    });
}

// Add new batch
function addBatch(batchYear, selectedDepartments) {
    $.ajax({
        url: '/admin/add-batch',
        type: 'POST',
        data: {
            batchYear: batchYear,
            selectedDepartments: selectedDepartments
        },
        success: function (response) {
            if (response.success) {
                showBatchNotification(response.message,'success');
                hideModal('batchModal');
                setTimeout(() => {
                    // Refresh the page or update the batch list
                    location.reload();
                }, 1000);            } else {
                showError(response.message);
            }
            resetSaveButton('Create Batch');
        },
        error: function () {
            showError('An error occurred while creating the batch');
            resetSaveButton('Create Batch');
        }
    });
}

// Edit existing batch
function editBatch(batchYear, selectedDepartments) {
    $.ajax({
        url: '/admin/edit-batch',
        type: 'POST',
        data: {
            batchYear: batchYear,
            selectedDepartments: selectedDepartments
        },
        success: function (response) {
            if (response.success) {
                showBatchNotification(response.message,'success');
                hideModal('batchModal');
                setTimeout(() => {
                    // Refresh the page or update the batch list
                    location.reload();
                }, 1000);            } else {
                showError(response.message);
            }
            resetSaveButton('Update Batch');
        },
        error: function () {
            showError('An error occurred while editing the batch');
            resetSaveButton('Update Batch');
        }
    });
}



function deleteBatch(batchYear) {
    if (confirm(`Are you sure you want to delete Batch "${batchYear} This will remove all departments, students, and associated data.`)) {

        $.ajax({
            url: '/admin/delete-batch',
            type: 'POST',
            data: {
                batch: batchYear,
            },
            success: function (response) {
                if (response.success) {
                    showBatchNotification(response.message, 'success');
                    setTimeout(() => {
                        // Refresh the page or update the batch list
                        location.reload();
                    }, 1000);
                } else {
                    showError(response.message);
                }
            },
            error: function () {
                showError('An error occurred while editing the batch');
            }
        });
    }
}
// Get selected departments
function getSelectedDepartments() {
    const selected = [];
    $('#departmentsList input[type="checkbox"]:checked:not(:disabled)').each(function () {
        selected.push(parseInt($(this).val()));
    });
    return selected;
}

// Validate inputs
function validateInputs(batchYear, selectedDepartments, mode) {
    let isValid = true;

    // Clear previous errors
    clearErrors();

    // Validate batch year
    if (!batchYear || batchYear < 2020 || batchYear > 2030) {
        showBatchYearError('Please enter a valid batch year (2020-2030)');
        isValid = false;
    }

    // Validate department selection
    if (selectedDepartments.length === 0) {
        const errorMsg = mode === 'edit'
            ? 'Please select at least one new department to add'
            : 'Please select at least one department';
        showDepartmentsError(errorMsg);
        isValid = false;
    }

    return isValid;
}

// Error handling functions
function showBatchYearError(message) {
    $('#batchYearError').text(message).show();
}

function showDepartmentsError(message) {
    $('#departmentsError').text(message).show();
}

function clearBatchYearError() {
    $('#batchYearError').hide();
}

function clearDepartmentsError() {
    $('#departmentsError').hide();
}

function clearErrors() {
    clearBatchYearError();
    clearDepartmentsError();
}

function clearForm() {
    $('#batchYear').val('');
    clearErrors();
    $('#departmentsList input[type="checkbox"]').prop('checked', false);
}

function resetSaveButton(text) {
    $('#saveBatchBtn').prop('disabled', false).text(text);
}

// Utility function to show notifications
function showBatchNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `batch-notification batch-notification-${type}`;
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


function showError(message) {
    // Implement your error notification system
    alert('Error: ' + message);
}

// Modal functions (assuming these exist in your main script)
function showModal(modalId) {
    $('#' + modalId).show();
}

function hideModal(modalId) {
    $('#' + modalId).hide();
}