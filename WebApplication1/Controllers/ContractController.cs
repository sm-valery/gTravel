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

namespace gTravel.Controllers
{
    [Authorize]
    public class ContractController : Controller
    {
        private goDbEntities db = new goDbEntities();
        private Guid MainSeria = Guid.Parse("4e92555e-f69b-47a6-8721-68150ef48e03");

        //
        // GET: /Contract/
        public ActionResult Index(int? page, decimal? contractnumber, Guid? ImportLogId)
        {

            ViewBag.filtr = "";
            ViewBag.contractnumber = contractnumber;

            //серия по умолчанию
            ViewBag.seria = MainSeria;


            var pageNumber = page ?? 1;

            string userid = User.Identity.GetUserId();
            // var agentuser= db.AgentUsers.SingleOrDefault(x=>x.UserId==userid);

            //var clist = from c in db.v_contract select c;
            var clist = db.spContract(userid, contractnumber, ImportLogId, null);

            if (contractnumber != null)
                ViewBag.filtr = "номер договора = " + contractnumber.ToString();


            if (ImportLogId != null)
            {
                var imp = db.ImportLogs.SingleOrDefault(x => x.ImportLogId == ImportLogId.Value);

                ViewBag.filtr =string.Format("импорт #{0} от {1}",imp.docnum,imp.dateinsert);
            }
                


            if (clist != null)
            {

                return View(clist.ToList().ToPagedList(pageNumber, 25));
            }


            return View();

            //эту проверку надо как-то вынести в класс
            //админ видет все
            //if (!User.IsInRole("Admin"))
            //{
            //    //видит все договора агента
            //    if(agentuser.IsGlobalUser==1)
            //        clist = clist.Where(x => x.AgentId == agentuser.AgentId);

            //    //только свои договора
            //    if(agentuser.IsGlobalUser==2)
            //        clist = clist.Where(x => x.UserId == userid);

            //}

            //if (contractnumber != null)
            //{
            //    ViewBag.filtr = "номер договора = " + contractnumber.ToString();

            //    return View(clist.Where(x => x.contractnumber == contractnumber).OrderBy(o => o.contractnumber).ToPagedList(pageNumber, 25));
            //}

            //if(ImportLogId!=null)
            //{
            //    clist = from c in clist
            //            join h in db.ImportLogContracts on c.ContractId equals h.ContractId
            //            where h.ImportLogId == ImportLogId
            //            select c;

            //    var imp = db.ImportLogs.SingleOrDefault(x=>x.ImportLogId == ImportLogId.Value);
            //    if(imp!= null)
            //        ViewBag.filtr = "импорт от " + imp.dateinsert.ToString();
            //}

            //return View(clist.OrderByDescending(o => o.date_out).ToPagedList(pageNumber, 25));
        }

        //public ActionResult List(decimal? contractnumber)
        //{

        //    ViewBag.contractnumber = contractnumber;

        //    if (contractnumber != null)
        //        return View( db.Contracts.Where(x => x.contractnumber == contractnumber).ToList());

        //    return View(db.Contracts.ToList());
        //}

        private void Contract_ini(Guid contract_id)
        {
            ViewBag.currency = new SelectList(db.Currencies.ToList(), "currencyid", "code");
            // ViewBag.territory = new SelectList(db.Territories.ToList(), "TerritoryId", "name", "a4d9e80a-72be-463b-b696-92737eba1060");

            ViewBag.risklist = db.v_contractrisk.Where(x => x.ContractId == contract_id).OrderBy(o => o.sort);

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
            Contract c = new Contract();
            c.seriaid = seriaid;
            c.currencyid = seria.DefaultCurrencyId.Value;
            c.date_out = DateTime.Now;

            //if (p_contract_add(c))
            if (c.add_contract(db,User.Identity.GetUserId()))
                return RedirectToAction("Contract_edit", new { id = c.ContractId });


            return RedirectToAction("Index");


            // #region Subject

            // Subject s = new Subject();

            // s.SubjectId = Guid.NewGuid();
            // s.Type = "fiz";

            // db.Subjects.Add(s);
            // #endregion

            // Guid contr_stat_id = Guid.NewGuid();

            // #region contract
            // Contract c = new Contract();

            // c.ContractId = Guid.NewGuid();
            // c.seriaid = seriaid;
            // c.currencyid = seria.DefaultCurrencyId.Value;
            // c.date_begin = null;
            // c.date_end = null;
            // c.date_out = DateTime.Now;
            // c.Holder_SubjectId = s.SubjectId;
            // c.contractnumber = null;
            // c.ContractStatusId = contr_stat_id;
            //// c.StatusId = db.Status.SingleOrDefault(x=>x.Code.Trim()=="project").StatusId;

            // c.UserId = User.Identity.GetUserId();
            // db.Contracts.Add(c);

            // ContractStatu stat = new ContractStatu();
            // stat.ContractStatusId = contr_stat_id;
            // stat.ContractId = c.ContractId;
            // stat.StatusId = db.Status.SingleOrDefault(x => x.Code.Trim() == "project").StatusId;
            // stat.DateInsert = DateTime.Now;
            // stat.UserId = User.Identity.GetUserId();

            // db.ContractStatus.Add(stat);

            // #endregion




            // #region риски

            // var cr = db.RiskSerias.Where(x => x.SeriaId == c.seriaid).OrderBy(o=>o.sort);

            // foreach(var item in cr)
            // {
            //     ContractRisk item_rs = new  ContractRisk();

            //     item_rs.ContractRiskId = Guid.NewGuid();
            //     item_rs.ContractId = c.ContractId;
            //     item_rs.RiskId = item.RiskId;
            //     //item_rs.Risk = item.Risk;

            //     c.ContractRisks.Add(item_rs);
            // }

            // #endregion

            // #region доп параметры
            // var cs = db.ConditionSerias.Include("Condition").Where(x => x.SeriaId == c.seriaid).OrderBy(o => o.num);

            // foreach (var item in cs)
            // {
            //     ContractCondition cc = new ContractCondition();
            //     cc.ContractCondId = Guid.NewGuid();
            //     cc.ConditionId = item.ConditionId;
            //     cc.Contractid = c.ContractId;
            //     //cc.Condition = item.Condition;
            //     cc.num = item.num;

            //     switch(item.Condition.Type)
            //     {
            //         case "L":
            //             cc.Val_l = false;
            //             break;
            //     }


            //     db.ContractConditions.Add(cc);

            // }
            // #endregion

            // db.SaveChanges();

            //return RedirectToAction(seria.formname, "Contract", new { contractid = c.ContractId });


            //Contract_ini(c.ContractId);

            //ViewBag.risklist = db.v_contractrisk.Where(x => x.ContractId == c.ContractId).OrderBy(o=>o.sort);

            //return View("Contract", db.Contracts.SingleOrDefault(x=>x.ContractId==c.ContractId));

        }

        //private bool p_contract_add(Contract c)
        //{
        //    var seria = db.serias.FirstOrDefault(x => x.SeriaId == c.seriaid);

        //    if (seria == null)
        //    {
        //        return false;
        //    }

        //    #region Застрахованный

        //    Subject s = new Subject();

        //    s.SubjectId = Guid.NewGuid();
        //    s.Type = "fiz";

        //    db.Subjects.Add(s);
        //    #endregion

        //    Guid contr_stat_id = Guid.NewGuid();

        //    #region contract
        //    //Contract c = new Contract();

        //    c.ContractId = Guid.NewGuid();
        //    c.currencyid = (c.currencyid == Guid.Parse("{00000000-0000-0000-0000-000000000000}")) ? seria.DefaultCurrencyId.Value : c.currencyid;

        //    c.date_begin = c.date_begin;
        //    c.date_end = c.date_end;
        //    c.date_out = (c.date_out == null) ? DateTime.Now : c.date_out;
        //    c.date_diff = get_period_diff(c.date_begin, c.date_end);
        //    c.Holder_SubjectId = s.SubjectId;

        //    if (seria.AutoNumber == 1)
        //        c.contractnumber = c.getnextnumber(db, c.seriaid);

        //    c.ContractStatusId = contr_stat_id;

        //    c.UserId = User.Identity.GetUserId();
        //    db.Contracts.Add(c);

        //    ContractStatu stat = new ContractStatu();
        //    stat.ContractStatusId = contr_stat_id;
        //    stat.ContractId = c.ContractId;
        //    stat.StatusId = db.Status.SingleOrDefault(x => x.Code.Trim() == "project").StatusId;
        //    stat.DateInsert = DateTime.Now;
        //    stat.UserId = User.Identity.GetUserId();

        //    db.ContractStatus.Add(stat);

        //    #endregion

        //    #region риски

        //    var cr = db.RiskSerias.Where(x => x.SeriaId == c.seriaid).OrderBy(o => o.sort);

        //    foreach (var item in cr)
        //    {
        //        ContractRisk item_rs = new ContractRisk();

        //        item_rs.ContractRiskId = Guid.NewGuid();
        //        item_rs.ContractId = c.ContractId;
        //        item_rs.RiskId = item.RiskId;
        //        //item_rs.Risk = item.Risk;

        //        c.ContractRisks.Add(item_rs);
        //    }

        //    #endregion

        //    #region доп параметры
        //    var cs = db.ConditionSerias.Include("Condition").Where(x => x.SeriaId == c.seriaid).OrderBy(o => o.num);

        //    foreach (var item in cs)
        //    {
        //        ContractCondition cc = new ContractCondition();
        //        cc.ContractCondId = Guid.NewGuid();
        //        cc.ConditionId = item.ConditionId;
        //        cc.Contractid = c.ContractId;
        //        //cc.Condition = item.Condition;
        //        cc.num = item.num;

        //        switch (item.Condition.Type)
        //        {
        //            case "L":
        //                cc.Val_l = false;
        //                break;
        //        }


        //        db.ContractConditions.Add(cc);

        //    }
        //    #endregion

        //    db.SaveChanges();

        //    return true;
        //}

        //private bool p_assured_add(Subject s)
        //{

        //    if (!findcontract(s.ContractId.Value))
        //        return false;

        //   // Subject s = new Subject();
        //    s.SubjectId = Guid.NewGuid();
        //    s.num = db.Subjects.Where(x => x.ContractId == s.ContractId).Count() + 1;

        //    db.Subjects.Add(s);
        //    db.SaveChanges();
        //    return true;
        //}

        public ActionResult ContractCh(Guid contractid)
        {

            //Contract_ini(contractid);

            //var c = db.Contracts.SingleOrDefault(x => x.ContractId == contractid);
            //c.ContractConditions = c.ContractConditions.OrderBy(o => o.num).ToList();

            return View();
        }

        [HttpPost]
        public ActionResult ContractCh(Contract c, string caction = "save")
        {

            string errmess = "";

            c.date_diff = mLib.get_period_diff(c.date_begin, c.date_end);

            //пересчет
            bool isCalculated = ContractRecalc(c, out errmess);

            if (caction == "recalc")
            {
                if (!isCalculated)
                    ModelState.AddModelError(string.Empty, errmess);
            }

            ContractSave(c);

            if (ModelState.IsValid)
            {
                if (caction == "recalc")
                {
                    return Redirect(Url.RouteUrl(new { Controller = "Contract", Action = "Contract_edit", id = c.ContractId }) + "#block-total");
                    //return RedirectToAction("Contract_edit", new { id = c.ContractId,block-total });
                }

                return RedirectToAction("Index");
            }

            Contract_ini(c.ContractId);

            var retc = db.Contracts.Include("Contract_territory").Include("ContractConditions").Include("Subjects").Include("ContractRisks").SingleOrDefault(x => x.ContractId == c.ContractId);
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

        private bool ContractRecalc(Contract c, out string errmess)
        {
            bool ret = true;
            errmess = "";

            try
            {
                var cter = c.Contract_territory.FirstOrDefault();
                var crisk = c.ContractRisks.FirstOrDefault();

                //найдем тариф
                var t = (from tr in db.Tarifs
                         where tr.SeriaId == c.seriaid &&
                         tr.TerritoryId == cter.TerritoryId &&
                         tr.RiskId == crisk.RiskId
                         select tr).FirstOrDefault();

                decimal dcount = 0;
                if (t != null)
                {
                    crisk.BaseTarif = (decimal)t.PremSum;
                    dcount = (decimal)(c.date_diff * c.Subjects.Count());
                    crisk.InsPrem = crisk.BaseTarif * dcount;
                    crisk.InsFee = (decimal)t.InsFee * dcount;
                    crisk.InsPremRur = crisk.InsPrem * CurrManage.getCurRate(db, c.currencyid, c.date_out);
                }
                else
                {
                    errmess = "Тариф не найден! Обратитесь к администратору";
                    ret = false;
                }

            }
            catch
            {

                errmess = "Ошибка при расчете тарифа! Обратитесь к администратору";
                ret = false;
            }

            return ret;
        }

        private void ContractSave(Contract c)
        {
            if (c.date_out == null)
                c.date_out = DateTime.Now;

            #region дополнительные параметры
            foreach (var item in c.ContractConditions)
            {
                db.Entry(item).State = EntityState.Modified;
            }
            #endregion

            #region застрахованные
            foreach (var s in c.Subjects)
            {
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



            db.SaveChanges();
            c.Contract_territory = db.Contract_territory.Where(x => x.ContractId == c.ContractId).ToList();

            #endregion

            #region Риски
            foreach (var r in c.ContractRisks)
            {
                db.Entry(r).State = EntityState.Modified;
            }
            #endregion

            db.Entry(c).State = EntityState.Modified;

            db.SaveChanges();
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

            var c = db.Contracts.Include("Contract_territory").Include("ContractConditions").Include("Subjects").SingleOrDefault(x => x.ContractId == id);

            if (c == null)
                return HttpNotFound();

            int access = c.checkaccess(db, User.Identity.GetUserId());

            if (access == 0)
                return HttpNotFound();


            c.Subjects = c.Subjects.OrderBy(x => x.num).ToList();
            c.ContractConditions = c.ContractConditions.OrderBy(o => o.num).ToList();

            ViewBag.terr_count = c.Contract_territory.Count();



            Contract_ini(c.ContractId);


            return View(c.seria.formname, c);
        }

        //[HttpPost]
        //public ActionResult Contract_edit(Contract c)
        //{
        //    if(ModelState.IsValid)
        //    {
        //        ContractSave(c);

        //        //contract_before_save(ref c);

        //        ////обновление территории
        //        ////contract_update_territory(c.ContractId, territory);

        //        //#region дополнительные параметры
        //        //foreach (var cond in c.ContractConditions)
        //        //    db.Entry(cond).State = EntityState.Modified;

        //        ////var cond = db.ContractConditions.Where(x => x.Contractid == c.ContractId);

        //        ////foreach (var item in cond)
        //        ////{
        //        ////    bool ischecked = oform.GetValues("cond_" + item.Condition.Code.Trim()).Contains("true");

        //        ////    if (item.Val_l != ischecked)
        //        ////    {
        //        ////        item.Val_l = ischecked;
        //        ////        db.Entry(item).State = EntityState.Modified;
        //        ////    }


        //        ////}
        //        //#endregion

        //        //#region страхователь
        //        //if (c.Subject != null)
        //        //{
        //        //    c.Subject.SubjectId = c.SubjectId.Value;
        //        //    db.Entry(c.Subject).State = EntityState.Modified;
        //        //}
        //        //#endregion

        //        //db.Entry(c).State = EntityState.Modified;

        //        //db.SaveChanges();

        //        return RedirectToAction("Index");
        //    }

        //    Contract_ini(c.ContractId);
        //    return View(c.seria.formname, c);
        //    //return View("contract", c);
        //}


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
                    ViewBag.Message = string.Format("Импорт #{0} завершен!",x.lognum );
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


            foreach(var i in impset)
            {
                colcount++;
                s.Cell(1, i.numcol.Value).SetValue<string>(i.colname);

                switch(i.colcode.Trim())
                {
                    case "contract_number":
                        s.Cell(2,i.numcol.Value).SetValue<string>("1");
                        s.Cell(5, i.numcol.Value).SetValue<string>("2");    
                        break;
                    case "date_begin":
                        s.Cell(2,i.numcol.Value).SetValue<DateTime>(DateTime.Now);
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


        public ActionResult _addInsuredRow(string contractid, int indx)
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

            var s = contr.Single().add_insured(db);



            //s.SubjectId = Guid.NewGuid();
            //s.ContractId = gContractId;
            //s.num = db.Subjects.Where(x => x.ContractId == gContractId).Count() + 1;

            //db.Subjects.Add(s);
            //db.SaveChanges();



            ViewBag.Gender = mLib.GenderList();

            return PartialView(s);
        }

        public ActionResult _edtInsuredRow(Guid SubjectId, int indx)
        {
            Subject s = db.Subjects.SingleOrDefault(x => x.SubjectId == SubjectId);


            ViewBag.Gender = mLib.GenderList(s.Gender);

            ViewData["indx"] = indx;

            return PartialView("_addInsuredRow", s);
        }

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

            var htmlContent = RenderRazorViewToString("generatepdf_ch", c);

            var pdfgen = new NReco.PdfGenerator.HtmlToPdfConverter();

            pdfgen.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
            var pdfBytes = pdfgen.GeneratePdf(htmlContent);

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=\"test.pdf\"");

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