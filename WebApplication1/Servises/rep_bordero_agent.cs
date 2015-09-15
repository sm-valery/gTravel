using gTravel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClosedXML.Excel;

namespace gTravel.Servises
{
    public class rep_bordero_agent
    {
        private goDbEntities _db;

        public rep_bordero_agent(goDbEntities db)
        {
            this._db = db;
        }

        public void printout(RepAgentsQuery q)
        {
            XLWorkbook workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Бордеро");

            var agent = _db.Agents.SingleOrDefault(x => x.AgentId == q.Agents);



            int iheader = 4;
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 2).SetValue("Дата выдачи");
            ws.Cell(iheader, 3).SetValue("Аннулирован");
            ws.Cell(iheader, 4).SetValue("Застрахованные");
            ws.Cell(iheader, 6).SetValue("Франшиза");
            ws.Cell(iheader, 7).SetValue("Страна пребывания");
            ws.Cell(iheader, 8).SetValue("Период страхования");
            ws.Cell(iheader, 9).SetValue("Страховая сумма");
            ws.Cell(iheader, 10).SetValue("Страховая сумма, руб");
            ws.Cell(iheader, 11).SetValue("Страховая премия");
            ws.Cell(iheader, 12).SetValue("Страховая премия, руб");

            ws.Cell(iheader+1, 4).SetValue("Кол-во");
            ws.Cell(iheader + 1, 5).SetValue("Фамилия И.О.");



            var rdata = _db.v_bordero_agent.Where(x => x.AgentId == q.Agents).OrderBy(o => o.seriaid).ThenBy(o => o.contractnumber).ToList();

            int irow = 6;

            foreach(var row in rdata)
            {
                ws.Cell(irow, 1).SetValue(row.SeriaCode.Trim()+"-"+row.contractnumber.ToString());
                ws.Cell(irow, 2).SetValue(row.date_out);
                

                if(row.stat_code=="annul")
                {
                    ws.Cell(irow, 3).SetValue("Да");
                }
                    
                    ws.Cell(irow, 4).SetValue(rdata.Count(x=>x.ContractId==row.ContractId));
                    ws.Cell(irow, 5).SetValue(row.Name1);
                    ws.Cell(irow, 6).SetValue(row.Fransh);
                    ws.Cell(irow, 7).SetValue(row.terr_name.Trim().Substring(1,row.terr_name.Trim().Length-1) );
                    ws.Cell(irow, 8).SetValue(row.date_begin.Value.ToShortDateString()+"-"+row.date_end.Value.ToShortDateString());
                    ws.Cell(irow, 9).SetValue(row.InsSum);
                    ws.Cell(irow, 10).SetValue((row.InsPremRur/row.InsPrem) * row.InsSum);
                    ws.Cell(irow, 11).SetValue(row.InsPrem);
                    ws.Cell(irow, 12).SetValue(row.InsPremRur);

                irow++;
            }
            
            ws.Columns(1, 12).AdjustToContents();

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=\"bordero.xlsx\"");

            workbook.SaveAs(HttpContext.Current.Response.OutputStream);

            HttpContext.Current.Response.End();

        }
    }
}