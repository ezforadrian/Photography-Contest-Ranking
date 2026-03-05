$(document).ready(function () {





    // Initialize GLightbox
    let lightbox = GLightbox({
        selector: '.glightbox',
        loop: false,
        touchNavigation: false,
        keyboardNavigation: false
    });


    //-----Start application - Validation
    var GlobalrowNumber = parseInt($("#rowNumber").val());
    var GlobaltotalCount = parseInt($("#totalCount").val());
    var csrfToken = $("#csrf-token").val();

    updateButtonStatus(GlobalrowNumber);




    // AJAX functions for next and previous buttons
    function handleButtonClick(url) {
        const token = $('input[name="__RequestVerificationToken"]').val();
        const rowNumber = parseInt($("#rowNumber").val());
        const photoIdx = $("#photoIdx").val();
        const vphotoIdx = $("#VphotoIdx").val();
        const xcat = $("#xcat").val();
        const xnum = $("#xnum").val();

        const postData = {
            RowNumber: rowNumber,
            PhotoIdx: photoIdx,
            ViewPhotoIdx: vphotoIdx,
            XCat: xcat,
            XNum: xnum
        };

        $.ajax({
            url: url,
            method: "POST",
            contentType: "application/json",
            headers: { 'RequestVerificationToken': token },
            data: JSON.stringify(postData),
            success: function (data) {
                try {
                    const jsonData = JSON.parse(data);
                    if (typeof jsonData === "object" && Object.keys(jsonData).length > 0) {
                        loadDisplay(jsonData);
                    } else {
                        console.log("No data found or data is not in the expected format:", jsonData);
                    }
                } catch (error) {
                    console.error("Error parsing JSON:", error);
                }
            },
            error: function (xhr) {
                handleAjaxError(xhr);
            }
        });
    }


    $("#nextBtn").click(function () {
        handleButtonClick("/Judge/NextPhoto");
    });

    $("#prevBtn").click(function () {
        handleButtonClick("/Judge/PrevPhoto");
    });

    $("#lockscoreBtn").click(function () {
        handleLockScoreButtonClick();
    });

    $(document).keypress(function (event) {
        if (event.keyCode === 13) { // Enter key
            event.preventDefault();
            handleLockScoreButtonClick();
        }
    });

    function loadDisplay(data) {

        const judgeImageView = data.judgeImageView;
        const judgeScores = data.judgeScores;
        const existsInJudgeScores = judgeScores.some(score => score.PhotoId === judgeImageView.ImageBatch_ImageHashId);

        let judgeScoreValue = "0.00";
        let lockscoreBtnText = "Submit";
        let judgeScore_ = false;

        if (existsInJudgeScores) {
            const judgeScore = judgeScores.find(score => score.PhotoId === judgeImageView.ImageBatch_ImageHashId);
            judgeScoreValue = parseFloat(judgeScore.Score).toFixed(2);
            lockscoreBtnText = "Unlock";
            judgeScore_ = true;
        }

        const newImgUrl = judgeImageView.PhotoMetaData_ImageURL;
        const imgTitle = "Title: " + judgeImageView.PhotoTitle;
        const imgDescription = "Description: " + judgeImageView.Description + "<br />" + "Location: " + judgeImageView.Location + "<br />" + "Date Taken: " + judgeImageView.PhotoTaken;

        // Update the attributes of the anchor tag containing the image
        $('#imgAnchor').attr('data-title', imgTitle);
        $('#imgAnchor').attr('data-description', imgDescription);
        $("#imgURL").attr('src', newImgUrl);
        $("#imgURL").parent().attr('href', newImgUrl);


        // Set other field values
        $("#rowNumber").val(judgeImageView.ImageBatch_Sort);
        $("#currentCount_").html(judgeImageView.ImageBatch_Sort);
        $("#photoIdx").val(judgeImageView.ImageBatch_ImageHashId);
        $("#VphotoIdx").val(judgeImageView.PhotoMetaData_HashPhotoID);
        $("#judgeScore_").val(judgeScoreValue);
        $("#lockscoreBtn").text(lockscoreBtnText);
        $("#judgeScore_").prop("readonly", judgeScore_);
        $("#phoDimension").val(judgeImageView.PhotoMetaData_Dimension);
        $("#phoWidth").val(judgeImageView.PhotoMetaData_Width);
        $("#phoHeight").val(judgeImageView.PhotoMetaData_Height);
        $("#phoRegion").val(judgeImageView.RegionName);
        $("#phoCategory").val(judgeImageView.CategoryName);
        $("#phoHorizontalResolution").val(judgeImageView.PhotoMetaData_HorizontalResolution);
        $("#phoVerticalResolution").val(judgeImageView.PhotoMetaData_VerticalResolution);
        $("#phoBitDepth").val(judgeImageView.PhotoMetaData_BitDepth);
        $("#phoResolutionUnit").val(judgeImageView.PhotoMetaData_ResolutionUnit);
        $("#phoCameraMaker").val(judgeImageView.PhotoMetaData_CameraMaker);
        $("#phoCameraModel").val(judgeImageView.PhotoMetaData_CameraModel);
        $("#phoFStop").val(judgeImageView.PhotoMetaData_FStop);
        $("#phoExposureTime").val(judgeImageView.PhotoMetaData_ExposureTime);
        $("#phoISOSpeed").val(judgeImageView.PhotoMetaData_ISOSpeed);
        $("#phoFocalLength").val(judgeImageView.PhotoMetaData_FocalLength);
        $("#phoMaxAperture").val(judgeImageView.PhotoMetaData_MaxAperture);
        $("#phoMeteringMode").val(judgeImageView.PhotoMetaData_MeteringMode);
        $("#phoFlashMode").val(judgeImageView.PhotoMetaData_FlashMode);
        $("#phoMmFocalLength").val(judgeImageView.PhotoMetaData_MmFocalLength);
        // Update button status
        updateButtonStatus($("#rowNumber").val());



        refreshGLightbox();

        if (judgeScoreValue == "0.00") {
            $("#judgeScore_").prop("readonly", false).focus().select(); // Focus and select all text
        }


    }


    function refreshGLightbox() {
        lightbox.destroy();
        lightbox = GLightbox({
            selector: '.glightbox',
            loop: false,
            touchNavigation: false,
            keyboardNavigation: false,

        });
    }


    function updateButtonStatus(pageNumber) {
        $("#nextBtn").prop('disabled', pageNumber >= GlobaltotalCount);
        $("#prevBtn").prop('disabled', pageNumber <= 1);
    }

    function handleAjaxError(xhr) {
        console.log(xhr.responseText);
        const errors = xhr.responseJSON && xhr.responseJSON.Errors ? xhr.responseJSON.Errors : [xhr.responseText];
        let errorMessage = '<ul>';
        errors.forEach(function (e) {
            errorMessage += '<li>' + e + '</li>';
        });
        errorMessage += '</ul>';
        $('#errorModalBody').html(errorMessage);
        $('#errorModal').modal('show');
    }

    // Function to handle right arrow key press
    function handleArrowRightKeyPress(event) {
        if (event.keyCode === 39) { // Check if the pressed key is the right arrow key
            // Your code to execute when the right arrow key is pressed
            $("#nextBtn").click();
        }
    }

    // Function to handle right arrow key press
    function handleLeftRightKeyPress(event) {
        if (event.keyCode === 37) { // Check if the pressed key is the right arrow key
            // Your code to execute when the right arrow key is pressed
            $("#prevBtn").click();
        }
    }



    function handleLockScoreButtonClick() {
        const buttonVal = $("#lockscoreBtn").text();
        if (buttonVal === "Unlock") {
            $("#lockscoreBtn").text("Submit");
            $("#judgeScore_").prop("readonly", false);
            $("#judgeScore_").prop("readonly", false).focus().select(); // Focus and select all text
        } else if (buttonVal === "Submit") {
            const token = $('input[name="__RequestVerificationToken"]').val();
            const rowNumber = parseInt($("#rowNumber").val());
            const photoIdx = $("#photoIdx").val();
            const vphotoIdx = $("#VphotoIdx").val();
            const judgescore = $("#judgeScore_").val();
            const round = $("#xround").val();

            const postData = {
                RowNumber: rowNumber,
                VPhotoIdx: vphotoIdx,
                PhotoId: photoIdx,
                Score: judgescore,
                Round: round
            };

            $.ajax({
                url: "/Judge/ImageScore",
                method: "POST",
                contentType: "application/json",
                headers: { 'RequestVerificationToken': token },
                data: JSON.stringify(postData),
                success: function (data) {
                    if (data.serverCode == 200) {
                        $("#judgeScore_").val(parseFloat(judgescore).toFixed(2));
                        $("#lockscoreBtn").text("Unlock");
                        $("#judgeScore_").prop("readonly", true);
                    } else {
                        $("#lockscoreBtn").text("Lock");
                        $("#judgeScore_").prop("readonly", false);
                        $('#errorModalBody').text(data.message);
                        $('#errorModalLabel').text("Error Code: " + data.serverCode);
                        $('#errorModal').modal('show');
                    }
                },
                error: function (xhr) {
                    handleAjaxError(xhr);
                }
            });
        }
    }

    $(window).keydown(function (event) {
        if (event.keyCode === 39) { // Right arrow key
            $("#nextBtn").click();
        } else if (event.keyCode === 37) { // Left arrow key
            $("#prevBtn").click();
        }
    });

    // Focus and select text when #judgeScore_ input is focused
    $("#judgeScore_").focus(function () {
        $(this).select();
    });


});