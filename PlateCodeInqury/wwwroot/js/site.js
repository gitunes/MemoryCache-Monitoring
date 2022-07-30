$("#getValueBtn").click(() => {
    if (checkValidity($("#plateCode").val())) {
        const data = {
            plateCode: $("#plateCode").val()
        }
        $.ajax({
            type: "POST",
            url: "/Home/GetPlateValue",
            data: data,
            success: function (response) {
                $("#listHistory").prepend("<li>" + response + "</li>", $("#listHistory"))
            },
            error: function (response) {
                alert("Teknik Hata");
            }
        });
    }
    else {
        alert("1 ile 81 arasında bir sayı giriniz.")
    }
    
})

$("#clear").click(() => {
    $("#plateCode").val('')
    var parentList = document.querySelector("#listHistory");
    parentList.innerHTML = ""
})

function checkValidity(x) {
    if (x < 1 || x > 81)
        return false
    return true
}