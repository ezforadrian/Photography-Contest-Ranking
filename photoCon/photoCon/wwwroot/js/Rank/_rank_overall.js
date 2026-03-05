
document.addEventListener("DOMContentLoaded", function () {
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
});






$(document).ready(function () {
    $("select").change(function () {
        var xReg = $("#x_reg").val();
        var xCat = $("#x_cat").val();
        var xTop = $("#x_top").val();

        // Validate and sanitize inputs


        // Encode user inputs before constructing the URL
        var url = "OverallRanking?region=" + encodeURIComponent(xReg) + "&category=" + encodeURIComponent(xCat) + "&top=" + encodeURIComponent(xTop);

        // Redirect to the sanitized and encoded URL
        window.location.href = url;
    });


});









