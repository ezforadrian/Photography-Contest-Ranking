$(document).ready(function () {

    $('#successModal, #errorModal, #confirmDeleteModal, #confirmOpenModal, #confirmCloseInitialRoundModal').modal({
        backdrop: false
    });


    $('#grandFinalListTable').DataTable({
        "paging": true,
        "select": true,
        "processing": true,
        "serverSide": true, // Enable server-side processing
        ajax: {
            url: '/GrandFinalSetup/GetAllGrandFinalDates',
            type: 'GET',
            data: function (d) {
                d.start = d.start || 0;
                d.length = d.length || 5;
            },
            "dataSrc": "data", // Assuming your data is wrapped in a "data" property
        },
        columns: [
            { data: 'id_', className: 'text-center' },
            { data: 'filler01', className: 'text-center' },
            { data: 'parameterValue', className: 'text-center' },
            {
                data: 'detailedDescription', className: 'text-center',
                render: function (data, type, row) {
                    return (data === 'Close') ? 'Close' : 'Open';
                }
            },
            {
                data: 'filler02', className: 'text-center',
                render: function (data, type, row) {
                    if (row.detailedDescription === 'Close') {
                        if (data === 'False') {
                            return '-';
                        }
                        return (data === 'False') ? '-' : 'Completed';
                    } else if (row.detailedDescription === 'Open') {
                        return (data === 'False') ? 'Ongoing' : 'Completed';
                    }
                }
            },
            {
                data: 'filler03', className: 'text-center',
                render: function (data, type, row) {
                    return (row.filler02 === 'False') ? '-' : data;
                }
            },
            {
                data: null, className: 'text-justify',
                render: function (data, type, row, meta) {
                    var showLabel = '';
                    var previousRow = meta.row > 0 ? $('#grandFinalListTable').DataTable().row(meta.row - 1).data() : null;
                    var showOpenButton = row.detailedDescription === 'Close' && (!previousRow || previousRow.filler02.trim() === 'True') && row.filler02 != 'True';
                    var showEditButton = row.detailedDescription === 'Open' && (!previousRow || previousRow.filler02 === 'True');
                    var openOrClose = (row.detailedDescription === 'Close') ? 'Open' : 'Close';
                    var showDetails = row.filler02 == 'True';


                    var buttons = '';

                    if (showOpenButton) {
                        buttons += '<button class="btn btn-primary btn-sm open-close-btn me-2" data-id="' + row.hashIndex + '">' + openOrClose + '</button> ';
                    }

                    if (showEditButton) {
                        buttons += '<button class="btn btn-secondary btn-sm edit-btn me-2" data-id="' + row.hashIndex + '">Edit</button> ';
                    }
                    buttons += '<button class="btn btn-danger btn-sm remove-btn me-2" data-id="' + row.hashIndex + '">Remove</button>';

                    if (showDetails) {
                        buttons += '<button class="btn btn-primary btn-sm edit-btn me-2" data-id="' + row.hashIndex + '">Details</button> ';
                    }
                    return buttons;
                }
            }
        ],
        "lengthChange": false, // Remove the show entries drop-down
        "pageLength": 20, // Set the initial page length
        "ordering": false, // Disable column sorting
        "searching": false,
        language: {
            paginate: {
                next: "Next",
                previous: "Previous"
            }
        }
    });


    $('#grandfinalDateForm').submit(function (e) {
        e.preventDefault();
        var token = $('input[name="__RequestVerificationToken"]').val();
        var grandfinalStartDate = $('#grandfinalStartDate').val();

        var formData = {
            prelimStartDate: grandfinalStartDate

        };

        $.ajax({
            type: 'POST',
            url: 'AddGrandFinalDates',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                $('#grandfinalStartDate').val('');
                $('#grandFinalListTable').DataTable().ajax.reload();

                // Show success message in the success modal
                $('#successModalBody').html(response.message);
                $('#successModal').modal('show');


            },
            error: function (xhr, status, error) {
                var errors = [];
                if (xhr.responseJSON && xhr.responseJSON.Errors) {
                    errors = xhr.responseJSON.Errors;
                } else {
                    errors.push(xhr.responseText);
                }

                var errorMessage = '<ul>';
                errors.forEach((e, index) => {
                    errorMessage += `<li>${e}</li>`;
                });
                errorMessage += '</ul>';

                $('#errorModalBody').html(errorMessage);
                $('#errorModal').modal('show');
            }
        });
    });

    $('#grandFinalListTable').on('click', '.remove-btn', function () {
        var rowId = $(this).data('id');
        var token = $('input[name="__RequestVerificationToken"]').val();
        $('#confirmDeleteModal').modal('show');

        var formData = {
            HashId: rowId
        };

        $('#confirmDeleteBtn').off('click').on('click', function () {
            $.ajax({
                type: 'POST',
                url: '/GrandFinalSetup/DeleteFinalDate',
                contentType: 'application/json',
                data: JSON.stringify(formData),
                headers: { 'RequestVerificationToken': token },
                success: function (response) {
                    $('#confirmDeleteModal').modal('hide');
                    $('#successModalBody').html(response.message);
                    $('#successModal').modal('show');
                    $('#grandFinalListTable').DataTable().ajax.reload();
                },
                error: function (xhr, status, error) {
                    $('#confirmDeleteModal').modal('hide');
                    var errors = xhr.responseJSON && xhr.responseJSON.Errors ? xhr.responseJSON.Errors : [xhr.responseText];
                    var errorMessage = '<ul>';
                    errors.forEach(function (e) {
                        errorMessage += '<li>' + e + '</li>';
                    });
                    errorMessage += '</ul>';
                    $('#errorModalBody').html(errorMessage);
                    $('#errorModal').modal('show');
                }
            });
        });
    });


    // Event delegation for Open/Close button
    $('#grandFinalListTable').on('click', '.open-close-btn', function () {
        var rowId = $(this).data('id');
        var token = $('input[name="__RequestVerificationToken"]').val();
        $('#confirmOpenModal').modal('show');

        var formData = {
            HashId: rowId
        };

        $('#confirmOpenBtn').off('click').on('click', function () {
            $.ajax({
                type: 'POST',
                url: '/GrandFinalSetup/UpdateDetailedDescription',
                contentType: 'application/json',
                data: JSON.stringify(formData),
                headers: { 'RequestVerificationToken': token },
                success: function (response) {
                    $('#confirmOpenModal').modal('hide');
                    $('#successModalBody').html(response.message);
                    $('#successModal').modal('show');
                    $('#grandFinalListTable').DataTable().ajax.reload();
                },
                error: function (xhr, status, error) {
                    $('#confirmOpenModal').modal('hide');
                    var errors = xhr.responseJSON && xhr.responseJSON.Errors ? xhr.responseJSON.Errors : [xhr.responseText];
                    var errorMessage = '<ul>';
                    errors.forEach(function (e) {
                        errorMessage += '<li>' + e + '</li>';
                    });
                    errorMessage += '</ul>';
                    $('#errorModalBody').html(errorMessage);
                    $('#errorModal').modal('show');
                }
            });
        });
    });


    // Event delegation for Edit button
    $('#grandFinalListTable').on('click', '.edit-btn', function () {
        $('#RegionalFinNumber').val("");
        $("#inclusion").prop("checked", false);


        var rowId = $(this).data('id');
        var token = $('input[name="__RequestVerificationToken"]').val();

        var formData = {
            HashId: rowId
        };

        $.ajax({
            type: 'POST',
            url: 'OpenInfoEditModal',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                $('#hashRowIdProcessParam').val(response.hashId);
                $('#imageBatchCount').val(response.imageBatchCount);
                $('#gfdayNumber').val(response.paramInfo.filler01);
                $('#gfdate').val(response.paramInfo.parameterValue);




                // Populate the category checkboxes
                var categoryCheckboxes = $('#categoryCheckboxes');
                categoryCheckboxes.empty();
                $.each(response.categoryList, function (index, category) {
                    var isChecked = category.batchImageCount > 0 ? 'checked' : '';

                    var isEnabled;
                    if (category.photoLocationCount > 0 && response.isUpdateEnabled && category.readonly == false) {
                        isEnabled = '';
                    } else {
                        isEnabled = 'disabled';
                    }
                    //var isEnabled = category.photoLocationCount > 0 ? '' : 'disabled';
                    var checkbox = $('<div class="form-check">')
                        .append('<input class="form-check-input" type="checkbox" value="' + category.hashId + '" id="category' + category.hashId + '" ' + isChecked + ' ' + isEnabled + '>')
                        .append('<label class="form-check-label" for="category' + category.hashId + '">' + category.name + '</label>');
                    categoryCheckboxes.append(checkbox);
                });

                if (response.paramInfo.filler08 == "True") {
               
                    $("#inclusion").prop("checked", true);
                }


                if (!response.isUpdateEnabled) {
                    $('#updateBatch button[type="submit"]').prop('disabled', true);
                    $('#inclusion').prop('disabled', true);

                } else {
                    $('#updateBatch button[type="submit"]').prop('disabled', false);
                    $('#inclusion').prop('disabled', false);
                  
                }



                if (!response.isUpdateCloseEnabledGrandFinal) {;
                    $('#btnCompleteDailyRound').prop('disabled', true);
                    $('#RegionalFinNumber').prop('readonly', true);
                    $('#RegionalFinNumber').val(response.paramInfo.filler03);
                }
                else {
                    $('#btnCompleteDailyRound').prop('disabled', false);
                    $('#RegionalFinNumber').prop('readonly', false);
                    $('#RegionalFinNumber').val(response.paramInfo.filler03);
                }



                // Show the edit modal
                $('#editInfoModal').modal('show');
            },
            error: function (xhr, status, error) {
                // Handle error
                var errors = [];
                if (xhr.responseJSON && xhr.responseJSON.Errors) {
                    errors = xhr.responseJSON.Errors;
                } else {
                    errors.push(xhr.responseText);
                }

                var errorMessage = '<ul>';
                errors.forEach((e, index) => {
                    errorMessage += `<li>${e}</li>`;
                });
                errorMessage += '</ul>';

                $('#errorModalBody').html(errorMessage);
                $('#errorModal').modal('show');
            }
        });

    });


    // Validate at least one checkbox is checked before submitting the form
    $('#updateBatch').on('submit', function (e) {
        var inclusion = false;
        if ($('#inclusion').is(':checked')) {
            inclusion = true;
        };

        var categoryChecked = $('#categoryCheckboxes input[type="checkbox"]:checked').length > 0;

        if (!categoryChecked) {
            e.preventDefault();
            var errorMessage = '<ul><li>Please select at least one region or category.</li></ul>';
            $('#errorModalBody').html(errorMessage);

            // Hide the current modal backdrop to ensure the error modal is visible
            $('#editInfoModal').modal('hide');
            $('#errorModal').on('hidden.bs.modal', function () {
                $('#editInfoModal').modal('show');
            });

            $('#errorModal').modal('show');
        } else {

            var selectedCategories = $('#categoryCheckboxes input[type="checkbox"]:checked').map(function () {
                return $(this).val();
            }).get();


            // Create the data object to be sent
            var dataToSend = {
                hashRowIdProcessParam: $('#hashRowIdProcessParam').val(),
                selectedCategories: selectedCategories,
                Inclusion : inclusion
            };

            // Send the data via AJAX
            $.ajax({
                type: 'POST',
                url: 'SubmitBatch',
                contentType: 'application/json',
                data: JSON.stringify(dataToSend),
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                success: function (response) {
                    if (response.responseCode === "200") {
                        //success
                        $('#editInfoModal').modal('hide');
                        $('#successModalBody').html(response.message);
                        $('#successModal').modal('show');
                        $('#grandFinalListTable').DataTable().ajax.reload();

                    }

                },
                error: function (xhr, status, error) {
                    //console.error('Error submitting batch:', error);
                    $('#editInfoModal').modal('hide');
                    var errors = xhr.responseJSON && xhr.responseJSON.Errors ? xhr.responseJSON.Errors : [xhr.responseText];
                    var errorMessage = '<ul>';
                    errors.forEach(function (e) {
                        errorMessage += '<li>' + e + '</li>';
                    });
                    errorMessage += '</ul>';
                    $('#errorModalBody').html(errorMessage);
                    $('#errorModal').modal('show');
                }
            });

            // Prevent default form submission to allow AJAX handling
            e.preventDefault();
        }
    });

    $('#btnCompleteDailyRound').on('click', function () {
        var RegionalFinNumber = $('#RegionalFinNumber').val();

        if (!RegionalFinNumber) {
            $('#RegionalFinNumber').focus().css('border', '2px solid red');
            return;
        } else {
            $('#RegionalFinNumber').css('border', '');
        }

        $('#confirmDailyFinNumber').val(RegionalFinNumber);
        $('#confirmCloseDailyRoundModal').modal('show');
    });

    $('#confirmCloseDailyRoundBtn').on('click', function () {
        var dailyFinNumber = $('#confirmDailyFinNumber').val();
        var token = $('input[name="__RequestVerificationToken"]').val();

        var formData = {
            QualifyingNumber: dailyFinNumber,
            RoundInfo: '1'
        };

        $.ajax({
            type: 'POST',
            url: '/GrandFinalSetup/CompleteDailyRound',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: { 'RequestVerificationToken': token },
            success: function (response) {

                $('#confirmCloseDailyRoundModal').modal('hide');
                $('#editInfoModal').modal('hide');
                $('#successModalBody').html(response.message);
                $('#successModal').modal('show');
                $('#grandFinalListTable').DataTable().ajax.reload();
            },
            error: function (xhr, status, error) {
                $('#editInfoModal').modal('hide');
                $('#confirmCloseInitialRoundModal').modal('hide');
                var errors = xhr.responseJSON && xhr.responseJSON.Errors ? xhr.responseJSON.Errors : [xhr.responseText];
                var errorMessage = '<ul>';
                errors.forEach(function (e) {
                    errorMessage += '<li>' + e + '</li>';
                });
                errorMessage += '</ul>';
                $('#errorModalBody').html(errorMessage);
                $('#errorModal').modal('show');
            }
        });
    });
    
});