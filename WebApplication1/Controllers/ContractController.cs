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
using Microsoft.AspNet.Identity;
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

            var clist = db.spContract(userid, contractnumber, ImportLogId, null, borderoid).ToList();

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

        [ChildActionOnly]
        [OutputCache(Duration = 60, VaryByParam = "*")]
        public PartialViewResult _RiskProgram(Guid riskid, Guid seriaid, Guid? progid, int idx)
        {
            var riskprog = from rs in db.RiskSerias
                           join rp in db.RiskPrograms on rs.RiskSeriaId equals rp.RiskSeriaId
                           where rs.RiskId == riskid
                           && rs.SeriaId == seriaid
                           orderby rp.Num
                           select rp;

            if(progid.HasValue)
                ViewBag.RiskProgramId = new SelectList(riskprog, "RiskProgramId", "ProgramCode", (Guid)progid);
            else
             ViewBag.RiskProgramId = new SelectList(riskprog, "RiskProgramId", "ProgramCode");
               
            ViewBag.idx = idx;

            return PartialView();
        }




        [ChildActionOnly]
        public PartialViewResult _RiskFranchise(Guid seriaid, Guid riskid, decimal? FranshPerc, int idx)
        {
          if(!new ContractService().riskhasfranchise(seriaid,riskid))
                return null;

            ViewBag.idx = idx;
            ViewBag.FranshPerc = (FranshPerc.HasValue)?FranshPerc:0;

            return PartialView();

        }

        public string _print_currate(DateTime cdate, Guid curid)
        {


            return CurrManage.getCurRate(db, curid, cdate).ToString();
        }

        public string _program_changed(Guid programid)
        {
            string ss = "";

            var prg = db.RiskPrograms.FirstOrDefault(x => x.RiskProgramId==programid);

            if (prg != null && prg.DefaultInsSum > 0)
                ss = prg.DefaultInsSum.ToString();

            return ss;
        }


        [HttpPost]
        public PartialViewResult _addAgentRow(Guid contractid)
        {
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());


            ContractService cs = new ContractService();
            ViewBag.idx = cs.ContractAgent_Count(contractid);

            var vagents = db.Agents.OrderBy(x=>x.Name).ToList();
            vagents.Add(new Agent() { AgentId = Guid.Empty, Name = "не выбран" });
            ViewBag.agentlist = new SelectList(vagents, "AgentId", "Name");

           return PartialView(cs.Create_ContractAgent(contractid));
        }

        [ChildActionOnly]
        public PartialViewResult _AgentList(Guid contractid)
        {
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());


            var vagents = db.Agents.OrderBy(x=>x.Name).ToList();
            vagents.Add(new Agent() { AgentId = Guid.Empty, Name = "не выбран" });
            ViewBag.agentlist = new SelectList(vagents, "AgentId", "Name");

            return PartialView(db.ContractAgents.Where(x => x.ContractId == contractid).ToList());
        }

        [HttpPost]
        public ActionResult _removeAgentRow(Guid contractagentid)
        {

            var ca = db.ContractAgents.SingleOrDefault(x => x.ContractAgentId == contractagentid);

            if (ca == null)
                return HttpNotFound();

            db.ContractAgents.Remove(ca);
            db.SaveChanges();

            return null;
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
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());

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

        private void ContractForm_ini(Contract c,string currentuserid)
        {
            ViewBag.currency = new SelectList(db.Currencies.ToList(), "currencyid", "code",c.currencyid);

            //ViewBag.risklist = db.v_contract_risk.Where(x => x.ContractId == contract_id).OrderBy(o => o.sort);

            ViewBag.PeriodMultiType = new SelectList(new[]
            {
                new SelectListItem {Text = "Разовая поездка", Value = "0"},
                new SelectListItem {Text = "Многократная. Каждая из поездок не больше", Value = "1"},
                new SelectListItem {Text = "Многократная. Всего поездок не больше", Value = "2"}
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

            ViewBag.isic = mLib.AgentInRole(currentuserid, "IC");

            ViewBag.RiskSeria = db.RiskSerias.Where(x => x.SeriaId == c.seriaid).ToList();
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
            ContractForm_ini(c, userid);

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
        public ActionResult ContractTo(Contract c, string userid, string caction = "save", string import_assured="")
        {
            var errMess = new List<string>();
            var cs = new ContractService(db);

            //очистить застрахованных от удаленных
            c.SubjectClearDeleted();

            c.db = db;

            ContractSave(c);

            if (caction == "import" && !string.IsNullOrEmpty(import_assured))
                cs.import_assured(c, import_assured);

          
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
                {
                    ModelState.AddModelError(string.Empty, "Дата рождения страхователя не заполнена!");
                }
                else
                {
                    if (mLib.get_age(c.Subject.DateOfBirth.Value, DateTime.Now) < 18)
                        ModelState.AddModelError(string.Empty, "Страхователь не может быть моложе 18 лет");
                }
                   
                if (c.date_begin < c.date_out)
                    ModelState.AddModelError(string.Empty, "Дата начала договора не может быть меньше даты выдачи!");

                if (c.tripduration > c.date_diff)
                    ModelState.AddModelError(string.Empty, "Срок поездки не может быть больше периода страхования!");



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
            ContractForm_ini(c,userid);



            return View(cs.GetContractForEdit(c.ContractId,userid));
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

            ContractForm_ini(c,userid);

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
                        where //tr.SeriaId == c.seriaid &&
                              tr.TerritoryId == cter.TerritoryId 
                            //  &&                              tr.RiskId == crisk.RiskId
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

            c.date_diff = mLib.get_period_diff(c.date_begin, c.date_end);

            c.tripduration = (c.tripduration.HasValue && c.tripduration!=0) ? c.tripduration : c.date_diff;

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

            #region Агенты

            foreach(var ag in c.ContractAgents)
            {
                if(ag.num!=-1)
                    db.Entry(ag).State = EntityState.Modified;
            }

            //если агенты не указаны, добавим агента пользователя
            if(c.ContractAgents.Count==0)
            {
                var caguser = new ContractAgent();

                caguser.ContractAgentId = Guid.NewGuid();
                caguser.ContractId = c.ContractId;
                caguser.num = 1;
                caguser.Percent = 100;

                 caguser.AgentId = mLib.GetCurrentUserAgent(c.UserId).AgentId;

                db.ContractAgents.Add(caguser);
            }
            #endregion

            #region территория
            var first_terr = c.Contract_territory.FirstOrDefault();
            decimal? terr_recsum =0;

            if(first_terr!=null)
            {
                terr_recsum = db.Territories.SingleOrDefault(x => x.TerritoryId == first_terr.TerritoryId).RecomInsSum;
            }
         

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
                    { 
                        r.ischecked = true;

                        r.InsSum = ((!r.InsSum.HasValue || r.InsSum == 0) 
                            && terr_recsum.HasValue) ? terr_recsum : r.InsSum;
                   }

                    db.Entry(r).State = EntityState.Modified;
                }

                #endregion

                db.Entry(c).State = EntityState.Modified;

                db.SaveChanges();
            }

            return ret;
        }

      

        [UserIdFilter]
        [ContractUserFilter]
        public ActionResult Contract_edit(Guid contractid, string userid)
        {


            Contract c = new ContractService(db).GetContractForEdit(contractid,userid);

            ContractForm_ini(c,userid);


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


             
            }


            ViewBag.settings = db.import_settings.OrderBy(x => x.numcol).ToList();

            var viewlist = db.v_importlog.Where(x => x.userid == userid).OrderByDescending(o => o.dateinsert);

            return View((PagedList<v_importlog>) viewlist.ToPagedList(1, 10));
        }

        public void importexapmle()
        {
         new ContractService(db).GetImportExampleFile();
        }

      


        private void importOledb()
        {
           
        }

        

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


            return Content(ret);
        }

        [UserIdFilter]
        public ActionResult _addInsuredRow(string contractid, int indx, string fieldlist, string ins_name, string dayob, string pasport, string userid)
        {
            //это чтоб работало в ie, иначе ajax запросы будут кешироваться
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());

            var gContractId = Guid.Parse(contractid);


        
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

            s.Pasport = pasport;

            ViewBag.viewonly = false;




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
      
        }

        public PartialViewResult _ContractRiskInsSum(Guid seriaid, Guid riskid, int risk_num, decimal riskinssum)
        {
            var agentid = mLib.GetCurrentUserAgent(User.Identity.GetUserId());



            return PartialView();
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
       
        }


        [UserIdFilter]
        public void print01(Guid contractid,string userid, decimal blanktype=1)
        {
            var c = db.Contracts.SingleOrDefault(x => x.ContractId == contractid);

            bool extrim = db.ContractFactors.Any(x => x.ContractId == contractid && x.Factor.FactorType.Trim()=="extrim");

            ViewBag.PrintMess = "";
            if (extrim)
                ViewBag.PrintMess = "Включены риски, связанные с занятием спортом, за исключением скалолазание, каньониг, катание на немаркированных трассах(в т числе фрирайд),heliski.";

            var contractagent = db.AgentUsers.SingleOrDefault(x => x.UserId == c.UserId);

            ViewBag.HidePremium = false;

            if(contractagent!=null)
            {
                var agser = db.AgentSerias.SingleOrDefault(x => x.AgentId == contractagent.AgentId
                    && x.SeriaId == c.seriaid);

                if (agser != null)
                    ViewBag.HidePremium = agser.HidePremium;
            }
               

            StringBuilder territory_string = new StringBuilder();
            foreach (var t in c.Contract_territory)
            {
                territory_string.Append(t.Territory.Name + " ");
            }
            ViewBag.territory_string = territory_string.ToString();

            string viewname = (blanktype == 0) ? "print01blank" : "print01";
            string filename = string.Format("{1}01_{0}.pdf", c.contractnumber,
                (blanktype == 0) ? "blank" : "polis");

            ViewBag.agentname = mLib.GetCurrentUserAgent(userid).Name;

            new ContractService().OutputPdf(
                RenderRazorViewToString(viewname, c),
                filename
               );

        }

         [UserIdFilter]
        public void print02(Guid contractid, string userid, decimal blanktype = 1)
        {
            

            var c = db.Contracts.SingleOrDefault(x => x.ContractId == contractid);

            var risk = c.ContractRisks.FirstOrDefault();


            using(ContractService cs= new ContractService())
            {
                

                ViewBag.RisksPrintList = cs.get_diver_risksum(c.tripduration, risk.RiskProgram.ProgramCode.Trim(), c.Currency.code);

                StringBuilder territory_string = new StringBuilder();
                foreach (var t in c.Contract_territory)
                {
                    territory_string.Append(t.Territory.Name + " ");
                }
                ViewBag.territory_string = territory_string.ToString();

                bool extrim = db.ContractFactors.Any(x => x.ContractId == contractid && x.Factor.FactorType.Trim() == "extrim");

                bool techno = db.ContractFactors.Any(x => x.ContractId == contractid && x.Factor.FactorType.Trim() == "technodive");

                string printmess = "в страховое покрытие не включены никакие дополнительные условия";

                if (extrim)
                    printmess = "спорт за исключением скалолазание, каньонинг, катание на немаркированных трассах (в т.ч. фрирайд), бейсджампинг, heliski . Включены риски, связанные с ездой на мототранспортных средствах";

                if (techno)
                    printmess = "в страховое покрытие включены риски, связанные с занятием техно-дайвингом.";

                ViewBag.PrintMess = printmess;

              ViewBag.agentname  = mLib.GetCurrentUserAgent(userid).Name;

              string viewname = (blanktype == 0) ? "print02blank" : "print02";
              string filename = string.Format("{1}02_{0}.pdf", c.contractnumber,
                  (blanktype == 0) ? "blank" : "polis");


                cs.OutputPdf(
                    RenderRazorViewToString(viewname, c),
                    string.Format(filename, c.contractnumber));
            }


        }

        [UserIdFilter]
         public void print03(Guid contractid, string userid, decimal blanktype = 1)
         {

             var c = db.Contracts.SingleOrDefault(x => x.ContractId == contractid);

             ViewBag.vzrnumber = c.ContractConditions.SingleOrDefault(x => x.Condition.Code.Trim() == "vzrnum").Val_c;

             StringBuilder territory_string = new StringBuilder();
             foreach (var t in c.Contract_territory)
             {
                 territory_string.Append(t.Territory.Name + " ");
             }
             ViewBag.territory_string = territory_string.ToString();

             string viewname = (blanktype == 0) ? "print03blank" : "print03";
             string filename = string.Format("{1}03_{0}.pdf", c.contractnumber,
                 (blanktype == 0) ? "blank" : "polis");


             new ContractService().OutputPdf(
                 RenderRazorViewToString(viewname, c),
                 string.Format(filename, c.contractnumber));
         }

        //public void printcrm(Guid contractid)
        //{
        //    var c = db.Contracts.Include("Contract_territory")
        //        .Include("ContractConditions")
        //        .Include("Subjects")
        //        .Include("ContractRisks")
        //        .Include("ContractStatu")
        //        .SingleOrDefault(x => x.ContractId == contractid);
           
        //    new ContractService().OutputPdf(
        //        RenderRazorViewToString("printcrm", c),
        //        string.Format("polis_crm{0}", c.contractnumber));

        //    //var htmlContent = RenderRazorViewToString("printcrm", c);

        //    //var converter = new HtmlToPdf();

        //    //converter.Options.PdfPageSize = PdfPageSize.A4;
        //    //converter.Options.MarginBottom = 10;
        //    //converter.Options.MarginLeft = 10;
        //    //converter.Options.MarginRight = 10;
        //    //converter.Options.MarginTop = 10;

        //    //var doc = converter.ConvertHtmlString(htmlContent);

        //    //// save pdf document
        //    //var pdfBuf = doc.Save();


        //    //Response.Clear();
        //    //Response.ContentType = "application/pdf";
        //    //Response.AddHeader("content-disposition", "attachment;filename=\"poliscrm.pdf\"");

        //    //Response.OutputStream.Write(pdfBuf, 0, pdfBuf.Count());

        //    //Response.End();

        //    //// close pdf document
        //    //doc.Close();
        //}

        //public void printag(Guid contractid)
        //{
        //    var model = db.v_contract_ag.SingleOrDefault(x => x.ContractId == contractid);

        //    new ContractService().OutputPdf(
        //        RenderRazorViewToString("printag", model),
        //        string.Format("polisag{0}",model.contractnumber));


            
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