// Define the openModalFS function globally
function openModalFS(param2, param3) {
    var token = $('input[name="__RequestVerificationToken"]').val();
    var hashPhoto = param2;
    var roundHash = param3;

    var postData = {
        HashId: hashPhoto,
        HashRound: roundHash
    };

    

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
                document.getElementById("imageFullModalLabel").innerText = data.imageInfo.regionName + "-" + data.imageInfo.categoryName;
                document.getElementById("xRound").value = roundHash;

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
    // Lazy loading images
    var lazyloadImages = document.querySelectorAll(".lazy-load");
    var lazyloadThrottleTimeout;

    function lazyload() {
        if (lazyloadThrottleTimeout) {
            clearTimeout(lazyloadThrottleTimeout);
        }

        lazyloadThrottleTimeout = setTimeout(function () {
            var scrollTop = window.pageYOffset;
            lazyloadImages.forEach(function (img) {
                if (img.offsetTop < (window.innerHeight + scrollTop)) {
                    img.src = img.dataset.src; // Replace src with data-src
                    img.onload = function () {
                        img.classList.remove('placeholder-image'); // Remove the placeholder class
                        var spinner = img.parentElement.querySelector('.spinner');
                        if (spinner) {
                            spinner.style.display = 'none'; // Hide the spinner once image is loaded
                        }
                    }
                }
            });
            if (lazyloadImages.length == 0) {
                document.removeEventListener("scroll", lazyload);
                window.removeEventListener("resize", lazyload);
                window.removeEventListener("orientationChange", lazyload);
            }
        }, 20);
    }

    document.addEventListener("scroll", lazyload);
    window.addEventListener("resize", lazyload);
    window.addEventListener("orientationChange", lazyload);

    // Initially call lazyload function
    lazyload();

    // Dropdown change event
    $("select").change(function () {
        var xReg = $("#x_reg").val();
        var xCat = $("#x_cat").val();
        var xOth = $("#x_oth").val();
        var xTop = $("#x_top").val();

        // Validate and sanitize inputs

        // Encode user inputs before constructing the URL
        var url = "MyRank?region=" + encodeURIComponent(xReg) + "&category=" + encodeURIComponent(xCat) + "&other=" + encodeURIComponent(xOth) + "&top=" + encodeURIComponent(xTop);

        // Redirect to the sanitized and encoded URL
        window.location.href = url;
    });

    // Lock score button click event
    $("#lockscoreBtn").click(function () {
        if (document.getElementById("lockscoreBtn").innerHTML == "Submit") {
            var token = $('input[name="__RequestVerificationToken"]').val();
            var photoIdx = $("#photoIdxModal").val();
            var judgescore = $("#judgeScoreModal").val();
            var xRound = $("#xRound").val();
            var identifier = '#' + photoIdx;
            var dividentifier = '#div_' + photoIdx;


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
                    if (data.serverCode == 200) { // Accessing serverCode directly
                        $("#judgeScoreModal").val(parseFloat(judgescore).toFixed(2));
                        $("#lockscoreBtn").text("Unlock");
                        $("#judgeScoreModal").prop("readonly", true);
                       
                        $(dividentifier).css("border-color", "black");
                        $(identifier).css("color", "black");
                        $(identifier).html(parseFloat(judgescore).toFixed(2));
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
});
