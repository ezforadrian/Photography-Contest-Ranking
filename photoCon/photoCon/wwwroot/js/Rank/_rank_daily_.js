
function openModalFS(param1, param2, param3, param4, param5, param6, param7) {

    //initialize modal
    document.getElementById("judgeScoreModal").value = "";
    document.getElementById("judgeScoreModal").readOnly = false;
    if (param3 != "0.00") {
        document.getElementById("imgRankMModal").innerText = param4;
        document.getElementById("imgScoreModal").innerText = param3;

    }

    if (param7 != "0.00") {
        document.getElementById("judgeScoreModal").value = param7;
        document.getElementById("judgeScoreModal").readOnly = true;
        document.getElementById("lockscoreBtn").innerHTML = "Unlocked";

    }

    document.getElementById("photoIdxModal").value = param2;
    document.getElementById("imageFullModalLabel").innerText = param5 + "-" + param6;
    document.getElementById("ImageModal").src = param1;
    $('#imageFullModal').modal('show');
}
$(document).ready(function () {
    $("#lockscoreBtn").click(function () {
        if (document.getElementById("lockscoreBtn").innerHTML == "Submit") {
            var token = $('input[name="__RequestVerificationToken"]').val();
            var photoIdx = $("#photoIdxModal").val();
            var judgescore = $("#judgeScoreModal").val(); 


            var postData = {
                PhotoId: photoIdx,
                Score: judgescore
            };


            $.ajax({
                url: "/Rank/AddUpdateScore",
                method: "POST",
                contentType: "application/json",
                headers: { 'RequestVerificationToken': token },
                data: JSON.stringify(postData), // Serialize postData to JSON
                success: function (data) {
                    if (data.serverCode == 200) { // Accessing serverCode directly
                        $("#judgeScoreModal").val(parseFloat(judgescore).toFixed(2));
                        $("#lockscoreBtn").text("Unlock");
                        $("#judgeScoreModal").prop("readonly", true);
                        $('#imageFullModal').modal('hide');
                    } else {
                        $("#judgeScoreModal").text("Submit");
                        $("#judgeScoreModal").prop("readonly", false);
                        $('#imageFullModal').modal('hide');
                        // Show warning modal for user notification
                        $('#errorModalMessage').text(data.message);
                        $('#errorModalTitle').text("Error Code: " + data.serverCode);

                        $('#errorModal').modal('show');
                    }
                },
                error: function (xhr, status, error) {
                    console.log(xhr.responseText);
                }
            });

        } else {
            document.getElementById("judgeScoreModal").readOnly = false;
            document.getElementById("lockscoreBtn").innerHTML = "Submit";
        }
    });
});



