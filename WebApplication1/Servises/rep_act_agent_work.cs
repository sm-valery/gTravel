using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using gTravel.Models;

namespace gTravel.Servises
{
    public class rep_act_agent_work
    {
        private goDbEntities _db;
        public rep_act_agent_work(goDbEntities db)
        {
            this._db = db;
        }

        public void printout(RepAgentsQuery q)
        {
            XLWorkbook workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Акт выполненных работ");

            var agent = _db.Agents.SingleOrDefault(x => x.AgentId == q.Agents);


            ws.Cell(6, 1).SetValue("Полис");
            ws.Cell(6, 2).SetValue("Страховой взнос, руб");
            ws.Cell(6, 3).SetValue("Агентское вознагр,%");
            ws.Cell(6, 4).SetValue("Агентское вознагр,руб");

            var th = ws.Range(6, 1, 6, 4);
            th.Style.Font.Bold = true;
            th.Style.Fill.BackgroundColor = XLColor.Silver;
            //ws.Cell(7, 5).SetValue("Полис");
            //ws.Cell(7, 6).SetValue("Страховой взнос, руб");
            //ws.Cell(7, 7).SetValue("Агентское вознагр,%");
            //ws.Cell(7, 8).SetValue("Агентское вознагр,руб");

            var rdata = _db.v_contract_agent.Where(x => x.AgentId == q.Agents).OrderBy(o => o.seriaid).ThenBy(o => o.contractnumber).ToList();

            int irow = 7;

            decimal sum_all = 0, sum_fee = 0;
            decimal sum_annul = 0, sum_fee_aanul = 0;


            foreach (var row in rdata)
            {
                ws.Cell(irow, 1).SetValue(row.SeriaCode.Trim() + "-" + row.contractnumber.Value.ToString());
                ws.Cell(irow, 2).SetValue(row.InsPremRur);
                ws.Cell(irow, 3).SetValue(row.Percent);
                ws.Cell(irow, 4).SetValue(row.sum_fee);

                if (row.stat_code.Trim() == "annul")
                {
                    ws.Range(irow, 1, irow, 4).Style.Font.Strikethrough = true;
                    sum_annul += row.InsPremRur.Value;
                    sum_fee_aanul += row.sum_fee.Value;
                }

                sum_all += (row.InsPremRur.HasValue)?row.InsPremRur.Value:0;
                sum_fee += (row.sum_fee.HasValue)?row.sum_fee.Value:0;

                irow++;
            }

            ws.Columns(1, 4).AdjustToContents();

            ws.Cell(2, 1).SetValue("Акт выполнения работ № ______ от \"     \" __________ 200__ г.").Style.Font.FontSize = 14;
            ws.Cell(3, 1).SetValue(string.Format("Страховой агент {0}", agent.Name)).Style.Font.FontSize = 14;


            ws.Cell(5, 1).SetValue(string.Format("За период с {0} по {1} при содействии указанного страхового агента  согласно " +
     "договору № {2} от {3}г. получены страховые премии и начислено агентское " +
     "вознаграждение по следующим полисам:",
     q.d1.Value.ToShortDateString(),
     q.d2.Value.ToShortDateString(),
     (string.IsNullOrEmpty(agent.AgentContractNum)) ? "" : agent.AgentContractNum,
     (agent.AgentContractDate.HasValue) ? agent.AgentContractDate.Value.ToShortDateString() : ""
     )).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;


            ws.Row(5).Height = 60;
            ws.Range(5, 1, 5, 4).Merge();

            ws.Cell(5, 1).Style.Alignment.WrapText = true;

            ws.Cell(irow++, 1).SetValue("Штриховкой выделены аннулированные полисы").Style.Font.FontSize = 9;
            irow++;
            ws.Cell(irow++, 1).SetValue(string.Format("Всего полисов: {0}", rdata.Count()));
            ws.Cell(irow, 1).SetValue("Всего взносов по проданным полисам:");
            ws.Cell(irow++, 4).SetValue(sum_all);
            ws.Cell(irow, 1).SetValue("Возвращено по аннулированным полисам:");
            ws.Cell(irow++, 4).SetValue(sum_annul);
            ws.Cell(irow, 1).SetValue("Итого взносов:");
            ws.Cell(irow++, 4).SetValue(sum_all - sum_annul).Style.Font.Bold = true;
            ws.Cell(irow, 1).SetValue("Агентское вознаграждение по проданным полисам:");
            ws.Cell(irow++, 4).SetValue(sum_fee);
            ws.Cell(irow, 1).SetValue("Удержано по аннулированным полисам:");
            ws.Cell(irow++, 4).SetValue(sum_fee_aanul);
            ws.Cell(irow, 1).SetValue("Сумма агентского вознаграждения:");
            ws.Cell(irow++, 4).SetValue(sum_fee - sum_fee_aanul).Style.Font.Bold = true;

            irow++;
            ws.Cell(irow++, 1).SetValue("ООО «Страховая компания «ТИТ»");
            ws.Cell(irow, 4).SetValue("Страховой агент");
            ws.Cell(irow++, 1).SetValue("Заместитель Генерального директора");
            ws.Cell(irow, 1).SetValue("____________/Болдовская Е.И./");
            ws.Cell(irow, 4).SetValue("____________/                /");




            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=\"actw.xlsx\"");

            workbook.SaveAs(HttpContext.Current.Response.OutputStream);

            HttpContext.Current.Response.End();
        }
    }
}