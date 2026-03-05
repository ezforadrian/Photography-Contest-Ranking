$("#testbutton").on("click", function () {
    ViewReport();
    $("#fieldSelect").trigger("change");
});



$("#fieldSelect").on("change", function () {
    if (this.value == 1) {
        $("#fieldGrpRegion").hide();
        $("#fieldGrpRound").hide();
    }
    else if (this.value == 0) {
        $("#fieldGrpRegion").show();
        $("#fieldGrpRound").show();
    }
});

$(function () {
    $("#fieldGrpTopSelect").hide();
    $("#fieldTopSelect").val(10);
});
$("#fieldTopSelect").on("input", function () {
    if (this.value <= 0) {
        this.value = 1;
    }
});

function ViewReport() {
    var reportData = {
        RankScreening: $("#fieldSelect").val(),
        RankCount: $("fieldTopSelect").val(),
        ReportCode: $("#fieldReportType").val(),
        Region: $("#fieldRegion").val(),
        Category: $("#fieldCategory").val(),
        Round: $("#fieldRound").val(),
        EventName: "-",
        EventVenue: "-"
    };

    $.ajax({
        url: "GoToPDFView",
        type: "POST",
        contentType: "application/json",
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        data: JSON.stringify(reportData),
        success: function (data) {
            window.open(data);
        },
    });
}