$(document).ready(function () {
    // Initialize modals without dimming effect
    $('#successModal, #errorModal, #confirmDeleteModal, #confirmOpenModal, #confirmCloseInitialRoundModal').modal({
        backdrop: false
    });

    $('#prelimListTable').DataTable({
        "paging": true,
        "select": true,
        "processing": true,
        "serverSide": true, // Enable server-side processing
        ajax: {
            url: '/InitialSetup/GetAllPrelimDates',
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
                    if (row.detailedDescription == 'Open') {
                        if (data === 'False' && row.filler04 === 'False' && row.filler06 === 'False') {
                            return 'Round 1';
                        } else if (data === 'True' && row.filler04 === 'False' && row.filler06 === 'False') {
                            return 'Round 2';
                        } else if (data === 'True' && row.filler04 === 'True' && row.filler06 === 'False') {
                            return 'Round 3';
                        } else {
                            return 'Round 3';
                        }
                    } else {
                        if (data === 'False' && row.filler06 === 'False') {
                            return '---';
                        } else {
                            if (data === 'False' && row.filler04 === 'False' && row.filler06 === 'False') {
                                return 'Round 1';
                            } else if (data === 'True' && row.filler04 === 'False' && row.filler06 === 'False') {
                                return 'Round 2';
                            } else if (data === 'True' && row.filler04 === 'True' && row.filler06 === 'False') {
                                return 'Round 3';
                            } else {
                                return 'Round 3';
                            }
                        }    
                        
                    }
                        
                  
                }
            },
            {
                data: 'filler03', className: 'text-center',
                render: function (data, type, row) {

                    var filler03 = (data == 0 && row.filler02 == 'False') ? '-' : data;
                    var filler05 = (row.filler05 == 0 && row.filler04 == 'False') ? '-' : row.filler05;
                    var filler07 = (row.filler07 == 0 && row.filler06 == 'False') ? '-' : row.filler07;
                    return (filler03 + '/' + filler05 );
                }
            },
            {
                data: null, className: 'text-center',
                render: function (data, type, row) {
                    if (row.detailedDescription === 'Close') {
                        if (row.filler02 == 'True' && row.filler04 == 'True' && row.filler06 == 'True') {
                            return 'Completed';
                        }
                        return '---';
                    } else {
                        if (row.filler02 == 'True' && row.filler04 == 'True' && row.filler06 == 'True') {
                            return 'Completed';
                        }
                        return 'Ongoing';
                    }
                }
            },
            {
                data: 'filler07', className: 'text-center',
                render: function (data, type, row) {
                    return (row.filler06 === 'False') ? '---' : data;
                }
            },
            {
                data: null, className: 'text-justify',
                render: function (data, type, row, meta) {
                    var showLabel = '';
                    var previousRow = meta.row > 0 ? $('#prelimListTable').DataTable().row(meta.row - 1).data() : null;
                    var showOpenButton = row.detailedDescription === 'Close' && (!previousRow || previousRow.filler06 === 'True') && row.filler06 != 'True';
                    var showEditButton = row.detailedDescription === 'Open' && (!previousRow || previousRow.filler06 === 'True');
                    var openOrClose = (row.detailedDescription === 'Close') ? 'Open' : 'Close';
                    var showDetails = row.filler06 == 'True';
                    


                    var buttons = '';

                    if (showOpenButton) {
                        buttons += '<button class="btn btn-primary btn-sm open-close-btn me-2" data-id="' + row.hashIndex + '">' + openOrClose + '</button> ';
                    }

                    if (showEditButton) {
                        buttons += '<button class="btn btn-secondary btn-sm edit-btn me-2" data-id="' + row.hashIndex + '">Edit</button> ';
                    }

                    if (showOpenButton || showEditButton) {
                        buttons += '<button class="btn btn-danger btn-sm remove-btn me-2" data-id="' + row.hashIndex + '">Remove</button>';
                    }
                    

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

    // Event delegation for Open/Close button
    $('#prelimListTable').on('click', '.open-close-btn', function () {
        var rowId = $(this).data('id');
        var token = $('input[name="__RequestVerificationToken"]').val();
        $('#confirmOpenModal').modal('show');

        var formData = {
            HashId: rowId
        };

        $('#confirmOpenBtn').off('click').on('click', function () {
            $.ajax({
                type: 'POST',
                url: '/InitialSetup/UpdateDetailedDescription',
                contentType: 'application/json',
                data: JSON.stringify(formData),
                headers: { 'RequestVerificationToken': token },
                success: function (response) {
                    $('#confirmOpenModal').modal('hide');
                    $('#successModalBody').html(response.message);
                    $('#successModal').modal('show');
                    $('#prelimListTable').DataTable().ajax.reload();
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
    $('#prelimListTable').on('click', '.edit-btn', function () {
        $('#InitialFinNumber').val("");
        $('#RegionalFinNumber').val("");

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
                
                // Populate the modal with the received data
                $('#hashRowIdProcessParam').val(response.hashId);
                $('#imageBatchCount').val(response.imageBatchCount);
                $('#ppdayNumber').val(response.paramInfo.filler01);
                $('#ppdate').val(response.paramInfo.parameterValue);

                // Populate the region checkboxes
                var regionCheckboxes = $('#regionCheckboxes');
                regionCheckboxes.empty();
                $.each(response.regionList, function (index, region) {
                    var isChecked = region.batchImageCount > 0 ? 'checked' : '';
                    //var isEnabled = region.photoLocationCount > 0 ? '' : 'disabled';
                    var isEnabled;
                    if (region.photoLocationCount > 0 && response.isUpdateEnabled && region.readonly == false) {
                        isEnabled = '';
                    } else {
                        isEnabled = 'disabled';
                    }
                    var checkbox = $('<div class="form-check">')
                        .append('<input class="form-check-input" type="radio" name="regionList" value="' + region.hashId + '" id="region' + region.hashId + '" ' + isChecked + ' ' + isEnabled + '>')
                        .append('<label class="form-check-label" for="region' + region.hashId + '">' + region.name + '</label>');
                    regionCheckboxes.append(checkbox);
                });

                // Populate the category checkboxes
                var categoryCheckboxes = $('#categoryCheckboxes');
                categoryCheckboxes.empty();
                $.each(response.categoryList, function (index, category) {
                    var isChecked;
                    if (response.imageBatchCount <= 0) {
                        isChecked = 'checked';
                    } else {
                        isChecked = category.batchImageCount > 0 ? 'checked' : '';
                    }


                    var isEnabled;
                    if (category.photoLocationCount > 0 && response.isUpdateEnabled) {
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

                // Disable the submit button if isUpdateEnabled is false
                if (!response.isUpdateEnabled) {
                    $('#updateBatch button[type="submit"]').prop('disabled', true);
                } else {
                    $('#updateBatch button[type="submit"]').prop('disabled', false);
                }

                // One
                if (!response.isCloseInitialRoundEnable) {
                    $('#btnCloseInitialRound').prop('disabled', true);
                    $('#InitialFinNumber').prop('readonly', true);
                    $('#InitialFinNumber ').val(response.closeInitialRoundValue);
                }
                else
                {
                    $('#btnCloseInitialRound').prop('disabled', false);
                    $('#InitialFinNumber').prop('readonly', false);
                    $('#InitialFinNumber').val(response.closeInitialRoundValue);
                }

                // Two
                if (!response.isCloseInitialRoundTwoEnable) {
                    $('#btnCloseInitialRoundTwo').prop('disabled', true);
                    $('#InitialFinNumberTwo').prop('readonly', true);
                    $('#InitialFinNumberTwo').val(response.closeInitialRoundTwoValue);
                }
                else {
                    $('#btnCloseInitialRoundTwo').prop('disabled', false);
                    $('#InitialFinNumberTwo').prop('readonly', false);
                    $('#InitialFinNumberTwo').val(response.closeInitialRoundTwoValue);
                }

                // Three
                if (!response.isCompleteDailyRoundEnable) {
                    $('#btnCompleteDailyRound').prop('disabled', true);
                    $('#RegionalFinNumber').prop('readonly', true);
                    $('#RegionalFinNumber').val(response.completeDailyRoundValue);
                }
                else
                {
                    $('#btnCompleteDailyRound').prop('disabled', false);
                    $('#RegionalFinNumber').prop('readonly', false);
                    $('#RegionalFinNumber').val(response.completeDailyRoundValue);
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

    




    //remove btn
    $('#prelimListTable').on('click', '.remove-btn', function () {
        var rowId = $(this).data('id');
        var token = $('input[name="__RequestVerificationToken"]').val();
        $('#confirmDeleteModal').modal('show');

        var formData = {
            HashId: rowId
        };

        $('#confirmDeleteBtn').off('click').on('click', function () {
            $.ajax({
                type: 'POST',
                url: '/InitialSetup/DeletePrelimDate',
                contentType: 'application/json',
                data: JSON.stringify(formData),
                headers: { 'RequestVerificationToken': token },
                success: function (response) {
                    $('#confirmDeleteModal').modal('hide');
                    $('#successModalBody').html(response.message);
                    $('#successModal').modal('show');
                    $('#prelimListTable').DataTable().ajax.reload();
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


    //addbtn
    $('#prelimDateForm').submit(function (e) {
        e.preventDefault();
        var token = $('input[name="__RequestVerificationToken"]').val();
        var prelimDate = $('#prelimStartDate').val();

        var formData = {
            prelimStartDate: prelimDate
        };

        $.ajax({
            type: 'POST',
            url: 'AddPrelimDates', 
            contentType: 'application/json', 
            data: JSON.stringify(formData), 
            headers: { 'RequestVerificationToken': token }, 
            success: function (response) {
                $('#prelimStartDate').val('');
                $('#prelimListTable').DataTable().ajax.reload();

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

    // Validate at least one checkbox is checked before submitting the form
    $('#updateBatch').on('submit', function (e) {
        var regionChecked = $('#regionCheckboxes input[type="radio"]:checked').length > 0;
        var categoryChecked = $('#categoryCheckboxes input[type="checkbox"]:checked').length > 0;

        if (!regionChecked || !categoryChecked) {
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
            // Collect selected region and category values
            var selectedRegions = $('#regionCheckboxes input[type="radio"]:checked').map(function () {
                return $(this).val();
            }).get();

            var selectedCategories = $('#categoryCheckboxes input[type="checkbox"]:checked').map(function () {
                return $(this).val();
            }).get();


            // Create the data object to be sent
            var dataToSend = {
                hashRowIdProcessParam: $('#hashRowIdProcessParam').val(),
                selectedRegions: selectedRegions,
                selectedCategories: selectedCategories
            };


            
             //Send the data via AJAX
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
                        $('#prelimListTable').DataTable().ajax.reload();

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

    //closeRoundOne
    $('#btnCloseInitialRound').on('click', function () {
        var initialFinNumber = $('#InitialFinNumber').val();
        var roundInfo = $('#roundInfoOne').val();
        closeInitialRoundConfirmation(initialFinNumber, roundInfo, 'InitialFinNumber')
    });


    //closeRoundTwo
    $('#btnCloseInitialRoundTwo').on('click', function () {
        var initialFinNumber = $('#InitialFinNumberTwo').val();
        var roundInfo = $('#roundInfoTwo').val();
        closeInitialRoundConfirmation(initialFinNumber, roundInfo, 'InitialFinNumberTwo')
    });

    //closeRoundThree
    $('#btnCompleteDailyRound').on('click', function () {
        var initialFinNumber = $('#RegionalFinNumber').val();
        var roundInfo = $('#roundInfoThree').val();
        closeInitialRoundConfirmation(initialFinNumber, roundInfo, 'RegionalFinNumber')
    });



    function closeInitialRoundConfirmation(qualifyingNumber, roundNumber, identifier, )
    {
        
        var initialFinNumber = qualifyingNumber;
        var roundInfo = roundNumber;
        var inpId = "#" + identifier;

        

        if (initialFinNumber == 0) {
            $(inpId).focus().css('border', '2px solid red');
            return;
        } else {
            $(inpId).css('border', '');
        }


   
        
        $('#confirmInitialFinNumber').val(initialFinNumber);
        $('#roundInfoModal').val(roundInfo); //----
        $('#confirmCloseInitialRoundModal').modal('show');
    }

    $('#confirmCloseInitialRoundBtn').on('click', function () {
        var initialFinNumber = $('#confirmInitialFinNumber').val();
        var roundInfoOne = $('#roundInfoModal').val();
        var token = $('input[name="__RequestVerificationToken"]').val();

        var formData = {
            QualifyingNumber: initialFinNumber,
            RoundInfo: roundInfoOne
        };

        

        $.ajax({
            type: 'POST',
            url: '/InitialSetup/CloseRegionalRound',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: { 'RequestVerificationToken': token },
            success: function (response) {

                $('#confirmCloseInitialRoundModal').modal('hide');
                $('#editInfoModal').modal('hide');
                $('#successModalBody').html(response.message);
                $('#successModal').modal('show');
                $('#prelimListTable').DataTable().ajax.reload();
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

    //$('#btnCompleteDailyRound').on('click', function () {
    //    var RegionalFinNumber = $('#RegionalFinNumber').val();

    //    if (!RegionalFinNumber) {
    //        $('#RegionalFinNumber').focus().css('border', '2px solid red');
    //        return;
    //    } else {
    //        $('#RegionalFinNumber').css('border', '');
    //    }

    //    $('#confirmDailyFinNumber').val(RegionalFinNumber);
    //    $('#confirmCloseDailyRoundModal').modal('show');
    //});

    //$('#confirmCloseDailyRoundBtn').on('click', function () {
    //    var dailyFinNumber = $('#confirmDailyFinNumber').val();
    //    var token = $('input[name="__RequestVerificationToken"]').val();

    //    var formData = {
    //        QualifyingNumber: dailyFinNumber
    //    };

    //    $.ajax({
    //        type: 'POST',
    //        url: '/InitialSetup/CompleteDailyRound',
    //        contentType: 'application/json',
    //        data: JSON.stringify(formData),
    //        headers: { 'RequestVerificationToken': token },
    //        success: function (response) {

    //            $('#confirmCloseDailyRoundModal').modal('hide');
    //            $('#editInfoModal').modal('hide');
    //            $('#successModalBody').html(response.message);
    //            $('#successModal').modal('show');
    //            $('#prelimListTable').DataTable().ajax.reload();
    //        },
    //        error: function (xhr, status, error) {
    //            $('#editInfoModal').modal('hide');
    //            $('#confirmCloseInitialRoundModal').modal('hide');
    //            var errors = xhr.responseJSON && xhr.responseJSON.Errors ? xhr.responseJSON.Errors : [xhr.responseText];
    //            var errorMessage = '<ul>';
    //            errors.forEach(function (e) {
    //                errorMessage += '<li>' + e + '</li>';
    //            });
    //            errorMessage += '</ul>';
    //            $('#errorModalBody').html(errorMessage);
    //            $('#errorModal').modal('show');
    //        }
    //    });
    //});





});