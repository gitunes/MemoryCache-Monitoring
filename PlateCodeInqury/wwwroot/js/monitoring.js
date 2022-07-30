var baseData //global variable

$(window).on("load", () => {
    baseData = getLogs()
    setInterval(getLogs, 3000)//Test amaçlı 3 saniyede bir istek atılmaktadır.
})

function getLogs() {
    var data
    $.ajax({
        async: false,
        type: "GET",
        url: "/Home/GetLogs",
        success: function (response) {
            data = response
        },
        error: function (response) {
            alert("Teknik hata");
        }
    });

    if (!baseData || Object.keys(baseData).length != Object.keys(data).length) {
        $("#clear").click()
        for (var index = Object.keys(data).length; index >= 1; index--) {
            $("#listHistory").append("<li>" + data[index] + "</li>")
        }
        baseData = data
    }

    return data
}

$("#clear").click(() => {
    var parentList = document.querySelector("#listHistory");
    parentList.innerHTML = ""
})