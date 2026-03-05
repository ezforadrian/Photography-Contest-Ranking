$(document).ready(function () {
    var token = $('input[name="__RequestVerificationToken"]').val();
    $("#loginForm").submit(function (event) {
        // Prevent default form submission
        event.preventDefault();

        // Check if the username and password fields are not empty
        var username = $("#username_").val().trim();
        var password = $("#password_").val().trim();
        var rememberMe = $("#rememberme_").prop("checked");

        if (username.trim() === "") {
            // Show tooltip for empty username field
            $("#username_").tooltip({
                title: "Please enter your username",
                placement: "bottom"
            });
            $("#username_").tooltip("show");
            return; // Stop further execution
        }

        if (password.trim() === "") {
            // Show tooltip for empty password field
            $("#password_").tooltip({
                title: "Please enter your password",
                placement: "bottom"
            });
            $("#password_").tooltip("show");
            return; // Stop further execution
        }

        var LoginView = {
            Username: username,
            Password: password,
            RememberMe: rememberMe,
        };

        // Send AJAX request
        $.ajax({
            type: "POST",
            url: "/Home/AuthenticateUser",
            contentType: "application/json",
            headers: { 'RequestVerificationToken': token },
            data: JSON.stringify(LoginView),
            success: function (response) {
                var responseData = JSON.parse(response);
                if (responseData.ServerResponseCode === 200) {
                    // Success
                    // Do something if needed
                    window.location.href = '/Home/LoginSuccess';
                } else {
                    // Error
                    // Show error modal based on response code
                    showErrorModal(responseData.ServerResponseMessage, responseData.ServerResponseCode);
                }
            },
            error: function (xhr, status, error) {
                // Handle authentication error
                console.log("Error: " + error);
            }
        });
    });

    function showErrorModal(errorMessage, responseCode) {
        // Set error message inside modal
        $('#errorModalMessage').text(errorMessage);

        // Set modal title based on response code
        if (responseCode === 422) {
            $('#errorModalTitle').text("Account Locked");
        } else if (responseCode === 423) {
            $('#errorModalTitle').text("Not Allowed");
        } else if (responseCode === 421) {
            $('#errorModalTitle').text("Invalid Password");
        } else if (responseCode === 424) {
            $('#errorModalTitle').text("Username Not Found");
        } else {
            $('#errorModalTitle').text("Error");
        }

        // Show modal
        $('#errorModal').modal('show');
    }
});