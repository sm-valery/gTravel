$(function () {



    //добавление территоиии в грид
    $("#territory_add").click(function (e) {

        var vid = $("#d_territory").val();
        var vname = $("#d_territory_chosen a span").text();

        var terr_exist = $("#terr_" + vid).length > 0;

        if (terr_exist) {

            alert("eee");
            e.preventDefault();
            return;
        }
        
        if (vid != "") {

            alert("uuu");

            $.ajax({
                url: '@Url.Action("contract_terr_insert_row")',
                data: {
                    id: vid,
                    name: vname
                },
                error: function(data)
                {
                    alert(data.complete);
                },
                success: function (data) {

                    alert("aaaaaaaa");
                    $("<tr><td>1</td><td>2</td></tr>").insertBefore("#territory_newrow");
                }
            });
        }

        e.preventDefault();
    });


    $(".btn_terr_dell").click(function (e) {

      
        $(this).parent().parent().remove();
        e.preventDefault();
    })

    $(".date_period").change(function () {

        var d1 = $("#date_begin").val();
        var d2 = $("#date_end").val();


        $.ajax({
            url: '@Url.Action("get_strperiodday","Contract")',
            data: {
                date_from: d1,
                date_to: d2
            },
            success: function (data) {
                $("#period_day_count").html(data);
            }
        });
    });

}
  )
