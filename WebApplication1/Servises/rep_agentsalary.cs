using gTravel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClosedXML.Excel;

namespace gTravel.Servises
{

    public class rep_agentsalary
    {
        private goDbEntities _db;

        public rep_agentsalary(goDbEntities db)
        {
            this._db = db;
        }
        public void printout(RepAgentsQuery q)
        {
            var rdata = _db.agentSalary(q.d1, q.d2);

            XLWorkbook workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Зарплата агентов");

            ws.Cell(1, 1).SetValue("Отчет о зарплате агентам").Style.Font.FontSize=22;
            ws.Cell(2, 1).SetValue(string.Format("За период {0} - {1}", q.d1.Value.ToShortDateString(), q.d2.Value.ToShortDateString())).Style.Font.FontSize = 18;

            int nrow = 4;

            ws.Cell(nrow, 1).SetValue("Агент");
            ws.Cell(nrow, 2).SetValue("Продано");
            ws.Cell(nrow, 3).SetValue("Премия, у.е");
            ws.Cell(nrow, 4).SetValue("Премия, руб");
            ws.Cell(nrow, 5).SetValue("Комиссия, у.е");
            ws.Cell(nrow, 6).SetValue("Комиссия, руб");
            ws.Cell(nrow, 7).SetValue("Аннулированно");
            ws.Cell(nrow, 8).SetValue("Возврат, руб");
            ws.Cell(nrow, 9).SetValue("ИТОГО премия, у.е");
            ws.Cell(nrow, 10).SetValue("ИТОГО премия, руб");
            ws.Cell(nrow, 11).SetValue("Возврат комиссии, руб");
            ws.Cell(nrow, 12).SetValue("ИТОГО комиссия агента, у.е");
            ws.Cell(nrow, 13).SetValue("ИТОГО комиссия агента, руб");

            nrow++;
            foreach(var row in rdata)
            {
                ws.Cell(nrow, 1).SetValue(row.Name);
                ws.Cell(nrow, 2).SetValue(row.sold);
                ws.Cell(nrow, 3).SetValue(row.Prem);
                ws.Cell(nrow, 4).SetValue(row.PremRur);
                ws.Cell(nrow, 5).SetValue(row.comm);
                ws.Cell(nrow, 6).SetValue(row.commrur);
                ws.Cell(nrow, 7).SetValue(row.annul_count);
                ws.Cell(nrow, 8).SetValue(row.annul_sum_rur);
                ws.Cell(nrow, 9).SetValue(row.Prem - row.annul_sum);
                ws.Cell(nrow, 10).SetValue(row.PremRur - row.annul_sum_rur);
                ws.Cell(nrow, 11).SetValue(row.annul_comm_rur);
                ws.Cell(nrow, 12).SetValue(row.comm-row.annul_comm);
                ws.Cell(nrow, 13).SetValue(row.commrur - row.annul_comm_rur);

                    nrow++;
            }

            ws.Columns(1, 13).AdjustToContents();

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=\"salary.xlsx\"");

            workbook.SaveAs(HttpContext.Current.Response.OutputStream);

            HttpContext.Current.Response.End();
        }
    }
}