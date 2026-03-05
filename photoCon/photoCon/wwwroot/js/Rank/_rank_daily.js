

// Initialize GLightbox
let lightbox = GLightbox({
    selector: '.glightbox',
    loop: false,
    touchNavigation: false,
    keyboardNavigation: false
});



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
        var xDate = $("#x_date").val();
        var xRound = $("#x_round").val();

        // Validate and sanitize inputs

        // Encode user inputs before constructing the URL
        var url = "DailyRank?date=" + encodeURIComponent(xDate) + "&roundParam=" + encodeURIComponent(xRound) + "&region=" + encodeURIComponent(xReg) + "&category=" + encodeURIComponent(xCat) + "&other=" + encodeURIComponent(xOth) + "&top=" + encodeURIComponent(xTop);

        // Redirect to the sanitized and encoded URL
        window.location.href = url;
    });

    // Lock score button click event
    
});
