function insadd(rownum, id) {
    if (confirm("Удалить строку?")) {
        var subj_num = rownum - 1;

        $.ajax({
            url: "/Contract/_removeInsuredRow", // '@Url.Action("_removeInsuredRow", "Contract")'
            type: "GET",
            data: { subject_id: id, indx: subj_num },
            error: function (request, status, error) {
                alert(error);
            }
        });

        $("#insdtable tr:eq(" + rownum + ")").hide();//.remove();


        $("#Subjects_" + subj_num + "__num").val(-1);
    }
}


$(function () {



    //удаление застрахованного
    $(".ins-dell").click(function (e) {

        insadd($(this).parent().parent().index(), $(this).attr("id").substr(3))

        e.preventDefault();
    });


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

