$(function () {

    //изменение периода
    $(".date_period").change(function () {

        var d1 = $("#date_begin").val();
        var d2 = $("#date_end").val();

        $.ajax({
            method: "GET",
            url: "/Contract/get_strperiodday",
            data: {
                date_from: d1,
                date_to: d2
            },
            error: function (request, status, error) {
                alert(error);
            },
            success: function (data) {
                $("#period_day_count").html(data);
            }
        });
    });

})

