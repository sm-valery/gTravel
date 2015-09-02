using gTravel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Net.Mail;
using System.Configuration;
using ClosedXML.Excel;
using System.IO;
using System.Net.Mime;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations;

namespace gTravel.Controllers
{

    [Authorize]
    public class ReportController : Controller
    {
        private goDbEntities db = new goDbEntities();


        // GET: Report
        public ActionResult Index()
        {

            return View();
        }


        public class RepAgentsQuery
        {
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:dd'.'MM'.'yyyy}", ApplyFormatInEditMode = true)]
            [Display(Name = "Дата с")]
            public DateTime? d1 { get; set; }

            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:dd'.'MM'.'yyyy}", ApplyFormatInEditMode = true)]
            [Display(Name = "Дата по")]
            public DateTime? d2 { get; set; }

            public Guid Agents { get; set; }
        }

        public ActionResult RepAgents()
        {

            ViewBag.Agents = new SelectList(db.Agents.OrderBy(o=>o.Name), "AgentId", "Name");

            DateTime d1 = DateTime.Parse(string.Format("01.{0}.{1}",DateTime.Now.Month,DateTime.Now.Year));

            return View(new RepAgentsQuery()
            {
                d1 = d1,
                d2 = d1.AddMonths(1).AddDays(-1)
            });
        }

        public void RepAgentsQue(RepAgentsQuery q)
        {

            XLWorkbook workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Акт выполненных работ");

            var agent = db.Agents.SingleOrDefault(x => x.AgentId == q.Agents);


            ws.Cell(6, 1).SetValue("Полис");
            ws.Cell(6, 2).SetValue("Страховой взнос, руб");
            ws.Cell(6, 3).SetValue("Агентское вознагр,%");
            ws.Cell(6, 4).SetValue("Агентское вознагр,руб");

            //ws.Cell(7, 5).SetValue("Полис");
            //ws.Cell(7, 6).SetValue("Страховой взнос, руб");
            //ws.Cell(7, 7).SetValue("Агентское вознагр,%");
            //ws.Cell(7, 8).SetValue("Агентское вознагр,руб");

            var rdata = db.v_contract_agent.Where(x => x.AgentId == q.Agents).OrderBy(o=>o.seriaid).ThenBy(o=>o.contractnumber).ToList();

            int irow=7;

            foreach(var row in rdata)
            {
                ws.Cell(irow,1).SetValue(row.SeriaCode.Trim() + "-" + row.contractnumber.Value.ToString());
                ws.Cell(irow, 2).SetValue(row.InsPremRur);
                ws.Cell(irow, 3).SetValue(row.Percent);
                ws.Cell(irow, 4).SetValue(row.sum_share);

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
     (agent.AgentContractDate.HasValue)?agent.AgentContractDate.Value.ToShortDateString():""
     )).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;


            ws.Row(5).Height = 60;
            ws.Range(5, 1, 5, 4).Merge();

            ws.Cell(5, 1).Style.Alignment.WrapText = true;

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=\"actw.xlsx\"");

            workbook.SaveAs(Response.OutputStream);

            Response.End();

        }




        public class CollectedPremData
        {
            public decimal? insprem { get; set; }
            public decimal? insfee { get; set; }
            public decimal? contract_count { get; set; }
        }
        public ActionResult CollectedPrem()
        {
            string userid;

            if (User.IsInRole("Admin"))
                userid = "7c12356a-3dfd-4504-8d08-3f6c76ea7069";
            else
                userid = User.Identity.GetUserId();

            return View(db.spMonthPrem(userid).ToList());
        }

        public void generateact(decimal month, decimal year)
        {
            DateTime d1 = DateTime.Parse("01." + month + "." + year);
            DateTime d2 = DateTime.Parse(DateTime.DaysInMonth((int)year, (int)month).ToString() + "." + month + "." + year);

            XLWorkbook w = new XLWorkbook();

            IXLWorksheet s = w.Worksheets.Add("Акт");
            s.Cell(1, 1).SetValue("Акт выполнения работ № от").Style.Font.FontSize = 16;
            s.Cell(2, 1).SetValue("Страховой агент:	«ПРО Лоялти», ООО").Style.Font.FontSize = 16;

            s.Cell(4, 1).SetValue(string.Format("За период с {0} по {1} при содействии указанного страхового агента согласно договору № 13/АГ/140  от 08 июля 2013 г. получены страховые премии и начислено агентское вознаграждение по следующим полисам:",
                d1.ToShortDateString(),
                d2.ToShortDateString()
                ));



            s.Cell(6, 1).SetValue("Полис");
            s.Cell(6, 2).SetValue("Страховой взнос");
            s.Cell(6, 3).SetValue("Агентское вознаграждение, %");
            s.Cell(6, 4).SetValue("Агентское вознаграждение, руб");

            string userid;

            if (User.IsInRole("Admin"))
                userid = "7c12356a-3dfd-4504-8d08-3f6c76ea7069";
            else
                userid = User.Identity.GetUserId();

            var acts = db.v_contract.Where(x => x.date_out >= d1
                && x.date_out <= d2
                && x.status_code == "confirmed"
                && x.UserId == userid).OrderBy(o => o.contractnumber).ToList();

            decimal commission = 40;
            decimal commrur = 0;
            decimal premall = 0;
            decimal commall = 0;

            int irow = 7;
            foreach (var row in acts)
            {
                commrur = row.InsPrem.Value * commission / 100;
                premall += row.InsPrem.Value;
                commall += commrur;

                s.Cell(irow, 1).SetValue(row.serianame.Trim() + row.contractnumberformat);
                s.Cell(irow, 2).SetValue(row.InsPrem);
                s.Cell(irow, 3).SetValue(commission);
                s.Cell(irow, 4).SetValue(commrur);


                irow++;
            }

            s.Range(6, 1, irow - 1, 4).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            s.Range(6, 1, irow - 1, 4).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            s.Range(6, 1, irow - 1, 4).Style.Border.RightBorder = XLBorderStyleValues.Thin;
            s.Range(6, 1, irow - 1, 4).Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            s.Range(6, 1, 6, 4).Style.Font.Bold = true;
            s.Range(6, 1, 6, 4).Style.Fill.BackgroundColor = XLColor.Silver;

            s.Cell(irow, 1).SetValue(string.Format("Всего: {0}", acts.Count()));
            s.Cell(irow + 2, 1).SetValue(string.Format("Всего взносов по проданным полисам: {0:N0}", premall));
            s.Cell(irow + 3, 1).SetValue(string.Format("Агентское вознаграждение по проданным полисам: {0:N}", commall));

            s.Cell(irow + 5, 1).SetValue("Страховая компания «ТИТ»");
            s.Cell(irow + 6, 1).SetValue("Генеральный директор");
            s.Cell(irow + 7, 2).SetValue("/И.Э. Чаус/");
            s.Cell(irow + 9, 1).SetValue("М.П.");

            s.Cell(irow + 5, 3).SetValue("Страховой агент:");
            s.Cell(irow + 6, 3).SetValue("Генеральный директор");
            s.Cell(irow + 7, 4).SetValue("/Корчинский Д.Н./");
            s.Cell(irow + 9, 3).SetValue("М.П.");

            s.Column(1).Width = 20;
            s.Column(2).Width = 20;
            s.Column(3).Width = 20;
            s.Column(4).Width = 20;

            s.Row(4).Height = 46;
            s.Row(6).Height = 30;

            s.Range("A4:D4").Merge().Style.Alignment.WrapText = true;
            s.Range("C6").Merge().Style.Alignment.WrapText = true;
            s.Range("D6").Merge().Style.Alignment.WrapText = true;

            s.Range("A6:D6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            s.Cell(21, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            s.Cell(21, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=\"act.xlsx\"");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                w.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
               
            }

            Response.End();
        }

        public void geteratebordero(decimal month, decimal year)
        {
            DateTime d1 = DateTime.Parse("01." + month + "." + year);
            DateTime d2 = DateTime.Parse(DateTime.DaysInMonth((int)year, (int)month).ToString() + "." + month + "." + year);


            XLWorkbook w = new XLWorkbook();

            IXLWorksheet s = w.Worksheets.Add("Бордеро полное");

            s.Cell(1, 1).SetValue("За период");
            s.Cell(1, 2).SetValue("Номер полиса");
            s.Cell(1, 3).SetValue("ПодСерия");
            s.Cell(1, 4).SetValue("Дата выдачи");
            s.Cell(1, 5).SetValue("Дата аннулирования");
            s.Cell(1, 6).SetValue("Кол-во");
            s.Cell(1, 7).SetValue("ФИО застрахованного");
            s.Cell(1, 8).SetValue("Дата рождения");
            s.Cell(1, 9).SetValue("Франшиза вал");
            s.Cell(1, 10).SetValue("Территория");
            s.Cell(1, 11).SetValue("Период страхования");
            s.Cell(1, 12).SetValue("Страховая сумма");
            s.Cell(1, 13).SetValue("Валюта");
            s.Cell(1, 14).SetValue("Страховая сумма руб");
            s.Cell(1, 15).SetValue("Страховая премия");
            s.Cell(1, 16).SetValue("Страховая преия руб");
            s.Cell(1, 17).SetValue("Агент1");
            s.Cell(1, 18).SetValue("Агент2");
            s.Cell(1, 19).SetValue("Комиссия агента1, %");
            s.Cell(1, 20).SetValue("Комиссия агента2, %");

            string userid;

            if (User.IsInRole("Admin"))
                userid = "7c12356a-3dfd-4504-8d08-3f6c76ea7069";
            else
                userid = User.Identity.GetUserId();

            var bordero = db.v_contract.Where(x => x.date_out >= d1
                && x.date_out <= d2
                && x.status_code == "confirmed"
                && x.UserId == userid).OrderBy(o => o.contractnumber).ToList();

            int irow = 2;
            decimal commrur = 0;
            decimal commission = 40;

            foreach (var row in bordero)
            {
                var subj = db.Subjects.SingleOrDefault(x => x.SubjectId == row.Holder_SubjectId);

                var crisk = db.ContractRisks.SingleOrDefault(x=>x.ContractId==row.ContractId);

                commrur = row.InsPrem.Value * commission / 100;

                s.Cell(irow, 1).SetValue(d1.ToShortDateString()+"-"+d2.ToShortDateString());
                s.Cell(irow, 2).SetValue(row.serianame.Trim() + row.contractnumberformat);
                //s.Cell(irow, 3).SetValue();
                s.Cell(irow, 4).SetValue<DateTime>(row.date_out.Value);
                //s.Cell(irow, 5).SetValue();
                s.Cell(irow, 6).SetValue(1);
                s.Cell(irow, 7).SetValue(row.holder_name);

                if (subj.DateOfBirth.HasValue)
                s.Cell(irow, 8).SetValue<DateTime>(subj.DateOfBirth.Value);

                //s.Cell(irow, 9).SetValue();
                s.Cell(irow, 10).SetValue("Россия");
                s.Cell(irow, 11).SetValue(row.date_begin.Value.ToShortDateString() + "-"+row.date_end.Value.ToShortDateString());
                s.Cell(irow, 12).SetValue(crisk.InsSum.Value);
                s.Cell(irow, 13).SetValue(row.curr_code);
                s.Cell(irow, 14).SetValue(crisk.InsSum.Value);
                s.Cell(irow, 15).SetValue(row.InsPrem.Value);
                s.Cell(irow, 16).SetValue(row.InsPremRur.Value);
                s.Cell(irow, 17).SetValue("Про Лоялти");
                //s.Cell(irow, 18).SetValue();
                s.Cell(irow, 19).SetValue(commission);
                s.Cell(irow, 20).SetValue(commrur);


                irow++;
            }

            s.Columns().AdjustToContents();

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=\"act.xlsx\"");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                w.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
               
            }

            Response.End();
        }

        public void xlsActPrem(decimal month, decimal year, string type ="1")
        {
            if(type=="1")
            generateact(month, year);

            if(type=="2")
            {
                geteratebordero(month, year);
            }
        }

        [HttpPost]
        public ActionResult CollectedPrem(DateTime? dtFrom, DateTime? dtTo)
        {

            var viewrep = from r in db.v_contract
                          where r.date_begin >= dtFrom && r.date_out <= dtTo
                          group r by r.seriaid into g
                          select new CollectedPremData { insprem = g.Sum(a => a.InsPrem),
                          insfee = g.Sum(a=>a.InsFee),
                          contract_count = g.Count()
                          };

            ViewBag.dtFrom = dtFrom;
            ViewBag.dtFrom = dtTo;


            return View(viewrep.ToList());
        }


        public ActionResult Bordero(int? page)
        {
            var pageNumber = page ?? 1;


            return View(db.v_bordero.OrderByDescending(o=>o.DateInsert).ToPagedList(pageNumber, 25));
        }

        [AllowAnonymous]
        public ActionResult borderocreate(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            


            if(mLib.gethash(key)=="73bcaaa458bff0d27989ed331b68b64d")
            {
                db.BorderoCreate();
            }

            
            return View();
        }
        

        [AllowAnonymous]
        public ActionResult borderosend(string key)
        {

            if (mLib.gethash(key) != "73bcaaa458bff0d27989ed331b68b64d")
            {
                return null;
            }


            ViewBag.mess = "sended";

        

            string to = ConfigurationManager.AppSettings["MailTo"];  //"sm-valery@yandex.ru";

            string from = ConfigurationManager.AppSettings["MailFrom"]; //"titintravel@yandex.ru";

            MailMessage message = new MailMessage(from, to);

            message.Subject = "Бордеро за " + DateTime.Now.ToShortDateString();//"Using the new SMTP client.";

            message.Body = @"Бордеро за " + DateTime.Now.ToShortDateString(); 

            SmtpClient client = new SmtpClient();

            client.UseDefaultCredentials = true;

            client.EnableSsl = true;

            
            client.Credentials = new System.Net.NetworkCredential(
                ConfigurationManager.AppSettings["MailUser"],
                ConfigurationManager.AppSettings["MailPassword"]     );//("titintravel", "17101975");

            string fname = borderofile();

            if (!string.IsNullOrEmpty(fname))
            {
                Attachment a = new Attachment(fname);
                a.Name = string.Format("bordero{0}.xlsx", DateTime.Now.ToShortDateString());
                message.Attachments.Add(a);
            }
            else
            {
                message.Subject = "Бордеро за " + DateTime.Now.ToShortDateString() + ". Договоров нет"; 
                message.Body = @"Нет договоров для бордеро за " + DateTime.Now.ToShortDateString(); 
            }
              
            try
            {
                client.Host = ConfigurationManager.AppSettings["MailHost"];

                client.Send(message);

                //раз успешно, то обновим очередь
                if (!string.IsNullOrEmpty(fname))
                    db.BorderoUpdPrepare();
            }

            catch (Exception ex)
            {
              ViewBag.mess=  string.Format("Exception caught in SendBorderro(): {0}",  ex.ToString());
            }

            message.Dispose();
            client.Dispose();
            //удалить файл
            if(!string.IsNullOrEmpty(fname))
            {
                try
                {
                    System.IO.File.Delete(fname);
                }
                catch (Exception e)
                {
                    ViewBag.mess = "Ошибка удаления файла." + e.Message;
                }

            }
            
            return View();
        }


        
        private string borderofile()
        {
            string fname = "";

            XLWorkbook w = new XLWorkbook();
            IXLWorksheet s = w.Worksheets.Add("Бордеро");

            var bordero = db.v_bordero_new.OrderBy(x=>x.BorderoId).ToList();

            if (bordero.Count()==0)
                return "";

            //пометим как в обработке
            var borderoupd = bordero.Select(x => x.BorderoId).Distinct();
            foreach(var i in borderoupd)
            {
                var bh = db.Borderoes.SingleOrDefault(x => x.BorderoId == i);
                bh.Sended = 2;//в обработке

                db.Entry(bh).State = EntityState.Modified;
            }
            db.SaveChanges();


            s.Cell(1, 1).SetValue("Номер бордеро");
            s.Cell(1, 2).SetValue("Дата бордеро");
            s.Cell(1, 3).SetValue("Номер договора");
            s.Cell(1, 4).SetValue("Дата выдачи");
            s.Cell(1, 5).SetValue("Дата начала");
            s.Cell(1, 6).SetValue("Дата окончания");
            s.Cell(1, 7).SetValue("Валюта");
            s.Cell(1, 8).SetValue("Территория");
            s.Cell(1, 9).SetValue("Премия, руб");
            s.Cell(1, 10).SetValue("Агент");
            s.Cell(1, 11).SetValue("Оператор");

            int irow = 2;
            foreach(var b in bordero)
            {
                s.Cell(irow,1).SetValue(b.DocNum);
                s.Cell(irow,2).SetValue(b.DateInsert);
                s.Cell(irow,3).SetValue(b.contractnumber);
                s.Cell(irow, 4).SetValue<DateTime?>(b.date_out);
                s.Cell(irow, 5).SetValue<DateTime?>(b.date_begin);
                s.Cell(irow, 6).SetValue<DateTime?>(b.date_end);
                s.Cell(irow, 7).SetValue(b.currcode);
                s.Cell(irow, 8).SetValue(b.Name);
                s.Cell(irow, 9).SetValue(b.InsPremRur);
                s.Cell(irow, 10).SetValue(b.agentname);
                s.Cell(irow, 11).SetValue(b.UserName);

                irow++;
            }

            s.Columns().AdjustToContents();
            s.Range(1, 1, 1, 11).Style.Font.SetBold();


            fname = string.Format(Path.GetTempPath() +@"{0}.xlsx", Guid.NewGuid());
            w.SaveAs(fname);

            w.Dispose();

            return fname;
        }

        //private MemoryStream generateborderoxls()
        //{

        //    XLWorkbook w = new XLWorkbook();
        //    IXLWorksheet s = w.Worksheets.Add("Бордеро");

        //    s.Cell(1,1).SetValue("тестовый файл");

        //    MemoryStream memoryStream = new MemoryStream();
        //    w.SaveAs(memoryStream);


        //    return memoryStream;
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}