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


        public class CollectedPremData
        {
            public decimal? insprem { get; set; }
            public decimal? insfee { get; set; }
            public decimal? contract_count { get; set; }
        }
        public ActionResult CollectedPrem()
        {

            return View(new List<CollectedPremData>());
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

            string fname = borderofile();
            if (string.IsNullOrEmpty(fname))
                return null;

            string to = ConfigurationManager.AppSettings["MailTo"];  //"sm-valery@yandex.ru";

            string from = ConfigurationManager.AppSettings["MailFrom"]; //"titintravel@yandex.ru";

            MailMessage message = new MailMessage(from, to);

            message.Subject = "Бордеро за " + DateTime.Now.ToShortDateString();//"Using the new SMTP client.";

            message.Body = @"Бордеро за " + DateTime.Now.ToShortDateString(); ;

            SmtpClient client = new SmtpClient();

            client.UseDefaultCredentials = true;

            client.EnableSsl = true;

            
            client.Credentials = new System.Net.NetworkCredential(
                ConfigurationManager.AppSettings["MailUser"],
                ConfigurationManager.AppSettings["MailPassword"]     );//("titintravel", "17101975");
          

            Attachment a = new Attachment(fname);
            
            message.Attachments.Add(a);
     

            try
            {
                client.Host = ConfigurationManager.AppSettings["MailHost"];

                client.Send(message);

                //раз успешно, то обновим очередь
                db.BorderoUpdPrepare();
            }

            catch (Exception ex)
            {
              ViewBag.mess=  string.Format("Exception caught in SendBorderro(): {0}",  ex.ToString());
            }

            a.Dispose();
            client.Dispose();
            //удалить файл
            System.IO.File.Delete(fname);
           

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

                irow++;
            }

            s.Columns().AdjustToContents();
            s.Range(1, 1, 1, 9).Style.Font.SetBold();


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