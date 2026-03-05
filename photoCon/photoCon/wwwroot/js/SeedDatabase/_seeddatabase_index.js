$(document).ready(function () {

    var csrfToken = $("#csrf-token").val();


    $("#btnSeedData").click(function () {
        $.ajax({
            url: "/SeedDatabase/SeedData",
            method: "POST",
            contentType: "application/json",
            headers: {
                "X-CSRF-TOKEN": $("#csrf-token").val() // Include CSRF token in the request headers
            },
            success: function (data) {
                // Parse the JSON data
                var jsonData = JSON.parse(data);

                // Create an empty string to hold the HTML content
                var htmlContent = '';

                // Iterate over the metadata objects and construct HTML elements
                $.each(jsonData, function (index, metadata) {
                    // Construct HTML for each metadata item
                    htmlContent += '<div>';
                    htmlContent += '<h3>' + metadata.FileName + '</h3>';
                    htmlContent += '<p>Image URL: ' + metadata.ImageURL + '</p>';
                    htmlContent += '<p>Dimension: ' + metadata.Dimension + '</p>';
                    htmlContent += '<p>Dimension: ' + metadata.ImageDirectory + '</p>';
                    htmlContent += '<p>Dimension: ' + metadata.ImageNumberHash + '</p>';
                    // Add more properties as needed
                    htmlContent += '</div>';
                });

                // Append the HTML content to the metadata container
                $("#metadataContainer").html(htmlContent);

                alert("Data seeding successful!");
            },
            error: function (xhr, status, error) {
                console.log(xhr.responseText);
                alert("Data seeding failed: " + error);
            }
        });
    });



    $(".seedButton").click(function () {

        var regionIdHash = $(this).data("region-id");
        var categoryIdHash = $(this).data("category-id");


        $(this).prop('disabled', true);
        $(this).html(`<span class="spinner-grow spinner-grow-sm"></span>
                          <span role="status">Loading</span>`);



        var trId = "r";
        var catnameId = "c";
        var dbCountId = "d";
        var folCount = "f";
        var lastUp = "h";
        var act = "a";



        trId = trId + sanitizedStr(regionIdHash);
        catnameId = catnameId + sanitizedStr(categoryIdHash);
        dbCountId = dbCountId + sanitizedStr(categoryIdHash);
        folCount = folCount + sanitizedStr(categoryIdHash);
        lastUp = lastUp + sanitizedStr(categoryIdHash);
        act = act + sanitizedStr(categoryIdHash);



        var postData = {
            RegionHash: regionIdHash,
            CategoryHash: categoryIdHash
        };




        $.ajax({
            url: "/SeedDatabase/SeedDataPerRegionAndCategory",
            method: "POST",
            contentType: "application/json",
            headers: { "X-CSRF-TOKEN": csrfToken },
            data: JSON.stringify(postData), // Serialize postData to JSON
            success: function (data) {
                try {

 
                    var jsonData = JSON.parse(data);

                    if (jsonData.ServerResponseCode == 200) {
                        //success

                        $("#" + dbCountId).html(jsonData.DBCountTo);
                        $("#" + folCount).html(jsonData.FolderCount);
                        $("#" + lastUp).html(jsonData.ServerResponseMessage);



                        //with condition
                        if (jsonData.DBCountTo == jsonData.FolderCount) {
                            $("#" + act).html("Completed");
                        }

                    }
                    else {
                        //something went wrong
                        $(this).prop('disabled', false);
                        $(this).html(`Seed DB`);
                        alert("Error: Contact System Admin");
                        console.log(jsonData.ServerResponseMessage);
                    }




                } catch (error) {
                    console.error("Error parsing JSON:", error);
                }
            },
            error: function (xhr, status, error) {
                console.log(xhr.responseText);
            }
        });
    });


    function sanitizedStr(str) {
        return str.replace(/[^a-z0-9\s]/gi, '').replace(/[_\s]/g, '-');
    }



});

