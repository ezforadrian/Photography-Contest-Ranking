function openModalFS(param1, param2, param3, param4, param5, param6) {
    if (param3 != "0.00") {
        document.getElementById("imgRankMModal").innerText = param4;
        document.getElementById("imgScoreModal").innerText = param3;
    }

    document.getElementById("imageFullModalLabel").innerText = param5 + "-" + param6;
    document.getElementById("ImageModal").src = param1;
    $('#imageFullModal').modal('show');
}