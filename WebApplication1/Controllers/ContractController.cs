using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using gTravel.Models;
using gTravel.Servises;
//using Microsoft.AspNet.Identity;
using PagedList;


namespace gTravel.Controllers
{
    [Authorize]
    public class ContractController : Controller
    {
        private readonly goDbEntities db = new goDbEntities();
        //путешествие по россии
        private readonly Guid MainSeria = Guid.Parse("d6c115e5-538e-4cca-b33f-113d12de8386");
        //private Guid MainSeria = Guid.Parse("4e92555e-f69b-47a6-8721-68150ef48e03");

        //
        // GET: /Contract/
        [UserIdFilter]
        public ActionResult Index(string userid, int? page, decimal? contractnumber, Guid? ImportLogId, Guid? borderoid)
        {
            ViewBag.filtr = "";
            ViewBag.contractnumber = contractnumber;

            //серия по умолчанию
            ViewBag.seria = MainSeria;

            var pageNumber = page ?? 1;

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

            var clist = db.spContract(userid, contractnumber, ImportLogId, null, borderoid);

            if (contractnumber != null)
                ViewBag.filtr = "номер договора = " + contractnumber;


            //TODO 27042015 добавить в таблицу seria поле PrintFunction добавить во вью


            if (clist != null)
            {
                return View((PagedList<v_contract>) clist.ToList().ToPagedList(pageNumber, 25));
            }


            return View();
        }


        private List<v_agentseria> getAgentSerias(string userid)
        {
            //TODO изменить вьюху v_agentseria

            var userserias = (List<v_agentseria>) Session["userserias"];

            if (userserias == null)
            {
                userserias = db.v_agentseria.Where(x => x.UserId == userid).ToList();

                Session["userserias"] = userserias;
            }

            return userserias;
        }

        [UserIdFilter]
        public PartialViewResult _mainMenuCreateContract(string userid)
        {
            return PartialView(getAgentSerias(userid));
        }

        [UserIdFilter]
        public ActionResult _tools_add_contract_btn(string userid)
        {
            //TODO 27042015 добавить и заполнить таблицу agentseria
            var available_serias = getAgentSerias(userid);

            //ViewBag.available_serias = available_serias;

            return PartialView(available_serias);
        }


        [UserIdFilter]
        public ActionResult _bonus_btn(Guid seriaid, Guid riskid, Guid contractid, string userid, bool viewonly = false)
        {
          //  var fs = db.Factors.Where(x => x.SeriaId == seriaid && x.RiskId == riskid).OrderBy(o => o.Position);

            ViewBag.viewonly = viewonly;

            ViewBag.contractid = contractid;

            return PartialView(new ContractService(db).BaseFactors(userid,seriaid));
        }

        public PartialViewResult _addbonusRow(Guid factorid,Guid contractid, Guid? riskid, bool xv=false )
        {
            //это чтоб работало в ie, иначе ajax запросы будут кешироваться
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());


            ViewBag.viewonly = xv;

            if (db.ContractFactors.Any(x => x.ContractId == contractid && x.IdFactor == factorid))
                return PartialView("_bonus_list",
                    db.ContractFactors.Where(x => x.ContractId == contractid).OrderBy(o => o.Position).ToList());

            new ContractService(db).AddNewBonusToContract(factorid, contractid);
           

            return PartialView("_bonus_list",
                db.ContractFactors.Where(x => x.ContractId == contractid).OrderBy(o => o.Position).ToList());

            // return PartialView(db.ContractFactors.Where(x=>x.ContractId==contractid).ToList());
        }


        [HttpPost]
        public PartialViewResult _addAgentRow(Guid contractid)
        {
            ContractService cs = new ContractService();
            ViewBag.idx = cs.ContractAgent_Count(contractid);

            ViewBag.agentlist = new SelectList(db.Agents.ToList(), "AgentId", "Name");

           return PartialView(cs.Create_ContractAgent(contractid));
        }

        [ChildActionOnly]
        public PartialViewResult _AgentList(Guid contractid)
        {
            ViewBag.agentlist = new SelectList(db.Agents.ToList(), "AgentId", "Name");

            return PartialView(db.ContractAgents.Where(x => x.ContractId == contractid).ToList());
        }

        public PartialViewResult _dellbonusRow(Guid contractfactorid, bool xv = false)
        {
            Guid contract_id;
            var cvdell = db.ContractFactors.FirstOrDefault(x => x.ContractFactorId == contractfactorid);
            if (cvdell == null)
                return null;

            contract_id = cvdell.ContractId.Value;

            db.ContractFactors.Remove(cvdell);
            db.SaveChanges();


            ViewBag.viewonly = xv;

            return PartialView("_bonus_list",
                db.ContractFactors.Where(x => x.ContractId == contract_id).OrderBy(o => o.Position).ToList());
        }

        public PartialViewResult _bonus_list(Guid contractid, bool viewonly = false)
        {
            ViewBag.viewonly = viewonly;

            return
                PartialView(db.ContractFactors.Where(x => x.ContractId == contractid).OrderBy(o => o.Position).ToList());
        }


        public PartialViewResult _addContractTerritory(Guid contractid, string territorylist)
        {
            var list_comma = territorylist.Split(',');

            List<Contract_territory> tlist = new List<Contract_territory>();

            db.Contract_territory.RemoveRange(db.Contract_territory.Where(x => x.ContractId == contractid));

            foreach(var t in list_comma)
            {
                if (string.IsNullOrEmpty(t))
                    continue;

                var ct = new Contract_territory();
                ct.ContractId = contractid;
                ct.ContractTerritoryId = Guid.NewGuid();
                ct.TerritoryId = Guid.Parse(t);

                tlist.Add(ct);
   
            }

            db.Contract_territory.AddRange(tlist);

            db.SaveChanges();

            return PartialView(tlist);
        }

        private void ContractForm_ini(Contract c)
        {
            ViewBag.currency = new SelectList(db.Currencies.ToList(), "currencyid", "code",c.currencyid);

            //ViewBag.risklist = db.v_contract_risk.Where(x => x.ContractId == contract_id).OrderBy(o => o.sort);

            ViewBag.PeriodMultiType = new SelectList(new[]
            {
                new SelectListItem {Text = "За весь период", Value = "1"},
                new SelectListItem {Text = "за одну поездку", Value = "2"}
            }, "Value", "Text");

            string[] selectedterritory = new string[c.Contract_territory.Count];

            int i=0;
            foreach(var ct in c.Contract_territory)
            {
                selectedterritory[i] = ct.TerritoryId.ToString();
                i++;
            }

            ViewBag.TerritoryId = new MultiSelectList(db.Territories.ToList(),
                "TerritoryId", "name", selectedterritory);

       
            /*
             * 
              string userid = User.Identity.GetUserId();
                userserias = db.v_agentseria.Where(x => x.UserId == userid).ToList();
*/

            //var aa = from ags in db.AgentSerias 
            //         where ags.SeriaId ==

            //ViewBag.hasFactorContrat = db.Factors
        }

        public PartialViewResult build_contract_territory(Guid? ContractTerritoryId, Guid? contractid)
        {
            var t = db.Contract_territory.FirstOrDefault(x => x.ContractTerritoryId == ContractTerritoryId);


            if (t == null)
            {
                t = new Contract_territory();
                t.ContractId = contractid.Value;
                t.ContractTerritoryId = Guid.NewGuid();
                ViewBag.TerritoryId = new MultiSelectList(db.Territories.ToList(), "TerritoryId", "name");

            }
            else
            {
                ViewBag.TerritoryId = new MultiSelectList(db.Territories.ToList(), "TerritoryId", "name", db.Contract_territory.Where(x=>x.ContractId == contractid));

            }

            return PartialView(t);
        }

        [UserIdFilter]
        public ActionResult Contract_create(string userid, Guid seriaid)
        {
            var c = new ContractService(db).create_contract(seriaid, userid);

            if (c != null)
                return RedirectToAction("Contract_edit", new {contractid = c.ContractId});

            return RedirectToAction("Index");
        }

        [HttpPost]
        [UserIdFilter]
        [ValidateAntiForgeryToken]
        public ActionResult ContractCrm(Contract c, string userid, string caction = "save")
        {
            var ErrMess = new List<string>();

            //очистить застрахованных от удаленных
            c.SubjectClearDeleted();

            c.db = db;
            c.date_diff = mLib.get_period_diff(c.date_begin, c.date_end);

            //c.Holder_SubjectId = c.Subject.SubjectId;

            //пересчет
            var isCalculated = ContractRecalc(c, ErrMess);

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
                if (c.date_diff <= 0)
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
                    c.ContractStatusId = c.change_status(userid, "confirmed");
            }

            ContractSave(c);

            if (ModelState.IsValid && caction == "save")
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                if (caction == "recalc")
                {
                    return
                        Redirect(
                            Url.RouteUrl(new { Controller = "Contract", Action = "Contract_edit", contractid = c.ContractId }) +
                            "#block-total");
                }

                if (caction != "confirm")
                    return RedirectToAction("Index");
            }
            ContractForm_ini(c);

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
            //ViewBag.terr_count = retc.Contract_territory.Count();

            return View(retc);
        }

        public ActionResult ContractCh(Guid contractid)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserIdFilter]
        public ActionResult ContractTo(Contract c, string userid, string caction = "save")
        {
            var errMess = new List<string>();


            //очистить застрахованных от удаленных
            c.SubjectClearDeleted();

            c.db = db;
            c.date_diff = mLib.get_period_diff(c.date_begin, c.date_end);


            ContractSave(c);

            //пересчет
            //var isCalculated = ContractRecalc(c, errMess);
            var isCalculated = new ContractService(db).ContractRecalc(c, errMess);

            if (caction == "recalc" || caction == "confirm")
            {
                if (!isCalculated)
                {
                    foreach (var e in errMess)
                        ModelState.AddModelError(string.Empty, e);
                }
            }

            if (caction == "confirm")
            {
                if (c.date_diff <= 0)
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
                {
                    c.ContractStatusId = c.change_status(userid, "confirmed");
                    c.save();
                }
                   
            }
            

            if (ModelState.IsValid && caction == "save")
                return RedirectToAction("Index");


            if (ModelState.IsValid)
            {
                if (caction == "recalc")
                {
                    return
                        Redirect(
                            Url.RouteUrl(new { Controller = "Contract", Action = "Contract_edit", contractid = c.ContractId }) +
                            "#block-total");
                }

                if (caction != "confirm")
                    return RedirectToAction("Index");
            }
            ContractForm_ini(c);


            //var retc = db.Contracts.Include("Contract_territory")
            //    .Include("ContractConditions")
            //    .Include("Subjects")
            //    .Include("ContractRisks")
            //    .Include("ContractStatu")
            //    .SingleOrDefault(x => x.ContractId == c.ContractId);

            //retc.ContractConditions = retc.ContractConditions.OrderBy(o => o.num).ToList();
            
            //foreach (var cc in retc.ContractConditions)
            //{
            //    cc.Condition = db.Conditions.SingleOrDefault(x => x.ConditionId == cc.ConditionId);
            //}

            //foreach (var rr in retc.ContractRisks)
            //{
            //    rr.Risk = db.Risks.SingleOrDefault(x => x.RiskId == rr.RiskId);
            //}
            //ViewBag.terr_count = retc.Contract_territory.Count();

            return View(new ContractService(db).GetContractForEdit(c.ContractId,userid));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserIdFilter]
        public ActionResult ContractAg(Contract c, string userid, string caction = "save")
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
                    c.ContractStatusId = c.change_status(userid, "confirmed");
            }

            ContractSave(c);

            if (ModelState.IsValid && caction == "save")
                return RedirectToAction("Index");

            ContractForm_ini(c);

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
           // ViewBag.terr_count = retc.Contract_territory.Count();

            return View();
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
            var years = date.Year - birthDate.Year;
            if (date.Month < birthDate.Month || (date.Month == birthDate.Month && date.Day < birthDate.Day))
                years--;
            return years;
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
                    subj_sum += sum*fs.Factor1.Value;
                    factor_descr.Add(new factorgrp {ftype = "возраст", fvalue = fs.Factor1.Value.ToString()});
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
            return CalcSubjectsPremium(subjects, seriaid, sum, curdate, new List<factorgrp>());
        }

        private decimal getXtrimFactor(IEnumerable<ContractCondition> conds, Guid seriaid,
            List<factorgrp> factor_descr)
        {
            decimal fct = 1;
            var dbextrim = db.Conditions.FirstOrDefault(x => x.Code == "extrim");

            if (dbextrim != null)
            {
                var cextrim = conds.FirstOrDefault(x => x.ConditionId == dbextrim.ConditionId);
                if (cextrim != null && cextrim.Val_c == "on")
                {
                    var vfct = db.Factors.FirstOrDefault(x => x.FactorType == "extrim" && x.SeriaId == seriaid);
                    if (vfct != null)
                    {
                        fct = vfct.Factor1.Value;
                        factor_descr.Add(new factorgrp {ftype = "экстрим", fvalue = fct.ToString()});
                    }
                }
            }


            return fct;
        }

        private bool ContractRecalc(Contract c, List<string> ErrMess)
        {
            var ret = true;

            var date_diff = (decimal) c.date_diff;


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
                        crisk.BaseTarif = (decimal) t.PremSum;
                        var riskprem = (decimal) crisk.BaseTarif*(decimal) c.date_diff;

                        var factor_descr = new List<factorgrp>();

                        riskprem = riskprem*getXtrimFactor(c.ContractConditions, c.seriaid, factor_descr);

                        crisk.InsPrem = CalcSubjectsPremium(c.Subjects, c.seriaid, riskprem, c.date_out.Value,
                            factor_descr);
                        crisk.InsFee = CalcSubjectsPremium(c.Subjects, c.seriaid,
                            (decimal) t.InsFee*(decimal) c.date_diff, c.date_out.Value);
                        crisk.InsPremRur = crisk.InsPrem*CurrManage.getCurRate(db, c.currencyid, c.date_out);

                        //TODO 27042015 добавить поле FactorsDescr в таблицу [ContractRisk]
                        var fgrp = from n in factor_descr
                            group n by new {t = n.ftype, v = n.fvalue}
                            into g
                            select new factorgrp {ftype = g.Key.t, fvalue = g.Key.v, qnt = g.Count()};

                        var fdescr = new StringBuilder();

                        foreach (var f in fgrp)
                        {
                            fdescr.AppendFormat("{0}: {1}({2}); ", f.ftype, f.fvalue, f.qnt);
                            //crisk.FactorsDescr += string.Format("{0}: {1}({2}); ",f.ftype,f.fvalue,f.qnt);
                        }
                        crisk.FactorsDescr = fdescr.ToString();

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
            var ret = true;

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

            //foreach (var t in c.Contract_territory)
            //{
            //    if (!db.Contract_territory.Any(x => x.ContractTerritoryId == t.ContractTerritoryId))
            //    {
            //        //добавляем
            //        var ct = new Contract_territory();
            //        ct.ContractTerritoryId = Guid.NewGuid();
            //        ct.TerritoryId = t.TerritoryId;
            //        ct.ContractId = c.ContractId;

            //        db.Contract_territory.Add(ct);
            //    }
            //    else
            //    {
            //        //обновляем
            //        db.Entry(t).State = EntityState.Modified;
            //    }
            //}

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
                    if (r.isMandatory)
                        r.ischecked = true;

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

        [UserIdFilter]
        [ContractUserFilter]
        public ActionResult Contract_edit(Guid contractid, string userid)
        {
           // //if (!findcontract(id))
           //  //   return HttpNotFound();

           // var c = db.Contracts.Include("Contract_territory")
           //     .Include("ContractConditions")
           //     .Include("Subjects")
           //     .Include("ContractStatu")
           //     .SingleOrDefault(x => x.ContractId == contractid);
            
           // if (c == null)
           //     return HttpNotFound();

           // c.db = db;          
           // c.CheckFactors(userid);
           // //var access = c.checkaccess(userid: userid);

           // //if (access == 0)
           // //    return HttpNotFound();


           // c.Subjects = c.Subjects.OrderBy(x => x.num).ToList();
           // c.ContractConditions = c.ContractConditions.OrderBy(o => o.num).ToList();

           //// ViewBag.terr_count = c.Contract_territory.Count();

            Contract c = new ContractService(db).GetContractForEdit(contractid,userid);

            ContractForm_ini(c);


            return View(c.seria.formname, c);
        }

        [UserIdFilter]
        public ActionResult import_contract(string userid, int? page)
        {
            ViewBag.settings = db.import_settings.OrderBy(x => x.numcol).ToList();
            ViewBag.Message = "";

            var pageNumber = page ?? 1;

            var viewlist = db.v_importlog.Where(x => x.userid == userid).OrderByDescending(o => o.dateinsert);


            return View(model: (PagedList<v_importlog>) viewlist.ToPagedList(pageNumber, 10));
        }

        [HttpPost]
        [UserIdFilter]
        public ActionResult import_contract(HttpPostedFileBase file, string userid)
        {
            //http://www.codeproject.com/Tips/752981/Import-Data-from-Excel-File-to-Database-Table-in-A

            ViewBag.Message = "";

            if (file.ContentLength > 0)
            {
                var x = new import_xls(userid, MainSeria);

                ViewBag.Message = x.import(file.InputStream) ? string.Format("Импорт #{0} завершен!", x.lognum) : x.error_message;


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

            return View((PagedList<v_importlog>) viewlist.ToPagedList(1, 10));
        }

        public void importexapmle()
        {
         new ContractService(db).GetImportExampleFile();
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

            foreach (var id in territory)
            {
                var tid = Guid.Parse(id);
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

            var t = new Contract_territory();
            t.ContractId = contractid;
            t.ContractTerritoryId = Guid.NewGuid();
            t.TerritoryId = id;

            db.Contract_territory.Add(t);
            db.SaveChanges();

            var terr_count = db.Contract_territory.Where(x => x.ContractId == contractid).Count();

            ViewData["indx"] = terr_count - 1;

            return
                PartialView(
                    db.Contract_territory.Include("Territory")
                        .SingleOrDefault(x => x.ContractTerritoryId == t.ContractTerritoryId));
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
            var retval = "0";

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
            var d1 = DateTime.Now;
            var ret = "";

            if (DateTime.TryParse(date_from, out d1))
            {
                ret = d1.AddDays(addday).ToShortDateString();
            }


            // string retval = date_from.AddDays(addday).ToShortDateString();
            return Content(ret);
        }

        [UserIdFilter]
        public ActionResult _addInsuredRow(string contractid, int indx, string fieldlist, string ins_name, string dayob, string userid)
        {
            //это чтоб работало в ie, иначе ajax запросы будут кешироваться
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());

            var gContractId = Guid.Parse(contractid);

            //if (!findcontract(gContractId))
            //            return HttpNotFound();

        
            var contr = db.Contracts.Where(x => x.ContractId == gContractId);


            if (!User.IsInRole("Admin"))
                contr = contr.Where(x => x.UserId == userid);

            ViewData["indx"] = indx; //db.Subjects.Count(x => x.ContractId == gContractId);
            ViewBag.fieldlist = fieldlist;

            var s = contr.Single().add_insured(db);

            if (!string.IsNullOrEmpty(ins_name))
                s.Name1 = ins_name.Trim();

            if (!string.IsNullOrEmpty(dayob))
            {
                DateTime dob;

                if (DateTime.TryParse(dayob, out dob))
                {
                    s.DateOfBirth = dob;
                }
            }


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
            var s = db.Subjects.SingleOrDefault(x => x.SubjectId == SubjectId);

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
        }

        public ActionResult _ConditionsAddRefs(string condcode, string addcode)
        {
            var content = new StringBuilder();

            content.AppendFormat("<datalist id='{0}'>", condcode.Trim() + addcode.Trim());
            var addr = db.AddRefs.Where(x => x.Code == addcode).OrderBy(o => o.OrderNum);

            foreach (var itm in addr)
            {
                content.Append("<option>" + itm.Value + "</option>");
            }
            content.Append("</datalist>");

            return Content(content.ToString());
            //string content = string.Format("<datalist id='{0}'>", condcode.Trim() + addcode.Trim());

            //var addr = db.AddRefs.Where(x => x.Code == addcode).OrderBy(o => o.OrderNum);
            //foreach (var itm in addr)
            //{
            //    content += "<option>" + itm.Value + "</option>";
            //}

            //content += "</datalist>";

            //return Content(content);
        }

        [Authorize(Roles = @"Admin")]
        [UserIdFilter]
        public ActionResult contract_annul(Guid contractid,string userid)
        {
            var c = db.Contracts.SingleOrDefault(x => x.ContractId == contractid);
            c.db = db;

            c.ContractStatusId = c.change_status(userid, "annul");

            db.SaveChanges();

            return RedirectToAction("index");
        }

        public ActionResult history(Guid id)
        {
            return
                View(db.v_contract_history.Where(x => x.ContractId == id).OrderByDescending(o => o.DateInsert).ToList());
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
            //var c = db.Contracts.SingleOrDefault(x => x.ContractId == contractid);


            //cl_printmodel pmodel = new cl_printmodel();
            //pmodel.contract = c;

            //pmodel.entryport = c.ContractConditions.Where(x => x.Condition.Code.Trim() == "entryport").FirstOrDefault().Val_c;

            //pmodel.exitport = c.ContractConditions.Where(x => x.Condition.Code.Trim() == "exitport").FirstOrDefault().Val_c;

            //pmodel.route = c.ContractConditions.Where(x => x.Condition.Code.Trim() == "route").FirstOrDefault().Val_c;

            //var htmlContent = RenderRazorViewToString("generatepdf_ch", pmodel);

            //var pdfgen = new NReco.PdfGenerator.HtmlToPdfConverter();


            ////pdfgen.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
            //pdfgen.Orientation = NReco.PdfGenerator.PageOrientation.Portrait;
            //pdfgen.Size = NReco.PdfGenerator.PageSize.A4;

            //var pdfBytes = pdfgen.GeneratePdf(htmlContent);

            //Response.Clear();
            //Response.ContentType = "application/pdf";
            //Response.AddHeader("content-disposition", string.Format("attachment;filename=\"polis{0}.pdf\"", c.contractnumber));

            //Response.OutputStream.Write(pdfBytes, 0, pdfBytes.Count());

            //Response.End();
        }

//        public void test()
//        {
//            var TxtHtmlCode = @"<html>
// <body>
//  Hello World from selectpdf.com.
// </body>
//</html>
//";

//            var converter = new HtmlToPdf();

//            var doc = converter.ConvertHtmlString(TxtHtmlCode);

//            // save pdf document
//            var pdfBuf = doc.Save();


//            Response.Clear();
//            Response.ContentType = "application/pdf";
//            Response.AddHeader("content-disposition", "attachment;filename=\"poliscrm.pdf\"");

//            Response.OutputStream.Write(pdfBuf, 0, pdfBuf.Count());

//            Response.End();

//            // close pdf document
//            doc.Close();
//        }

        public void printcrm(Guid contractid)
        {
            var c = db.Contracts.Include("Contract_territory")
                .Include("ContractConditions")
                .Include("Subjects")
                .Include("ContractRisks")
                .Include("ContractStatu")
                .SingleOrDefault(x => x.ContractId == contractid);
           
            new ContractService().OutputPdf(
                RenderRazorViewToString("printcrm", c),
                string.Format("polis_crm{0}", c.contractnumber));

            //var htmlContent = RenderRazorViewToString("printcrm", c);

            //var converter = new HtmlToPdf();

            //converter.Options.PdfPageSize = PdfPageSize.A4;
            //converter.Options.MarginBottom = 10;
            //converter.Options.MarginLeft = 10;
            //converter.Options.MarginRight = 10;
            //converter.Options.MarginTop = 10;

            //var doc = converter.ConvertHtmlString(htmlContent);

            //// save pdf document
            //var pdfBuf = doc.Save();


            //Response.Clear();
            //Response.ContentType = "application/pdf";
            //Response.AddHeader("content-disposition", "attachment;filename=\"poliscrm.pdf\"");

            //Response.OutputStream.Write(pdfBuf, 0, pdfBuf.Count());

            //Response.End();

            //// close pdf document
            //doc.Close();
        }

        public void printag(Guid contractid)
        {
            var model = db.v_contract_ag.SingleOrDefault(x => x.ContractId == contractid);

            new ContractService().OutputPdf(
                RenderRazorViewToString("printag", model),
                string.Format("polisag{0}",model.contractnumber));


            
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

        private class factorgrp
        {
            public string ftype { get; set; }
            public string fvalue { get; set; }
            public decimal qnt { get; set; }
        }
    }
}