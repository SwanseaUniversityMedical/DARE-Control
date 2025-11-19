// DMN Rule Manager JavaScript - Fixed for Keycloak Authentication
let dmnTable = null;
let dataTable = null;

// API Base URL - will be set from the page or use default
const API_BASE_URL = (typeof TRE_API_BASE_URL !== 'undefined' ? TRE_API_BASE_URL : '') + '/api/Dmn';

function getAuthToken() {
    let token = sessionStorage.getItem('authToken');
    if (token) {
        console.log('[OK] Token found in sessionStorage');
        return token;
    }

    token = localStorage.getItem('authToken');
    if (token) {
        console.log('[OK] Token found in localStorage');
        return token;
    }

    const metaToken = document.querySelector('meta[name="auth-token"]');
    if (metaToken) {
        token = metaToken.getAttribute('content');
        if (token) {
            console.log('[OK] Token found in meta tag');
            return token;
        }
    }

    const inputToken = document.querySelector('input[name="authToken"]');
    if (inputToken) {
        token = inputToken.value;
        if (token) {
            console.log('[OK] Token found in hidden input');
            return token;
        }
    }

    console.warn('[ERROR] No authentication token found. API calls will fail with 401 Unauthorized.');
    return null;
}

/**
 * Setup jQuery AJAX to include authorization header with all requests
 */
$.ajaxSetup({
    beforeSend: function (xhr) {
        const token = getAuthToken();
        if (token) {
            xhr.setRequestHeader('Authorization', 'Bearer ' + token);
            console.log('[OK] Authorization header set on request');
        } else {
            console.warn('[WARNING] Authorization header NOT set - token is missing');
        }
    }
});

// Initialize on page load
$(document).ready(function () {
    console.log('DMN Manager initialized');
    console.log('API Base URL:', API_BASE_URL);

    loadDmnTable();

    // Event Listeners
    $('#addRuleBtn').on('click', showAddRuleModal);
    $('#refreshBtn').on('click', loadDmnTable);
    $('#validateBtn').on('click', validateDmn);
    $('#deployBtn').on('click', deployDmn);
    $('#saveRuleBtn').on('click', saveRule);
    $('#confirmDeleteBtn').on('click', deleteRule);
});

/**
 * Load the complete DMN table from API
 */
function loadDmnTable() {
    showLoading(true);
    hideAlert();

    $.ajax({
        url: API_BASE_URL + '/table',
        method: 'GET',
        xhrFields: {
            withCredentials: true
        },
        success: function (data) {
            dmnTable = data;
            displayDmnInfo(data);
            buildTableHeaders(data);
            displayRules(data);
            showLoading(false);
            console.log('[OK] DMN table loaded successfully');
        },
        error: function (xhr, status, error) {
            showLoading(false);
            console.error('[ERROR] Error loading DMN tableeeee:', {
                status: xhr.status,
                statusText: xhr.statusText,
                responseText: xhr.responseText,
                error: error
            });

            let errorMessage = 'Error loading DMN table';
            if (xhr.status === 401) {
                errorMessage += ': Unauthorized - Your session may have expired. Please refresh the page.';
            } else if (xhr.status === 403) {
                errorMessage += ': Forbidden - You do not have permission to access this resource. Check your Keycloak role assignments.';
            } else {
                errorMessage += ': ' + (xhr.responseJSON?.message || xhr.statusText || error);
            }

            showAlert(errorMessage, 'danger');
        }
    });
}

/**
 * Display DMN table information
 */
function displayDmnInfo(table) {
    $('#decisionId').text(table.decisionId);
    $('#decisionName').text(table.decisionName);
    $('#hitPolicy').text(table.hitPolicy);
    $('#dmnInfo').show();
}

/**
 * Build dynamic table headers based on inputs and outputs (excluding submissionId)
 */
function buildTableHeaders(table) {
    const headerRow = $('#rulesTable thead tr');

    // Remove existing dynamic columns
    headerRow.find('th:not(:first):not(:nth-child(2)):not(:last)').remove();

    // Add input columns (excluding submissionId)
    let visibleInputCount = 0;
    table.inputs.forEach(input => {
        const label = input.label || input.expression || 'Input';
        // Skip submissionId input
        if (label.toLowerCase() === 'submissionid') {
            return;
        }
        headerRow.find('th:nth-child(2)').after(`<th class="text-dark bg-light">Input: ${label}</th>`);
        visibleInputCount++;
    });

    // Add output columns (after inputs and description)
    const lastInputIndex = 2 + visibleInputCount;
    table.outputs.forEach(output => {
        const label = output.label || output.name || 'Output';
        headerRow.find(`th:nth-child(${lastInputIndex})`).after(`<th class="text-dark bg-light">Output: ${label}</th>`);
    });
}

/**
 * Display rules in the DataTable (excluding submissionId column)
 */
function displayRules(table) {
    const tbody = $('#rulesTable tbody');
    tbody.empty();

    table.rules.forEach((rule, index) => {
        const row = $('<tr>');

        // Rule number
        row.append(`<td>${index + 1}</td>`);

        // Description
        row.append(`<td>${rule.description || '-'}</td>`);

        // Input entries (excluding submissionId)
        rule.inputEntries.forEach((entry, inputIndex) => {
            const input = table.inputs[inputIndex];
            const label = input.label || input.expression || 'Input';

            // Skip submissionId input
            if (label.toLowerCase() === 'submissionid') {
                return;
            }

            const displayText = entry.text === '-' ? '<em>Any</em>' :
                entry.text === '' ? '<em>Empty</em>' :
                    entry.text;
            row.append(`<td>${displayText}</td>`);
        });

        // Output entries
        rule.outputEntries.forEach(entry => {
            const displayText = entry.text || '<em>Empty</em>';
            row.append(`<td><code>${displayText}</code></td>`);
        });

        // Actions
        const actions = `
            <button class="btn btn-sm btn-outline-primary edit-rule" data-rule-id="${rule.id}" title="Edit Rule">
                <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-outline-danger delete-rule" data-rule-id="${rule.id}" title="Delete Rule">
                <i class="fas fa-trash"></i>
            </button>
        `;
        row.append(`<td class="text-nowrap">${actions}</td>`);

        tbody.append(row);
    });

    // Initialize or reload DataTable
    if (dataTable) {
        dataTable.destroy();
    }

    dataTable = $('#rulesTable').DataTable({
        pageLength: 25,
        order: [[0, 'asc']],
        responsive: true,
        language: {
            emptyTable: "No rules found in the DMN table"
        }
    });

    $('#rulesTable').show();

    // Bind action buttons
    $('.edit-rule').on('click', function () {
        const ruleId = $(this).data('rule-id');
        showEditRuleModal(ruleId);
    });

    $('.delete-rule').on('click', function () {
        const ruleId = $(this).data('rule-id');
        showDeleteModal(ruleId);
    });
}

/**
 * Show modal for adding a new rule (submissionId hidden and defaults to "-" for Any)
 */
function showAddRuleModal() {
    if (!dmnTable) {
        showAlert('Please wait for DMN table to load', 'warning');
        return;
    }

    $('#ruleModalLabel').text('Add New Rule');
    $('#ruleId').val('');
    $('#isEdit').val('false');
    $('#ruleDescription').val('');

    // Build input fields (hide submissionId and set default to "-")
    const inputContainer = $('#inputFieldsContainer');
    inputContainer.empty();
    dmnTable.inputs.forEach((input, index) => {
        const label = input.label || input.expression || `Input ${index + 1}`;

        // Hide submissionId input and store default value
        if (label.toLowerCase() === 'submissionid') {
            inputContainer.append(`
                <input type="hidden" class="input-value" data-index="${index}" value="-">
            `);
            return;
        }

        inputContainer.append(`
            <div class="mb-2">
                <label class="form-label">${label}</label>
                <input type="text" class="form-control input-value" data-index="${index}"
                       placeholder="Enter value (use '-' for any, leave empty for none)">
                <small class="form-text text-muted">Tip: Use '-' to match any value, leave empty for no value</small>
            </div>
        `);
    });

    // Build output fields
    const outputContainer = $('#outputFieldsContainer');
    outputContainer.empty();
    dmnTable.outputs.forEach((output, index) => {
        const label = output.label || output.name || `Output ${index + 1}`;
        outputContainer.append(`
            <div class="mb-2">
                <label class="form-label">${label}</label>
                <input type="text" class="form-control output-value" data-index="${index}"
                       placeholder='Enter value (e.g., "stringValue" or expression)'>
                <small class="form-text text-muted">Tip: Use quotes for string literals, expressions for calculations</small>
            </div>
        `);
    });

    $('#ruleModal').modal('show');
}

/**
 * Show modal for editing an existing rule (submissionId hidden)
 */
function showEditRuleModal(ruleId) {
    const rule = dmnTable.rules.find(r => r.id === ruleId);
    if (!rule) {
        showAlert('Rule not found', 'danger');
        return;
    }

    $('#ruleModalLabel').text('Edit Rule');
    $('#ruleId').val(rule.id);
    $('#isEdit').val('true');
    $('#ruleDescription').val(rule.description || '');

    // Build input fields with existing values (hide submissionId)
    const inputContainer = $('#inputFieldsContainer');
    inputContainer.empty();
    dmnTable.inputs.forEach((input, index) => {
        const label = input.label || input.expression || `Input ${index + 1}`;
        const value = rule.inputEntries[index]?.text || '';

        // Hide submissionId input but keep its value
        if (label.toLowerCase() === 'submissionid') {
            inputContainer.append(`
                <input type="hidden" class="input-value" data-index="${index}" value="${value}">
            `);
            return;
        }

        inputContainer.append(`
            <div class="mb-2">
                <label class="form-label">${label}</label>
                <input type="text" class="form-control input-value" data-index="${index}"
                       value="${value}" placeholder="Enter value (use '-' for any, leave empty for none)">
                <small class="form-text text-muted">Tip: Use '-' to match any value, leave empty for no value</small>
            </div>
        `);
    });

    // Build output fields with existing values
    const outputContainer = $('#outputFieldsContainer');
    outputContainer.empty();
    dmnTable.outputs.forEach((output, index) => {
        const label = output.label || output.name || `Output ${index + 1}`;
        const value = rule.outputEntries[index]?.text || '';
        outputContainer.append(`
            <div class="mb-2">
                <label class="form-label">${label}</label>
                <input type="text" class="form-control output-value" data-index="${index}"
                       value="${value}" placeholder='Enter value (e.g., "stringValue" or expression)'>
                <small class="form-text text-muted">Tip: Use quotes for string literals, expressions for calculations</small>
            </div>
        `);
    });

    $('#ruleModal').modal('show');
}

/**
 * Save rule (add or update)
 */
function saveRule() {
    const isEdit = $('#isEdit').val() === 'true';
    const ruleId = $('#ruleId').val();
    const description = $('#ruleDescription').val();

    // Collect input values (includes hidden submissionId)
    const inputValues = [];
    $('.input-value').each(function () {
        inputValues.push($(this).val());
    });

    // Collect output values
    const outputValues = [];
    $('.output-value').each(function () {
        outputValues.push($(this).val());
    });

    // Validate
    if (inputValues.length !== dmnTable.inputs.length) {
        showAlert('Invalid number of input values', 'danger');
        return;
    }

    if (outputValues.length !== dmnTable.outputs.length) {
        showAlert('Invalid number of output values', 'danger');
        return;
    }

    const requestData = isEdit ? {
        ruleId: ruleId,
        description: description,
        inputValues: inputValues,
        outputValues: outputValues
    } : {
        description: description,
        inputValues: inputValues,
        outputValues: outputValues
    };

    const url = isEdit ? API_BASE_URL + '/rules' : API_BASE_URL + '/rules';
    const method = isEdit ? 'PUT' : 'POST';

    $.ajax({
        url: url,
        method: method,
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        xhrFields: {
            withCredentials: true
        },
        success: function (response) {
            $('#ruleModal').modal('hide');
            showAlert(response.message, response.success ? 'success' : 'danger');
            // Reload the table after a brief delay to ensure modal is fully hidden
            setTimeout(function () {
                loadDmnTable();
            }, 300);
        },
        error: function (xhr, status, error) {
            showAlert('Error saving rule: ' + (xhr.responseJSON?.message || error), 'danger');
            console.error('Error saving rule:', xhr);
        }
    });
}

/**
 * Show delete confirmation modal
 */
function showDeleteModal(ruleId) {
    $('#deleteRuleId').val(ruleId);
    $('#deleteModal').modal('show');
}

/**
 * Delete a rule
 */
function deleteRule() {
    const ruleId = $('#deleteRuleId').val();

    $.ajax({
        url: API_BASE_URL + '/rules/' + encodeURIComponent(ruleId),
        method: 'DELETE',
        xhrFields: {
            withCredentials: true
        },
        success: function (response) {
            $('#deleteModal').modal('hide');
            showAlert(response.message, response.success ? 'success' : 'danger');
            // Reload the table after a brief delay to ensure modal is fully hidden
            setTimeout(function () {
                loadDmnTable();
            }, 300);
        },
        error: function (xhr, status, error) {
            $('#deleteModal').modal('hide');
            showAlert('Error deleting rule: ' + (xhr.responseJSON?.message || error), 'danger');
            console.error('Error deleting rule:', xhr);
        }
    });
}

/**
 * Validate DMN
 */
function validateDmn() {
    $.ajax({
        url: API_BASE_URL + '/validate',
        method: 'GET',
        xhrFields: {
            withCredentials: true
        },
        success: function (response) {
            showAlert(response.message, response.success ? 'success' : 'warning');
        },
        error: function (xhr, status, error) {
            showAlert('Validation failed: ' + (xhr.responseJSON?.message || error), 'danger');
            console.error('Validation error:', xhr);
        }
    });
}

/**
 * Deploy DMN to Zeebe
 */
function deployDmn() {
    if (!confirm('Are you sure you want to deploy the DMN to Zeebe?')) {
        return;
    }

    $.ajax({
        url: API_BASE_URL + '/deploy',
        method: 'POST',
        xhrFields: {
            withCredentials: true
        },
        success: function (response) {
            showAlert(response.message, response.success ? 'success' : 'danger');
        },
        error: function (xhr, status, error) {
            showAlert('Deployment failed: ' + (xhr.responseJSON?.message || error), 'danger');
            console.error('Deployment error:', xhr);
        }
    });
}

/**
 * Show/hide loading spinner
 */
function showLoading(show) {
    if (show) {
        $('#loadingSpinner').show();
        $('#rulesTable').hide();
    } else {
        $('#loadingSpinner').hide();
    }
}

/**
 * Show alert message
 */
function showAlert(message, type) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    $('#alertContainer').html(alertHtml);

    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        $('.alert').alert('close');
    }, 5000);
}

/**
 * Hide alert
 */
function hideAlert() {
    $('#alertContainer').empty();
}