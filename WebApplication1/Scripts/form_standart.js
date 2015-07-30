function update_assuredlabelcount() {

    var acount = 0;

    $("#insdtable>tbody>tr").each(function (index) {

        if ($(this).css("display") != "none")
            acount++;

    });



    $("#label_assured_count").html(acount);

}

function ins_delete(rownum, id) {
    if (confirm("Удалить строку?")) {
        var subj_num = rownum;
        
        $.ajax({
            url: "/Contract/_removeInsuredRow", // '@Url.Action("_removeInsuredRow", "Contract")'
            type: "POST",
            data: { subject_id: id, indx: subj_num },
            success: function (data) {

                $("#insdtable tr:eq(" + (rownum+1).toString() + ")").hide();//.remove();

                $("#Subjects_" + subj_num + "__num").val(-1);

                update_assuredlabelcount();

            },
            error: function (request, status, error) {
                alert(error);
            }
        });

    }
}

function agent_delete(id, agentnum)
{
    if (confirm("Удалить строку?")) {

      
        $.ajax({
            url: "/Contract/_removeAgentRow",
            type: "POST",
            data: { contractagentid: id },
            success: function (data) {
                $("[data-id=" + id + "]").parent().parent().hide();

                $("#ContractAgents_" + agentnum + "__num").val(-1);

            },
            error: function (request, status, error) {
                alert(error);
            }
        });
    }
}


$(function () {


    $(".chosen-select").chosen({ no_results_text: "не найдено: " });

    //если есть ошибки покаазываем окошко
    //validation-summary-errors
    if ($(".validation-summary-errors").length > 0) {
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

   
        ins_delete($(this).parent().parent().index(), $(this).attr("id").substr(4))

        e.preventDefault();
    });


    //удаление агента
    $(".agent-dell").click(function (e) {

        var id = $(this).attr("data-id");
        var agentnum = $(this).attr("data-num").toString();

        agent_delete(id, agentnum);

        e.stopImmediatePropagation();

        return false;
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

