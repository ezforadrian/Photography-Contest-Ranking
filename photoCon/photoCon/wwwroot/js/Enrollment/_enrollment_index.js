//OnLoad
$(function () {
    $("#fieldUserType").trigger("change");
    $("#btnUpdateUser").hide();
    $("#btnCancelEditMode").hide();
    $("#fieldDepartment").prop("disabled", true);
    $("#fieldJobTitle").prop("disabled", true);
    getUsersList();

    $('#modalSystemMessage').on('hide.bs.modal', function (e) {
        $("#txtSystemMessage").html("...");
    });

    $("#fieldUserName").prop("maxlength", 50);
    $("#fieldFirstName").prop("maxlength", 100);
    $("#fieldLastName").prop("maxlength", 100);
    $("#fieldMiddleInitial").prop("maxlength", 50);
    $("#fieldEMail").prop("maxlength", 100);
    $("#fieldPassword").prop("maxlength", 50);
});

//On-Trigger
$("#fieldUserType").on("change", function () {
    if (this.value == "ECDUSER") {
        $("#fieldUserName").prop("placeholder", "##-####");
        $("#fieldFirstName").prop("disabled", true);
        $("#fieldLastName").prop("disabled", true);
        $("#fieldMiddleInitial").prop("disabled", true);
        $("#fieldDepartment").prop("placeholder", "");
        $("#fieldJobTitle").prop("placeholder", "");
        $("#fieldEMail").prop("disabled", true);
        $("#fieldPassword").prop("disabled", true);
        $("#fieldPassword").prop("placeholder", "Active Directory Password");
    }
    else if (this.value == "JUDGE") {
        $("#fieldUserName").prop("placeholder", "");
        $("#fieldFirstName").prop("disabled", false);
        $("#fieldLastName").prop("disabled", false);
        $("#fieldMiddleInitial").prop("disabled", false);
        $("#fieldDepartment").prop("placeholder", "N/A");
        $("#fieldJobTitle").prop("placeholder", "N/A");
        $("#fieldEMail").prop("disabled", false);
        $("#fieldPassword").prop("disabled", false);
        $("#fieldPassword").prop("placeholder", "");
    }
    resetFields();
});

$("#btnEnrollUser").on("click", function () {
    enrollUser();
});

$("#fieldUserName").on("keyup", function () {
    if ($("#fieldUserType").val() == "ECDUSER") {
        getUserInfo(this.value);
    }

});

var userForDeletion = "";
$("#usersTableBody").on("click", ".deleteUser", function () {
    var UserID = $(this).next("input[class='userID']").val();
    userForDeletion = UserID;
    $('#modalConfirmDelete').modal('show');
    //deleteUser(UserID);
    $("#btnCancelEditMode").trigger("click");
});
$("#btnConfirmDelete").on("click", function () {
    deleteUser(userForDeletion);
    userForDeletion = "";
});


$("#usersTableBody").on("click", ".updateUser", function () {
    var UserName = $(this).next("input[class='userName']").val();
    $("#btnEnrollUser").hide();
    $("#btnUpdateUser").show();
    $("#btnCancelEditMode").show();
    getUserInfo_System(UserName);
    $("#fieldUserName").prop("disabled", true);
    $("#fieldUserType").prop("disabled", true);
});

$("#btnCancelEditMode").on("click", function () {
    $("#btnEnrollUser").show();
    $("#btnUpdateUser").hide();
    $("#btnCancelEditMode").hide();

    $("#fieldUserName").prop("disabled", false);
    $("#fieldUserType").prop("disabled", false);
    $("#fieldUserType").prop("selectedIndex", 0);
    $("#fieldUserType").trigger("change");
});

$("#btnUpdateUser").on("click", function () {
    updateUser();
});

$("#fieldPassword").on("keypress", function (e) {
    var validFormat = /^[a-zA-Z0-9]+$/;
    var inputValue = this.value + e.key;
    return validFormat.test(inputValue);
});

//Input Trigger -- Works on Paste AND KeyPress
$("#fieldUserName").on("input", function (e) {
    var object = $(this);
    var objectVal = object.val();
    if ($("#fieldUserType").val() == "ECDUSER") {
        var cleanText = objectVal.replace(/((?![0-9-]).)+/g, "");
        this.value = cleanText.substring(0, 7);
    }
    else {
        var cleanText = objectVal.replace(/((?![a-zA-Z0-9]).)+/g, "");
        this.value = cleanText.substring(0, 50);
    }
});

$("#fieldLastName, #fieldFirstName").on("input", function (e) {
    var object = $(this);
    var objectVal = object.val();
    var cleanText = objectVal.replace(/((?![a-zA-Z ]).)+/g, "");
    this.value = cleanText.substring(0, 50);
});

$("#fieldMiddleInitial").on("input", function (e) {
    var object = $(this);
    var objectVal = object.val();
    var cleanText = objectVal.replace(/((?![a-zA-Z]).)+/g, "");
    this.value = cleanText.substring(0, 2);
});

$("#fieldEMail").on("input", function (e) {
    var object = $(this);
    var objectVal = object.val();
    var cleanText = objectVal.replace(/((?![a-zA-Z0-9@._-]).)+/g, "");
    this.value = cleanText.substring(0, 100);
});

$("#fieldPassword").on("paste", function (e) {
    return false;
});

//Functions
function resetFields() {
    $("#fieldUserName").val("");
    $("#fieldFirstName").val("");
    $("#fieldLastName").val("");
    $("#fieldMiddleInitial").val("");
    $("#fieldDepartment").val("");
    $("#fieldJobTitle").val("");
    $("#fieldEMail").val("");
    $("#fieldPassword").val("");
}

function requiredInput(event, object) {
    var test = object.value;
    var returnValue;
    if (test == "") { returnValue = "" }
    return returnValue;
}

function parseUserNameInput(event, object) {
    if ($("#fieldUserType").val() == "ECDUSER") {
        ////PAGCOR/ECDUSER User
        var validFormat = /(^\d{1,2}$)|(^\d{1,2}[-]$)|(^\d{1,2}[-]\d{1,4}$)/;
        var isFormatValid = validFormat.test(object.value + event.key);
        return (isFormatValid && object.value.length < 7);
    }
    else {
        //JUDGE User
        var validFormat = /(^[a-zA-Z]+)|(^[a-zA-Z]+([a-zA-Z]|\d+)$)/;
        var isFormatValid = validFormat.test(object.value + event.key);
        return (isFormatValid && object.value.length < 50);
    }
}

function getUsersList() {
    $.ajax({
        url: "GetUsersList",
        method: "post",
        type: "json",
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        success: function (data) {
            var tableData = "";
            for (x = 0; x < data.length; x++) {
                tableData = tableData + "<tr>";
                tableData = tableData + "<td>" + data[x].userName + "</td>";
                tableData = tableData + "<td>" + data[x].firstName + "</td>";
                tableData = tableData + "<td>" + data[x].lastName + "</td>";
                tableData = tableData + "<td>" + data[x].middleName + "</td>";
                tableData = tableData + "<td>" + data[x].email + "</td>";
                tableData = tableData + "<td>";

                tableData = tableData + "<input type='button' value='Edit' class='btn btn-secondary updateUser' style='padding:3px;width:100px;'> ";
                tableData = tableData + "<input type='hidden' value='" + data[x].userName + "' class='userName'>";

                tableData = tableData + "<input type='button' value='Delete' class='btn btn-secondary deleteUser' style='padding:3px;width:100px;'>";
                tableData = tableData + "<input type='hidden' value='" + data[x].userName + "' class='userID'>";

                tableData = tableData + "</td>";
                tableData = tableData + "<td></td>";
                tableData = tableData + "</tr>";
            }
            if ($.fn.dataTable.isDataTable("#usersTable")) {
                $("#usersTable").DataTable().destroy();
            }
            $("#usersTableBody").html(tableData);
            $("#usersTable").DataTable();
        }
    });
}

function enrollUser() {
    if ($("#fieldUserName").val() != "") {
        var newData = {
            UserName: $("#fieldUserName").val(),
            FirstName: $("#fieldFirstName").val(),
            LastName: $("#fieldLastName").val(),
            MiddleName: $("#fieldMiddleInitial").val(),
            Department: $("#fieldDepartment").val(),
            PayClass: $("#fieldJobTitle").val(),
            EMail: $("#fieldEMail").val(),
            Password: $("#fieldPassword").val(),
            UserRole: $("#fieldUserType").val()
        }

        $.ajax({
            url: "EnrollNewUser",
            type: "POST",
            contentType: "application/json",
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            data: JSON.stringify(newData),
            success: function (data) {
                var ProcessStatus = data.processStatus;
                var ProcessMessage = data.processMessage;

                if (ProcessStatus == "1") {
                    $("#txtSystemMessage").html("User Enrolled");
                    $('#modalSystemMessage').modal('show');
                    resetFields();
                    getUsersList();
                }
                else if (ProcessStatus == "2") {
                    $("#txtSystemMessage").html("User Enrollment Failed. Invalid PAGCOR ID");
                    $('#modalSystemMessage').modal('show');
                }
                else if (ProcessStatus == "3") {
                    $("#txtSystemMessage").html("User Enrollment Failed. UserName Field Cannot be blank");
                    $('#modalSystemMessage').modal('show');
                }
                else if (ProcessStatus == "4") {
                    $("#txtSystemMessage").html("User Enrollment Failed. An Input Field has invalid format<br><br><span style='font-size:15px;color:red;'>" + ProcessMessage);
                    $('#modalSystemMessage').modal('show');
                }
                else if (ProcessStatus == "5") {
                    $("#txtSystemMessage").html("User Enrollment Failed. Input Field cannot be blank/empty");
                    $('#modalSystemMessage').modal('show');
                }
                else if (ProcessStatus == "6") {
                    $("#txtSystemMessage").html("User Enrollment Failed. UserName already exists");
                    $('#modalSystemMessage').modal('show');
                }
                else {
                    $("#txtSystemMessage").html("User Enrollment Failed. Unknown Error");
                    $('#modalSystemMessage').modal('show');
                }

            }
        });
    }
    else {
        $("#txtSystemMessage").html("User Enrollment Failed. UserName Field Cannot be blank");
        $('#modalSystemMessage').modal('show');
    }
}

//Used for AD Users when inputting UserName
function getUserInfo(userName) {
    var UserName = userName;
    $.ajax({
        url: "GetEmployeeInfo",
        method: "post",
        type: "json",
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        data: { UserName },
        success: function (data) {
            if (data.userName != "") {
                $("#fieldFirstName").val(data.firstName);
                $("#fieldLastName").val(data.lastName);
                $("#fieldMiddleInitial").val(data.middleName);
                $("#fieldDepartment").val(data.department);
                $("#fieldJobTitle").val(data.payClass);
                $("#fieldEMail").val(data.email);
                $("#fieldPassword").val("");
            }
            else {
                $("#fieldFirstName").val("");
                $("#fieldLastName").val("");
                $("#fieldMiddleInitial").val("");
                $("#fieldDepartment").val("");
                $("#fieldJobTitle").val("");
                $("#fieldEMail").val("");
                $("#fieldPassword").val("");
            }
        }
    });
}

//Used for System Users when editing
function getUserInfo_System(UserName) {
    $.ajax({
        url: "GetUserInfo",
        method: "post",
        type: "json",
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        data: { UserName },
        success: function (data) {
            if (data.userName != "") {
                $("#fieldUserType").val(data.userRole);
                $("#fieldUserType").trigger("change");
                $("#fieldUserName").val(data.userName);
                $("#fieldFirstName").val(data.firstName);
                $("#fieldLastName").val(data.lastName);
                $("#fieldMiddleInitial").val(data.middleName);
                $("#fieldDepartment").val(data.department);
                $("#fieldJobTitle").val(data.payClass);
                $("#fieldEMail").val(data.email);
                $("#fieldPassword").val("");
            }
            else {
                $("#fieldUserType").val("");
                $("#fieldUserName").val("");
                $("#fieldFirstName").val("");
                $("#fieldLastName").val("");
                $("#fieldMiddleInitial").val("");
                $("#fieldDepartment").val("");
                $("#fieldJobTitle").val("");
                $("#fieldEMail").val("");
                $("#fieldPassword").val("");
            }
        }
    });
}

function updateUser() {
    if ($("#fieldUserName").val() != "") {
        var newData = {
            UserName: $("#fieldUserName").val(),
            FirstName: $("#fieldFirstName").val(),
            LastName: $("#fieldLastName").val(),
            MiddleName: $("#fieldMiddleInitial").val(),
            Department: $("#fieldDepartment").val(),
            PayClass: $("#fieldJobTitle").val(),
            EMail: $("#fieldEMail").val(),
            Password: $("#fieldPassword").val(),
            UserRole: $("#fieldUserType").val()
        }

        $.ajax({
            url: "UpdateUser",
            type: "POST",
            contentType: "application/json",
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            data: JSON.stringify(newData),
            success: function (data) {
                //alert(data);
                $("#txtSystemMessage").html(data);
                $('#modalSystemMessage').modal('show');

                $("#btnCancelEditMode").trigger("click");
                getUsersList();
            }
        });
    }
    else {
        $("#txtSystemMessage").html("User Enrollment Failed. Invalid UserName");
        $('#modalSystemMessage').modal('show');
    }
}

function deleteUser(userID) {
    var newData = {
        UserName: userID
    }
    $.ajax({
        url: "DeleteUser",
        type: "POST",
        contentType: "application/json",
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        data: JSON.stringify(newData),
        success: function (data) {
            //alert(data);
            $("#txtSystemMessage").html(data);
            $('#modalSystemMessage').modal('show');

            resetFields();
            getUsersList();
        }
    });
}