function insadd(rownum, id) {
    if (confirm("Удалить строку?")) {
        var subj_num = rownum;

        $.ajax({
            url: "/Contract/_removeInsuredRow", // '@Url.Action("_removeInsuredRow", "Contract")'
            type: "POST",
            data: { subject_id: id, indx: subj_num },
            error: function (request, status, error) {
                alert(error);
            }
        });

        $("#insdtable tbody tr:eq(" + rownum + ")").hide();//.remove();


        $("#Subjects_" + subj_num + "__num").val(-1);
    }
}


$(function () {

    //если есть ошибки покаазываем окошко
    //validation-summary-errors
    if ($(".validation-summary-errors").length>0)
    {
        $('#errModal').modal({
            keyboard: false
        });
    }

    //тримими все поля
    $("input").change(function (e) {

        var trim_val = $(this).val().toString().trim();

        $(this).val(trim_val);
    })

    //удаление застрахованного
    $(".ins-dell").click(function (e) {

        //if (confirm("Удалить строку?"))
        //{
        //    var _subject_id = $(this).attr('id').toString().replace('ins_', '');

        //    var _row = $(this).parent().parent();


        //    $.ajax({
        //        method: "POST",
        //        url: "/Contract/_removeInsuredRow",
        //        data: { subject_id: _subject_id },
        //        error: function (request, status, error) {
        //            alert(error);
        //        },
        //        success: function (data) {
        //            _row.remove();

        //            $("name^='Subjects['").each(function (e) {

        //                var _name = $(this).attr('name');
        //                alert(_name);
        //            });
        //        }
        //    });
        //}

        


        insadd($(this).parent().parent().index(), $(this).attr("id").substr(4))

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

