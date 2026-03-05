$(document).ready(function () {



    $('#uploadButton').click(function () {
        //$('#uploadButton').attr('disabled', 'disabled');
        var fileInput = document.getElementById('fileInput');
        var file = fileInput.files[0];

        if (!file) {
            //alert('No file selected.');
            var errors = "No file selected. Please select file to upload.";
            var errorMessage = '<ul>';
            errorMessage += errors + '</ul>';
            $('#errorModalBody').html(errorMessage);
            $('#errorModal').modal('show');
            document.getElementById('fileInput').value = '';
            return;
        }

        if (file) {
            var formData = new FormData();
            formData.append('file', file);

            $.ajax({
                url: '/CSVSetup/Upload',
                method: "post",
                type: "json",
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    $.ajax({
                        url: '/CSVSetup/UpdateCSV',
                        method: "post",
                        type: "json",
                        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                        success: function (response) {
                            if (response.success) {
                                var success = "Data updated successfully. " + "Uploaded Count: " + response.uploadedCount;
                                var successMessage = '<ul>';
                                successMessage += success + '</ul>';
                                $('#successModalBody').html(successMessage);
                                $('#successModal').modal('show');
                                document.getElementById('fileInput').value = '';
                                $('#uploadButton').removeAttr('disabled');
                                return;
                            } else {
                                var errors = "Failed to update data";
                                var errorMessage = '<ul>';
                                errorMessage += errors + '</ul>';
                                $('#errorModalBody').html(errorMessage);
                                $('#errorModal').modal('show');
                                document.getElementById('fileInput').value = '';
                                $('#uploadButton').removeAttr('disabled');
                            }
                        },
                        error: function (xhr, status, error) {
                            var errors = xhr.responseJSON && xhr.responseJSON.Errors ? xhr.responseJSON.Errors : [xhr.responseText];
                            var errorMessage = '<ul>';
                            errors.forEach(function (e) {
                                errorMessage += '<li>' + e + '</li>';
                            });
                            errorMessage += '</ul>';
                            $('#errorModalBody').html(errorMessage);
                            $('#errorModal').modal('show');
                            $('#uploadButton').removeAttr('disabled');
                        }
                    });

                },
                error: function (xhr, status, error) {
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
        } else {
            //alert('Please select a file to upload.');
            var errors = "No file selected. Please select file to upload1.";
            var errorMessage = '<ul>';
            errorMessage += errors + '</ul>';
            $('#errorModalBody').html(errorMessage);
            $('#errorModal').modal('show');
            document.getElementById('fileInput').value = '';
        }





    });
});
