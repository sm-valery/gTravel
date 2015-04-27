using System;
using System.Linq;
using System.Web.Mvc;
using gTravel.Models;
using System.Data.Entity;
using System.Net;
using System.IO;
using Microsoft.AspNet.Identity;
using System.Web;
using System.Data.OleDb;
using System.Data;
using ClosedXML.Excel;
using PagedList;
using System.Collections.Generic;


namespace gTravel.Controllers
{
    [Authorize]
    public class ContractController : Controller
    {
        private goDbEntities db = new goDbEntities();
        //путешествие по россии
        private Guid MainSeria = Guid.Parse("d6c115e5-538e-4cca-b33f-113d12de8386");

        //private Guid MainSeria = Guid.Parse("4e92555e-f69b-47a6-8721-68150ef48e03");

        //
        // GET: /Contract/
        public ActionResult Index(int? page, decimal? contractnumber, Guid? ImportLogId, Guid? borderoid)
        {

            ViewBag.filtr = "";
            ViewBag.contractnumber = contractnumber;

            //серия по умолчанию
            ViewBag.seria = MainSeria;

            var pageNumber = page ?? 1;

            string userid = User.Identity.GetUserId();
            // var agentuser= db.AgentUsers.SingleOrDefault(x=>x.UserId==userid);

            if (ImportLogId != null)
            {
                var imp = db.ImportLogs.SingleOrDefault(x => x.ImportLogId == ImportLogId);

                ViewBag.filtr = string.Format("импорт #{0} от {1}", imp.docnum, imp.dateinsert);
            }

            if (borderoid.HasValue)
            {
                var bordero = db.Borderoes.SingleOrDefault(x => x.BorderoId == borderoid);
                ViewBag.filtr = string.Format("бордеро #{0}", bordero.DocNum);
            }


            //var clist = from c in db.v_contract select c;
            var clist = db.spContract(userid, contractnumber, ImportLogId, null, borderoid);

            if (contractnumber != null)
                ViewBag.filtr = "номер договора = " + contractnumber.ToString();


            //TODO 27042015 добавить в таблицу seria поле PrintFunction добавить во вью
            
           
            if (clist != null)
            {

                return View(clist.ToList().ToPagedList(pageNumber, 25));
            }


            return View();


        }

        private List<v_agentseria> getAgentSeias()
        {
            //TODO 27042015 добавить вьюху v_agentseria

            var userserias = (List<v_agentseria>)Session["userserias"];

            if (userserias == null)
            {
                string userid = User.Identity.GetUserId();
                userserias = db.v_agentseria.Where(x => x.UserId == userid).ToList();

                Session["userserias"] = userserias;
            }

            return userserias;
        }

        public PartialViewResult _mainMenuCreateContract()
        {
     
            return PartialView(getAgentSeias());
        }

        public ActionResult _tools_add_contract_btn()
        {
            //TODO 27042015 добавить и заполнить таблицу agentseria
            var available_serias = getAgentSeias();

            //ViewBag.available_serias = available_serias;

            return PartialView(available_serias);
        }

        private void Contract_ini(Guid contract_id)
        {
            ViewBag.currency = new SelectList(db.Currencies.ToList(), "currencyid", "code");

            //ViewBag.risklist = db.v_contract_risk.Where(x => x.ContractId == contract_id).OrderBy(o => o.sort);

            ViewBag.PeriodMultiType = new SelectList(new[]{
                new SelectListItem(){Text="За весь период", Value="1"},
                new SelectListItem(){Text="за одну поездку",Value="2"}
            }, "Value", "Text");

        }



        public PartialViewResult build_contract_territory(Guid? ContractTerritoryId, Guid? contractid)
        {
            var t = db.Contract_territory.FirstOrDefault(x => x.ContractTerritoryId == ContractTerritoryId);
            ViewBag.territory = new SelectList(db.Territories.ToList(), "TerritoryId", "name", (t != null) ? t.TerritoryId : null);

            if (t == null)
            {
                t = new Contract_territory();
                t.ContractId = contractid.Value;
                t.ContractTerritoryId = Guid.NewGuid();

            }

            return PartialView(t);
        }

        public ActionResult Contract_create(Guid seriaid)
        {

            var seria = db.serias.FirstOrDefault(x => x.SeriaId == seriaid);

            if (seria == null)
            {
                return HttpNotFound();
            }
            Contract c = new Contract(db);
            c.seriaid = seriaid;
            c.currencyid = seria.DefaultCurrencyId.Value;
            c.date_out = DateTime.Now;

            //if (p_contract_add(c))
            if (c.add_contract(User.Identity.GetUserId()))
                return RedirectToAction("Contract_edit", new { id = c.ContractId });


            return RedirectToAction("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContractCrm(Contract c, string caction = "save")
        {
            List<string> ErrMess = new List<string>();

            //очистить застрахованных от удаленных
            c.SubjectClearDeleted();

            c.db = db;
            c.date_diff = mLib.get_period_diff(c.date_begin, c.date_end);

            //c.Holder_SubjectId = c.Subject.SubjectId;

            //пересчет
            bool isCalculated = ContractRecalc(c, ErrMess);

            if (caction == "recalc" || caction == "confirm")
            {
                if (!isCalculated)
                {
                    foreach (var e in ErrMess)
                        ModelState.AddModelError(string.Empty, e);
                }
            }

            if (caction == "confirm")
            {
                if (c.date_diff<=0)
                    ModelState.AddModelError(string.Empty, "Период задан не верно!");

                if (!c.date_begin.HasValue)
                    ModelState.AddModelError(string.Empty, "Дата начала договора не заполнена!");

                if (string.IsNullOrWhiteSpace(c.Subject.Name1))
                    ModelState.AddModelError(string.Empty, "ФИО страхователя не заполнено!");

                if (!c.Subject.DateOfBirth.HasValue)
                    ModelState.AddModelError(string.Empty, "Дата рождения страхователя не заполнена!");

                if (c.date_begin < c.date_out)
                    ModelState.AddModelError(string.Empty, "Дата начала договора не может быть меньше даты выдачи!");

                if (ModelState.IsValid)
                    c.ContractStatusId = c.change_status(User.Identity.GetUserId(), "confirmed");
            }

            ContractSave(c);

            if (ModelState.IsValid && caction == "save")
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                if (caction == "recalc")
                {
                    return Redirect(Url.RouteUrl(new { Controller = "Contract", Action = "Contract_edit", id = c.ContractId }) + "#block-total");
                }
                return RedirectToAction("Index");
            }
            Contract_ini(c.ContractId);

            var retc = db.Contracts.Include("Contract_territory")
                .Include("ContractConditions")
                .Include("Subjects")
                .Include("ContractRisks")
                .Include("ContractStatu")
                .SingleOrDefault(x => x.ContractId == c.ContractId);
            retc.ContractConditions = retc.ContractConditions.OrderBy(o => o.num).ToList();
            foreach (var cc in retc.ContractConditions)
            {
                cc.Condition = db.Conditions.SingleOrDefault(x => x.ConditionId == cc.ConditionId);
            }
            foreach (var rr in retc.ContractRisks)
            {
                rr.Risk = db.Risks.SingleOrDefault(x => x.RiskId == rr.RiskId);
            }
            ViewBag.terr_count = retc.Contract_territory.Count();

            return View(retc);

        }

        public ActionResult ContractCh(Guid contractid)
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContractAg(Contract c, string caction = "save")
        {
            c.db = db;
            c.date_diff = mLib.get_period_diff(c.date_begin, c.date_end);

            //очистить застрахованных от удаленных
            c.SubjectClearDeleted();

            c.Holder_SubjectId = c.Subject.SubjectId;

            if (caction == "confirm")
            {
                if (!c.date_begin.HasValue)
                    ModelState.AddModelError(string.Empty, "Дата начала договора не заполнена!");

                if (string.IsNullOrWhiteSpace(c.Subject.Name1))
                    ModelState.AddModelError(string.Empty, "ФИО страхователя не заполнено!");

                if (!c.Subject.DateOfBirth.HasValue)
                    ModelState.AddModelError(string.Empty, "Дата рождения страхователя не заполнена!");

                if (string.IsNullOrWhiteSpace(c.Subject.Pasport))
                    ModelState.AddModelError(string.Empty, "Паспорт страхователя не заполнен!");

                if (c.date_begin < c.date_out)
                    ModelState.AddModelError(string.Empty, "Дата начала договора не может быть меньше даты выдачи!");

                if (ModelState.IsValid)
                    c.ContractStatusId = c.change_status(User.Identity.GetUserId(), "confirmed");
            }

            ContractSave(c);

            if (ModelState.IsValid && caction == "save")
                return RedirectToAction("Index");

            Contract_ini(c.ContractId);

            var retc = db.Contracts.Include("Contract_territory")
                .Include("ContractConditions")
                .Include("Subjects")
                .Include("ContractRisks")
                .Include("ContractStatu")
                .SingleOrDefault(x => x.ContractId == c.ContractId);
            retc.ContractConditions = retc.ContractConditions.OrderBy(o => o.num).ToList();
            foreach (var cc in retc.ContractConditions)
            {
                cc.Condition = db.Conditions.SingleOrDefault(x => x.ConditionId == cc.ConditionId);
            }
            foreach (var rr in retc.ContractRisks)
            {
                rr.Risk = db.Risks.SingleOrDefault(x => x.RiskId == rr.RiskId);
            }
            ViewBag.terr_count = retc.Contract_territory.Count();

            return View(c);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ContractCh(Contract c, string caction = "save")
        //{

        //    string errmess = "";
        //    c.db = db;
        //    c.date_diff = mLib.get_period_diff(c.date_begin, c.date_end);

        //    //пересчет
        //    bool isCalculated = ContractRecalc(c, out errmess);

        //    if (caction == "recalc" || caction == "confirm")
        //    {
        //        if (!isCalculated)
        //            ModelState.AddModelError(string.Empty, errmess);

        //        if (caction == "confirm" && isCalculated)
        //        {
        //            //валидация пеперд утверждением 
        //            if (!c.date_begin.HasValue)
        //                ModelState.AddModelError(string.Empty, "Нужно указать дату начала");

        //            if (!c.date_end.HasValue)
        //                ModelState.AddModelError(string.Empty, "Нужно указать дату окончания");

        //            if (c.ContractRisks.Sum(x => x.InsPremRur) == 0)
        //                ModelState.AddModelError(string.Empty, "Премия не должна быть равной 0");

        //            if (ModelState.IsValid)
        //                c.ContractStatusId = c.change_status(User.Identity.GetUserId(), "confirmed");

        //        }
        //    }


        //    ContractSave(c);

        //    if (ModelState.IsValid)
        //    {

        //        if (caction == "recalc")
        //        {

        //            return Redirect(Url.RouteUrl(new { Controller = "Contract", Action = "Contract_edit", id = c.ContractId }) + "#block-total");
        //            //return RedirectToAction("Contract_edit", new { id = c.ContractId,block-total });
        //        }

        //        return RedirectToAction("Index");
        //    }

        //    Contract_ini(c.ContractId);

        //    var retc = db.Contracts.Include("Contract_territory")
        //        .Include("ContractConditions")
        //        .Include("Subjects")
        //        .Include("ContractRisks")
        //        .Include("ContractStatu")
        //        .SingleOrDefault(x => x.ContractId == c.ContractId);
        //    retc.ContractConditions = retc.ContractConditions.OrderBy(o => o.num).ToList();
        //    foreach (var cc in retc.ContractConditions)
        //    {
        //        cc.Condition = db.Conditions.SingleOrDefault(x => x.ConditionId == cc.ConditionId);
        //    }
        //    foreach (var rr in retc.ContractRisks)
        //    {
        //        rr.Risk = db.Risks.SingleOrDefault(x => x.RiskId == rr.RiskId);
        //    }
        //    ViewBag.terr_count = retc.Contract_territory.Count();

        //    return View(retc);
        //}
        public static int GetAge(DateTime birthDate, DateTime date)
        {
            int years = date.Year - birthDate.Year;
            if (date.Month < birthDate.Month || (date.Month == birthDate.Month && date.Day < birthDate.Day))
                years--;
            return years;
        }

        private class factorgrp
        {
            public string ftype { get; set; }
            public string fvalue { get; set; }
            public decimal qnt { get; set; }
        }

        private decimal CalcSubjectsPremium(IEnumerable<Subject> subjects,
            Guid seriaid,
            decimal sum,
            DateTime curdate,
            List<factorgrp> factor_descr)
        {
            decimal subj_sum = 0;

            foreach (var s in subjects)
            {
                if (s.num == -1)
                    continue;

                decimal age = GetAge(s.DateOfBirth.Value, curdate);

                var fs = db.Factors.FirstOrDefault(x => x.SeriaId == seriaid
                    && x.ValueFrom <= age && x.ValueTo >= age
                    && x.FactorType == "age");

                if (fs == null)
                {
                    subj_sum += sum;
                }
                else
                {
                    subj_sum += sum * fs.Factor1.Value;
                    factor_descr.Add(new factorgrp { ftype = "возраст", fvalue = fs.Factor1.Value.ToString() });

                }
            }



            return subj_sum;
        }

        private decimal CalcSubjectsPremium(IEnumerable<Subject> subjects,
    Guid seriaid,
    decimal sum,
    DateTime curdate
    )
        {


            return CalcSubjectsPremium(subjects, seriaid, sum, curdate, new List<factorgrp> { });
        }

        private decimal getXtrimFactor(IEnumerable<ContractCondition> conds,Guid seriaid,
            List<factorgrp> factor_descr)
        {
            decimal fct=1;
            var dbextrim = db.Conditions.FirstOrDefault(x => x.Code == "extrim");

            if(dbextrim!=null)
            {
                var cextrim = conds.FirstOrDefault(x => x.ConditionId == dbextrim.ConditionId);
                if(cextrim!=null && cextrim.Val_c=="on")
                {
                   var vfct = db.Factors.FirstOrDefault(x => x.FactorType == "extrim" && x.SeriaId==seriaid);
                   if (vfct != null)
                   {
                       fct = vfct.Factor1.Value;
                       factor_descr.Add(new factorgrp { ftype = "экстрим", fvalue = fct.ToString() });
                   }
                       
                }
            }


           

            return fct;
        }

        private bool ContractRecalc(Contract c, List<string> ErrMess)
        {
            bool ret = true;

            decimal date_diff = (decimal)c.date_diff;

           
            try
            {

                var cter = c.Contract_territory.FirstOrDefault();

                //TODO пока скидки надбавки по всему полису, далее переделать на порисковые скидки
                //скидки надбавки

                //идем по людям
                //валидация
                foreach (var s in c.Subjects)
                {
            

                    if (!s.DateOfBirth.HasValue)
                    {
                        ErrMess.Add(string.Format("У застрахованного {0} не заполнено поле дата рождения", s.Name1));
                        ret = false;
                    }
                }

                foreach (var crisk in c.ContractRisks)
                {
                    crisk.BaseTarif = 0;
                    crisk.InsPrem = 0;
                    crisk.InsFee = 0;
                    crisk.InsPremRur = 0;
                    crisk.FactorsDescr = "";

                    //найдем тариф
                    var t = (from tr in db.Tarifs
                             where tr.SeriaId == c.seriaid &&
                             tr.TerritoryId == cter.TerritoryId &&
                             tr.RiskId == crisk.RiskId
                             select tr).FirstOrDefault();

                 
                    if (t != null && ret)
                    {
                        crisk.BaseTarif = (decimal)t.PremSum;
                        decimal riskprem = (decimal)crisk.BaseTarif * (decimal)c.date_diff;

                        List<factorgrp> factor_descr = new List<factorgrp>();

                        riskprem = riskprem * getXtrimFactor(c.ContractConditions,c.seriaid, factor_descr);

                        crisk.InsPrem = CalcSubjectsPremium(c.Subjects, c.seriaid, riskprem, c.date_out.Value, factor_descr);
                        crisk.InsFee = CalcSubjectsPremium(c.Subjects, c.seriaid, (decimal)t.InsFee * (decimal)c.date_diff, c.date_out.Value);
                        crisk.InsPremRur = crisk.InsPrem * CurrManage.getCurRate(db, c.currencyid, c.date_out);

                        //TODO 27042015 добавить поле FactorsDescr в таблицу [ContractRisk]
                        var fgrp = from n in factor_descr
                                   group n by new {t= n.ftype, v =n.fvalue} into g
                                   select new factorgrp {ftype = g.Key.t, fvalue =g.Key.v, qnt= g.Count()};
                                  
                        foreach(var f in fgrp)
                        {
                            crisk.FactorsDescr += string.Format("{0}: {1}({2}); ",f.ftype,f.fvalue,f.qnt);
                        }
                        

                        //crisk.BaseTarif = (decimal)t.PremSum;
                        //dcount = (decimal)(c.date_diff * c.Subjects.Count());
                        //crisk.InsPrem = crisk.BaseTarif * dcount;
                        //crisk.InsFee = (decimal)t.InsFee * dcount;
                        //crisk.InsPremRur = crisk.InsPrem * CurrManage.getCurRate(db, c.currencyid, c.date_out);
                    }
                    else
                    {
                        ErrMess.Add("Тариф не найден! Обратитесь к администратору");

                        ret = false;
                    }
                }

            }
            catch
            {
                ErrMess.Add("Ошибка при расчете тарифа! Обратитесь к администратору");

                ret = false;
            }

            return ret;
        }



        private bool ContractSave(Contract c)
        {
            bool ret = true;

            if (c.date_out == null)
                c.date_out = DateTime.Now;

            var seria = db.serias.SingleOrDefault(x => x.SeriaId == c.seriaid);
            if (!string.IsNullOrEmpty(seria.numberformat))
                c.contractnumberformat = string.Format(seria.numberformat, c.contractnumber);

            #region Страхователь

            if (!string.IsNullOrEmpty(c.Subject.Name1))
                c.Subject.Name1 = c.Subject.Name1.Trim();

            if (!string.IsNullOrEmpty(c.Subject.Pasport))
                c.Subject.Pasport = c.Subject.Pasport.Trim();

            db.Entry(c.Subject).State = EntityState.Modified;
            #endregion

            #region дополнительные параметры
            foreach (var item in c.ContractConditions)
            {
                //item.Condition.Type

                db.Entry(item).State = EntityState.Modified;
            }
            #endregion

            #region застрахованные
            

            foreach (var s in c.Subjects)
            {
                //s.Name1 = s.Name1.Trim();
                //если -1 значит строка удалена
                if (s.num != -1)
                    db.Entry(s).State = EntityState.Modified;
   
            }
            #endregion

            #region территория
            foreach (var t in c.Contract_territory)
            {
                if (!db.Contract_territory.Any(x => x.ContractTerritoryId == t.ContractTerritoryId))
                {
                    //добавляем
                    var ct = new Contract_territory();
                    ct.ContractTerritoryId = Guid.NewGuid();
                    ct.TerritoryId = t.TerritoryId;
                    ct.ContractId = c.ContractId;

                    db.Contract_territory.Add(ct);
                }
                else
                {
                    //обновляем
                    db.Entry(t).State = EntityState.Modified;
                }

            }
            #endregion

            foreach (var err in db.GetValidationErrors())
            {
                foreach (var e in err.ValidationErrors)
                {
                    ModelState.AddModelError(string.Empty, e.PropertyName + ": " + e.ErrorMessage);
                }

                ret = false;
            }

            if (ret)
            {

                db.SaveChanges();
                c.Contract_territory = db.Contract_territory.Where(x => x.ContractId == c.ContractId).ToList();

                #region Риски
                foreach (var r in c.ContractRisks)
                {
                    db.Entry(r).State = EntityState.Modified;
                }
                #endregion

                db.Entry(c).State = EntityState.Modified;

                db.SaveChanges();

            }

            return ret;
        }


        //[HttpPost]
        //public ActionResult Contract_create(Contract contract, FormCollection oform)
        //{
        //    if(ModelState.IsValid)
        //    {
        //        contract_before_save(ref contract);

        //        #region дополнительные параметры
        //        foreach(var item in contract.ContractConditions)
        //        {
        //            db.Entry(item).State = EntityState.Modified;
        //        }


        //        #endregion

        //        db.Entry(contract).State = EntityState.Modified;

        //        db.SaveChanges();

        //        return RedirectToAction("List");
        //    }

        //    var seria = db.serias.SingleOrDefault(x => x.SeriaId == contract.seriaid);

        //    Contract_ini(contract.ContractId);

        //    return View(seria.formname,contract);
        //    //return View("contract",contract);
        //}




        public ActionResult Contract_edit(Guid id)
        {
            //if (!findcontract(id))
            //    return HttpNotFound();

            var c = db.Contracts.Include("Contract_territory")
                .Include("ContractConditions")
                .Include("Subjects")
                .Include("ContractStatu")
                .SingleOrDefault(x => x.ContractId == id);
            c.db = db;

            if (c == null)
                return HttpNotFound();

            int access = c.checkaccess(User.Identity.GetUserId());

            if (access == 0)
                return HttpNotFound();


            c.Subjects = c.Subjects.OrderBy(x => x.num).ToList();
            c.ContractConditions = c.ContractConditions.OrderBy(o => o.num).ToList();

            ViewBag.terr_count = c.Contract_territory.Count();



            Contract_ini(c.ContractId);


            return View(c.seria.formname, c);
        }

        public ActionResult import_contract(int? page)
        {
            ViewBag.settings = db.import_settings.OrderBy(x => x.numcol).ToList();
            ViewBag.Message = "";

            string userid = User.Identity.GetUserId();

            var pageNumber = page ?? 1;

            var viewlist = db.v_importlog.Where(x => x.userid == userid).OrderByDescending(o => o.dateinsert);


            return View(viewlist.ToPagedList(pageNumber, 10));
        }

        [HttpPost]
        public ActionResult import_contract(HttpPostedFileBase file)
        {
            //http://www.codeproject.com/Tips/752981/Import-Data-from-Excel-File-to-Database-Table-in-A

            ViewBag.Message = "";

            string userid = User.Identity.GetUserId();

            if (file.ContentLength > 0)
            {
                import_xls x = new import_xls(userid, MainSeria);

                if (x.import(file.InputStream))
                    ViewBag.Message = string.Format("Импорт #{0} завершен!", x.lognum);
                else
                    ViewBag.Message = x.error_message;



                ////  var workbook = new XLWorkbook(@"c:\temp\Книга1.xlsx");

                //try
                //{
                //    var workbook = new XLWorkbook(file.InputStream);

                //    var ws = workbook.Worksheet(1);

                //    import_data(ws.RowsUsed(), userid);



                //}
                //catch (Exception e)
                //{
                //    ModelState.AddModelError(string.Empty, "Файл импорта должен иметь формат *.xlsx."
                //        + "(" + e.Message + ")");
                //}

                //  return RedirectToAction("index");
            }


            ViewBag.settings = db.import_settings.OrderBy(x => x.numcol).ToList();

            var viewlist = db.v_importlog.Where(x => x.userid == userid).OrderByDescending(o => o.dateinsert);

            return View(viewlist.ToPagedList(1, 10));

        }

        public void importexapmle()
        {
            XLWorkbook w = new XLWorkbook();
            IXLWorksheet s = w.Worksheets.Add("Импорт");

            var impset = db.import_settings.OrderBy(x => x.numcol);
            int colcount = 0;


            foreach (var i in impset)
            {
                colcount++;
                s.Cell(1, i.numcol.Value).SetValue<string>(i.colname);

                switch (i.colcode.Trim())
                {
                    case "contract_number":
                        s.Cell(2, i.numcol.Value).SetValue<string>("1");
                        s.Cell(5, i.numcol.Value).SetValue<string>("2");
                        break;
                    case "date_begin":
                        s.Cell(2, i.numcol.Value).SetValue<DateTime>(DateTime.Now);
                        s.Cell(5, i.numcol.Value).SetValue<DateTime>(DateTime.Now);
                        break;
                    case "date_end":
                        s.Cell(2, i.numcol.Value).SetValue<DateTime>(DateTime.Now.AddDays(10));
                        s.Cell(5, i.numcol.Value).SetValue<DateTime>(DateTime.Now.AddDays(20));
                        break;
                    case "date_out":
                        s.Cell(2, i.numcol.Value).SetValue<DateTime>(DateTime.Now);
                        s.Cell(5, i.numcol.Value).SetValue<DateTime>(DateTime.Now);
                        break;
                    case "subjname":
                        s.Cell(2, i.numcol.Value).SetValue<string>("Застрахованный 1 дог1");
                        s.Cell(3, i.numcol.Value).SetValue<string>("Застрахованный 2 дог1");
                        s.Cell(4, i.numcol.Value).SetValue<string>("Застрахованный 3 дог1");
                        s.Cell(5, i.numcol.Value).SetValue<string>("Застрахованный 1 дог2");
                        break;
                }
            }

            s.Columns().AdjustToContents();
            s.Range(1, 1, 1, colcount).Style.Font.SetBold();

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=\"import.xlsx\"");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                w.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                memoryStream.Close();
            }

            Response.End();
        }

        //private void import_data(IXLRows rows, string userid)
        //{
        //    Guid log_contract_id;

        //    ImportLog l = new ImportLog(db, userid);

        //    int iusedrow=0;

        //    Guid? newcontractnumber = null;

        //    Contract newcontract = new Contract();

        //    foreach (var row in rows)
        //    {
        //        iusedrow++;

        //        //пропустим шапку
        //        if (iusedrow == 1)
        //            continue;

        //        var crow = readimportrow(row);

        //        //новый договор
        //        if (!newcontract.contractnumber.HasValue || !string.IsNullOrEmpty(crow.contract_number_str) )
        //        {
        //            newcontract = import_data_create_contract(crow);
        //            l.add_log(db, newcontract.ContractId);

        //            continue;      
        //        }

        //        //застрахованный
        //        var s = new Subject();
        //        s.Name1 = crow.SubjName;
        //        s.Gender = mLib.gender_parse(crow.gender);
        //        s.DateOfBirth = crow.dateofbirth;
        //        s.Pasport = crow.pasport;
        //        s.PasportValidDate = crow.passportvaliddate;
        //        s.PlaceOfBirth = crow.placeofbirth;

        //        newcontract.add_insured(db, s);


        //        //разделитель
        //        //if (crow.contract_number == 0
        //        //    && string.IsNullOrEmpty(crow.SubjName))
        //        //    newcontractnumber = 0;

        //        //if(crow.contract_number==0)
        //        //{
        //        //    ModelState.AddModelError(string.Empty, string.Format("Строка {0}: номер не является числом."));
        //        //    continue;
        //        //}

        //        //var contract_one = db.Contracts.FirstOrDefault(x => x.contractnumber == crow.contract_number
        //        //    && x.UserId == userid);

        //        ////застрахованный
        //        //var s = new Subject();
        //        //s.Name1 = crow.SubjName;
        //        //s.Gender = mLib.gender_parse(crow.gender);
        //        //s.DateOfBirth = crow.dateofbirth;
        //        //s.Pasport = crow.pasport;
        //        //s.PasportValidDate = crow.passportvaliddate;
        //        //s.PlaceOfBirth = crow.placeofbirth;

        //        //if(contract_one==null)
        //        //{
        //        //    //создаем новый 
        //        //    Contract contract_new = new Contract();

        //        //    contract_new.seriaid = this.MainSeria;
        //        //    contract_new.contractnumber = crow.contract_number;
        //        //    contract_new.date_out = crow.date_out;
        //        //    contract_new.date_begin = crow.date_begin;
        //        //    contract_new.date_end = crow.date_end;

        //        //    p_contract_add(contract_new);

        //        //    contract_new.add_insured(db, s);

        //        //    log_contract_id =contract_new.ContractId;
        //        //}
        //        //else
        //        //{
        //        //    contract_one.add_insured(db, s);
        //        //    log_contract_id = contract_one.ContractId;
        //        //}


        //        //l.add_log(db, log_contract_id);

        //    }//foreach
        //}



        //private void import





        private void importOledb()
        {
            //string fileLocation = Server.MapPath("~/Content/") + System.IO.Path.GetFileName(file.FileName);

            //              string excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
            //                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";

            //              if (System.IO.File.Exists(fileLocation))
            //              {

            //                  System.IO.File.Delete(fileLocation);
            //              }

            //              file.SaveAs(fileLocation);

            //              OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
            //              excelConnection.Open();

            //              DataTable dt = new DataTable();
            //              dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            //              if (dt == null)
            //              {
            //                  return;
            //              }
            //              String[] excelSheets = new String[dt.Rows.Count];
            //              int t = 0;
            //              foreach (DataRow row in dt.Rows)
            //              {
            //                  excelSheets[t] = row["TABLE_NAME"].ToString();
            //                  t++;
            //              }

            //              string query = string.Format("Select * from [{0}]", excelSheets[0]);
            //              OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);

            //              using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
            //              {
            //                  DataSet ds = new DataSet();

            //                  dataAdapter.Fill(ds);

            //                  for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            //                  {
            //                      string aa = ds.Tables[0].Rows[i][0].ToString();
            //                  }

            //              }


            //              excelConnection.Close();
            //          }
        }


        private void contract_update_territory(Guid contractid, string[] territory)
        {
            if (territory == null)
                return;

            var t_old = db.Contract_territory.Where(x => x.ContractId == contractid).ToList();
            foreach (var item in t_old)
            {
                //удалить
            }

            foreach (string id in territory)
            {
                Guid tid = Guid.Parse(id);
                //добавить
                if (t_old.Where(x => x.TerritoryId == tid).Count() == 0)
                {
                    var tnew = new Contract_territory();

                    tnew.ContractTerritoryId = Guid.NewGuid();
                    tnew.TerritoryId = tid;
                    tnew.ContractId = contractid;

                    db.Contract_territory.Add(tnew);
                }

            }


        }


        private ActionResult contract_terr_insert_row(Guid id, string name, Guid contractid)
        {
            if (db.Contract_territory.Any(x => x.ContractId == contractid && x.TerritoryId == id))
                return null;

            Contract_territory t = new Contract_territory();
            t.ContractId = contractid;
            t.ContractTerritoryId = Guid.NewGuid();
            t.TerritoryId = id;

            db.Contract_territory.Add(t);
            db.SaveChanges();

            int terr_count = db.Contract_territory.Where(x => x.ContractId == contractid).Count();

            ViewData["indx"] = terr_count - 1;

            return PartialView(db.Contract_territory.Include("Territory").SingleOrDefault(x => x.ContractTerritoryId == t.ContractTerritoryId));
            //Contract_territory[" + iterrnum + "]
            //return string.Format("<tr><td  class='input-value'> "
            //    + "<input type='hidden' name='Contract_territory[{3}].ContractTerritoryId' value='{4}' >" +
            //    "<input id='{2}' type='hidden' name='territory' value='{0}' /> {1}</td><td><button class='btn btn-default btn-sm'>x</button></td></tr>",
            //    id//0
            //    , name//1
            //    , "terr_" + id//2
            //    , tcount//3
            //    , t.ContractTerritoryId//4
            //    );
        }
        //private int get_period_diff(DateTime? d1, DateTime? d2)
        //{
        //    if (d1.HasValue && d2.HasValue)
        //        return (d2.Value - d1.Value).Days + 1;

        //    return 0;
        //}

        public ActionResult get_strperiodday(string date_from, string date_to)
        {
            DateTime d1 = DateTime.Now, d2 = DateTime.Now;
            bool isparsed;
            string retval = "0";

            isparsed = DateTime.TryParse(date_from, out d1);
            isparsed = isparsed && DateTime.TryParse(date_to, out d2);

            if (isparsed)
            {

                retval = mLib.get_period_diff(d1, d2).ToString();
            }

            return Content(retval);
        }




        public ActionResult get_periodadd(string date_from, int addday)
        {
            DateTime d1 = DateTime.Now;
            string ret = "";

            if (DateTime.TryParse(date_from, out d1))
            {
                ret = d1.AddDays(addday).ToShortDateString();
            }


            // string retval = date_from.AddDays(addday).ToShortDateString();
            return Content(ret);
        }

        public ActionResult _addInsuredRow(string contractid, int indx, string fieldlist)
        {
            //это чтоб работало в ie, иначе ajax запросы будут кешироваться
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());

            Guid gContractId = Guid.Parse(contractid);

            //if (!findcontract(gContractId))
            //            return HttpNotFound();

            string userid = User.Identity.GetUserId();

            var contr = db.Contracts.Where(x => x.ContractId == gContractId);


            if (!User.IsInRole("Admin"))
                contr = contr.Where(x => x.UserId == userid);

            ViewData["indx"] = indx;//db.Subjects.Count(x => x.ContractId == gContractId);
            ViewBag.fieldlist = fieldlist;

            var s = contr.Single().add_insured(db);

            ViewBag.viewonly = false;

            //s.SubjectId = Guid.NewGuid();
            //s.ContractId = gContractId;
            //s.num = db.Subjects.Where(x => x.ContractId == gContractId).Count() + 1;

            //db.Subjects.Add(s);
            //db.SaveChanges();



            ViewBag.Gender = mLib.GenderList();

            return PartialView(s);
        }

        public ActionResult _edtInsuredRow(Guid SubjectId, int indx, string fieldlist)
        {
            Subject s = db.Subjects.SingleOrDefault(x => x.SubjectId == SubjectId);

            ViewBag.viewonly = false;

            if (s.Contract.ContractStatu.Status.Readonly == 1)
                ViewBag.viewonly = true;

            ViewBag.Gender = mLib.GenderList(s.Gender);

            ViewData["indx"] = indx;

            ViewBag.fieldlist = fieldlist;

            return PartialView("_addInsuredRow", s);
        }

        [HttpPost]
        public void _removeInsuredRow(Guid subject_id)
        {
            var s = db.Subjects.SingleOrDefault(x => x.SubjectId == subject_id);
            if (s != null)
            {
                db.Subjects.Remove(s);
                db.SaveChanges();
            }

            return;
        }

        public ActionResult _ConditionsAddRefs(string condcode, string addcode)
        {
            string content = string.Format("<datalist id='{0}'>", condcode.Trim() + addcode.Trim());

            var addr = db.AddRefs.Where(x => x.Code == addcode).OrderBy(o => o.OrderNum);
            foreach (var itm in addr)
            {
                content += "<option>" + itm.Value + "</option>";
            }

            content += "</datalist>";

            return Content(content);
        }

        [Authorize(Roles = @"Admin")]
        public ActionResult contract_annul(Guid contractid)
        {
            var c = db.Contracts.SingleOrDefault(x => x.ContractId == contractid);
            c.db = db;

            c.ContractStatusId = c.change_status(User.Identity.GetUserId(), "annul");

            db.SaveChanges();

            return RedirectToAction("index");
        }

        public ActionResult history(Guid id)
        {

            return View(db.v_contract_history.Where(x => x.ContractId == id).OrderByDescending(o => o.DateInsert).ToList());
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public void printpdfch(Guid contractid)
        {

            //var htmlContent = String.Format("<body>Hello world: {0}</body>", DateTime.Now);
            var c = db.Contracts.SingleOrDefault(x => x.ContractId == contractid);


            cl_printmodel pmodel = new cl_printmodel();
            pmodel.contract = c;

            pmodel.entryport = c.ContractConditions.Where(x => x.Condition.Code.Trim() == "entryport").FirstOrDefault().Val_c;

            pmodel.exitport = c.ContractConditions.Where(x => x.Condition.Code.Trim() == "exitport").FirstOrDefault().Val_c;

            pmodel.route = c.ContractConditions.Where(x => x.Condition.Code.Trim() == "route").FirstOrDefault().Val_c;

            var htmlContent = RenderRazorViewToString("generatepdf_ch", pmodel);

            var pdfgen = new NReco.PdfGenerator.HtmlToPdfConverter();

          

            //pdfgen.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
            pdfgen.Orientation = NReco.PdfGenerator.PageOrientation.Portrait;
            pdfgen.Size = NReco.PdfGenerator.PageSize.A4;
            
            var pdfBytes = pdfgen.GeneratePdf(htmlContent);

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", string.Format("attachment;filename=\"polis{0}.pdf\"", c.contractnumber));

            Response.OutputStream.Write(pdfBytes, 0, pdfBytes.Count());

            Response.End();

        }

        public void printcrm(Guid contractid)
        {
           var htmlContent = RenderRazorViewToString("printcrm", null);

           // var pdfgen = new NReco.PdfGenerator.HtmlToPdfConverter();

           // pdfgen.Orientation = NReco.PdfGenerator.PageOrientation.Portrait;

           //// var pdfBytes = 
           //     pdfgen.GeneratePdf(htmlContent, "", Response.OutputStream);

           
            var pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(htmlContent);

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=\"poliscrm.pdf\"");

           Response.OutputStream.Write(pdfBytes, 0, pdfBytes.Count());

            Response.End();
        }

        public void printag(Guid contractid)
        {

            var model = db.v_contract_ag.SingleOrDefault(x => x.ContractId == contractid);

            var htmlContent = RenderRazorViewToString("printag", model);

            var pdfgen = new NReco.PdfGenerator.HtmlToPdfConverter();

            //pdfgen.Orientation = NReco.PdfGenerator.PageOrientation.Portrait;
            
            var pdfBytes = pdfgen.GeneratePdf(htmlContent);

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", string.Format("attachment;filename=\"polis{0}.pdf\"", "AG"));

            Response.OutputStream.Write(pdfBytes, 0, pdfBytes.Count());

            Response.End();
        }

        //private bool findcontract(Guid contract_id)
        //{
        //    bool ret = false;

        //    if (User.IsInRole("Admin"))
        //        return true;

        //    string userid = User.Identity.GetUserId();

        //    ret = db.Contracts.Any(x => x.ContractId == contract_id && x.UserId == userid);

        //    return ret;
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