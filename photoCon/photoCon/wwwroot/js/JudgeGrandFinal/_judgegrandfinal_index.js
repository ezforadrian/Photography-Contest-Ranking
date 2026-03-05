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

        const postData = {
            RowNumber: rowNumber,
            PhotoIdx: photoIdx,
            ViewPhotoIdx: vphotoIdx
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
        handleButtonClick("/JudgeGrandFinal/NextPhoto");
    });

    $("#prevBtn").click(function () {
        handleButtonClick("/JudgeGrandFinal/PrevPhoto");
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
        const refCodeCriteria = data.refCodeCriteria;


        // Update image
        const newImgUrl = judgeImageView.PhotoMetaData_ImageURL;
        const imgTitle = "Title: " + judgeImageView.PhotoTitle;
        const imgDescription = "Description: " + judgeImageView.Description + "<br />" + "Location: " + judgeImageView.Location + "<br />" + "Date Taken: " + judgeImageView.PhotoTaken;

        $('#imgAnchor').attr('data-title', imgTitle);
        $('#imgAnchor').attr('data-description', imgDescription);
        $("#imgURL").attr('src', newImgUrl);
        $("#imgURL").parent().attr('href', newImgUrl);

        // Update metadata
        $("#rowNumber").val(judgeImageView.ImageBatch_Sort);
        $("#currentCount_").html(judgeImageView.ImageBatch_Sort);
        $("#photoIdx").val(judgeImageView.ImageBatch_ImageHashId);
        $("#VphotoIdx").val(judgeImageView.PhotoMetaData_HashPhotoID);
        $("#phoDimension").val(judgeImageView.PhotoMetaData_Dimension);
        $("#phoWidth").val(judgeImageView.PhotoMetaData_Width);
        $("#phoHeight").val(judgeImageView.PhotoMetaData_Height);
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

        // Update judge scores
        const existsInJudgeScores = judgeScores.some(score => score.PhotoId === judgeImageView.ImageBatch_ImageHashId);
        if (existsInJudgeScores) {
            // Update the scores based on the found judge score object
            refCodeCriteria.forEach(criteria => {
                const photoIdToFind = judgeImageView.ImageBatch_ImageHashId; // Declare photoIdToFind here
                const score = judgeScores.find(score => score.Criteria.trim() === criteria.CriteriaCode.trim() && score.PhotoId.trim() === photoIdToFind.trim());
                if (score) {
                    $(`#judgeScore_${criteria.CriteriaCode}`).val(score.Score).prop("readonly", true);
                } else {
                    // Set score to 0 if not found
                    $(`#judgeScore_${criteria.CriteriaCode}`).val(0).prop("readonly", true);
                }
            });
           
            // Calculate average score
            const averageScore = judgeScores.reduce((sum, score) => sum + score.Score, 0);
            
            $("#average").val(averageScore.toFixed(2));
            $("#lockscoreBtn").text("Unlock");
        } else {
            // No judge scores found for the given PhotoId, reset the scores
            refCodeCriteria.forEach(criteria => {
                // Since no score is found, set the input field value to 0
                $(`#judgeScore_${criteria.CriteriaCode}`).val(0).prop("readonly", false);
            });
            $("#average").val("0.00");
            $("#lockscoreBtn").text("Submit");
        }


        // Update button status
        updateButtonStatus($("#rowNumber").val());

        // Refresh GLightbox
        refreshGLightbox();
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
        const buttonVal = $("#lockscoreBtn").text().trim();
        if (buttonVal === "Unlock") {
            $("#lockscoreBtn").text("Submit");
            $("input[id^='judgeScore_']").prop("readonly", false);
        } else if (buttonVal === "Submit") {
            const token = $('input[name="__RequestVerificationToken"]').val();
            const rowNumber = parseInt($("#rowNumber").val());
            const photoIdx = $("#photoIdx").val();
            const vphotoIdx = $("#VphotoIdx").val();
            const xRound = $("#xRound").val();

            // Collect scores and criteria as an array of objects and validate them
            let scoreData = [];
            let isValid = true;

            $("input[id^='judgeScore_']").each(function () {
                const criteriaCode = $(this).attr('id').split('_')[1];
                const scoreValue = parseFloat($(this).val());

                if (isNaN(scoreValue) || scoreValue < 1 || scoreValue > 10) {
                    isValid = false;
                    $(this).addClass('is-invalid'); // Add Bootstrap invalid class for error styling
                } else {
                    $(this).removeClass('is-invalid'); // Remove invalid class if valid
                    scoreData.push({ Criteria: criteriaCode, Score: scoreValue });
                }
            });

            if (!isValid) {
                // Display modal with validation error
                $('#errorModalBody').text("Please enter valid scores for all categories. Scores must be between 1 and 10.");
                $('#errorModalLabel').text("Validation Error");
                $('#errorModal').modal('show');
                return;
            }


            const postData = {
                RowNumber: rowNumber,
                VPhotoIdx: vphotoIdx,
                PhotoId: photoIdx,
                ScoreData: scoreData, // Updated to send score data as an array of objects
                Round: xRound
            };

            $.ajax({
                url: "/JudgeGrandFinal/ImageScore",
                method: "POST",
                contentType: "application/json",
                headers: { 'RequestVerificationToken': token },
                data: JSON.stringify(postData),
                success: function (data) {
                    if (data.serverCode == 200) {
                        $("input[id^='judgeScore_']").each(function () {
                            $(this).val(parseFloat($(this).val()).toFixed(2));
                            $(this).prop("readonly", true);
                        });
                        $("#average").val(data.averageScore.toFixed(2));
                        $("#lockscoreBtn").text("Unlock");
                    } else {
                        $("#lockscoreBtn").text("Lock");
                        $("input[id^='judgeScore_']").prop("readonly", false);
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


});