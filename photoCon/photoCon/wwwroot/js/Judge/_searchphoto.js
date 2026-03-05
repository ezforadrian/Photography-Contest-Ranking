function openModal(param2, param3) {

    const token = $('input[name="__RequestVerificationToken"]').val();
    var hashPhoto = param2;
    var roundHash = param3;

    var postData = {
        HashId: hashPhoto,
        HashRound: roundHash
    };

    //console.log(postData);

    $.ajax({
        url: "/Rank/MyRankGetInfo",
        method: "POST",
        contentType: "application/json",
        headers: { 'RequestVerificationToken': token },
        data: JSON.stringify(postData), // Serialize postData to JSON
        success: function (data) {
            if (data.serverCode == 200) {
                const encodedLocation = encodeURIComponent(data.imageInfo.location);
                const encodedDescription = encodeURIComponent(data.imageInfo.description);
                const encodedTitle = encodeURIComponent(data.imageInfo.photoTitle);
                const encodedDateTaken = encodeURIComponent(data.imageInfo.photoTaken);

                const newImgUrl = data.imageInfo.imageUrl;

                const imgTitle = "Title: " + decodeURIComponent(encodedTitle);
                const imgDescription = "Description: " + decodeURIComponent(encodedDescription) + "<br />" + "Location: " + decodeURIComponent(encodedLocation) + "<br />" + "Date Taken: " + decodeURIComponent(encodedDateTaken);


                //// Update the attributes of the anchor tag containing the image
                $('#imgAnchor').attr('data-title', imgTitle);
                $('#imgAnchor').attr('data-description', imgDescription);
                $("#ImageModal").attr('src', newImgUrl);
                $("#ImageModal").parent().attr('href', newImgUrl);

                document.getElementById("photoIdxModal").value = hashPhoto;
                if (data.imageInfo.myScore != "0.00") {
                    document.getElementById("judgeScoreModal").readOnly = true;
                    document.getElementById("lockscoreBtn").innerHTML = "Unlock";
                } else {
                    document.getElementById("judgeScoreModal").readOnly = false;
                    document.getElementById("lockscoreBtn").innerHTML = "Submit";
                }

                $("#judgeScoreModal").val(parseFloat(data.imageInfo.myScore).toFixed(2));
                //document.getElementById("judgeScoreModal").value = parseFloat(data.imageInfo.myScore.toFixed(2));
                document.getElementById("imageFullModalLabel").innerText = imgTitle; // data.imageInfo.regionName + "-" + data.imageInfo.categoryName;

                refreshGLightbox();

                $('#imageFullModal').modal('show');
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
            handleAjaxError(xhr);
        }
    });

}

// Initialize GLightbox
let lightbox = GLightbox({
    selector: '.glightbox',
    loop: false,
    touchNavigation: false,
    keyboardNavigation: false
});

function refreshGLightbox() {
    lightbox.destroy();
    lightbox = GLightbox({
        selector: '.glightbox',
        loop: false,
        touchNavigation: false,
        keyboardNavigation: false,
    });
}

function handleAjaxError(xhr) {
    const errors = xhr.responseJSON && xhr.responseJSON.Errors ? xhr.responseJSON.Errors : [xhr.responseText];
    let errorMessage = '<ul>';
    errors.forEach(function (e) {
        errorMessage += '<li>' + e + '</li>';
    });
    errorMessage += '</ul>';
    $('#errorModalBody').html(errorMessage);
    $('#errorModal').modal('show');
}


$(document).ready(function () {
    $('#btnSearch').click(function () {
        const token = $('input[name="__RequestVerificationToken"]').val();
        var searchKey = $('#inpSearchKey').val();
        var xRound = $('#xRound').val();

        const postData = {
            SearchKey: searchKey,
        };

        $.ajax({
            url: "/Judge/SearchPhoto_Result",
            method: "POST",
            contentType: "application/json",
            headers: { 'RequestVerificationToken': token },
            data: JSON.stringify(postData),
            success: function (result) {

                var results_ = $('#searchResults');
                results_.empty(); // Clear previous results

                if (result.length === 0) {
                    results_.append('<div class="text-center"><br />No results found...</div>');
                } else {
                    for (var i = 0; i < result.length; i++) {
                        
                        var photo = result[i];
                        var photoHtml = `<br />
                            <div class="photo-result">
                                <img src="${photo.imageUrl}" alt="${photo.photoTitle}" onclick="openModal('${photo.hashPhotoId}', '${xRound}')" />
                                <div class="photo-details">
                                    <p>Region: ${photo.regionName}</p>
                                    <p>Category: ${photo.categoryName}</p
                                    <p>Title: ${photo.photoTitle}</p>
                                    <p>Description: ${photo.description}</p>
                                    <p>Location: ${photo.location}</p>
                                    <p>Date Taken: ${photo.photoTaken}</p>
                                    <p>My Score:  ` + parseFloat(photo.myScore).toFixed(2) + `</p> 
                                </div>
                            </div>
                        `;
                        results_.append(photoHtml);
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    });

    $("#lockscoreBtn").click(function () {
        if (document.getElementById("lockscoreBtn").innerHTML == "Submit") {
            var token = $('input[name="__RequestVerificationToken"]').val();
            var photoIdx = $("#photoIdxModal").val();
            var judgescore = $("#judgeScoreModal").val();
            var xRound = $('#xRound').val();


            var postData = {
                PhotoId: photoIdx,
                Score: judgescore,
                Round: xRound
            };


            $.ajax({
                url: "/Rank/AddUpdateScore",
                method: "POST",
                contentType: "application/json",
                headers: { 'RequestVerificationToken': token },
                data: JSON.stringify(postData), // Serialize postData to JSON
                success: function (data) {
                    console.log(data);
                    if (data.serverCode == 200) { // Accessing serverCode directly
                        $("#judgeScoreModal").val(parseFloat(judgescore).toFixed(2));
                        $("#lockscoreBtn").text("Unlock");
                        $("#judgeScoreModal").prop("readonly", true);

                        $('#imageFullModal').modal('hide');
                        $("#btnSearch").click();  //$("#btnSearch").click();


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
                    handleAjaxError(xhr);
                }
            });
        } else {
            document.getElementById("judgeScoreModal").readOnly = false;
            document.getElementById("lockscoreBtn").innerHTML = "Submit";
        }
    });


    $('#imageFullModal').on('keypress', function (e) {
        if (e.key === "Enter") {
            // Trigger click on lockscoreBtn
            event.preventDefault();
            $("#lockscoreBtn").trigger("click");
        }
    });


    $(document).keypress(function (event) {
        if (event.keyCode === 13) { // Enter key
            event.preventDefault();
            $("#btnSearch").click();  //$("#btnSearch").click();
        }
    });



    
});
