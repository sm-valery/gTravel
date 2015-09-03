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

            int iheader = 4;
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 1).SetValue("Полис");
            ws.Cell(iheader, 1).SetValue("Полис");
        }
    }
}