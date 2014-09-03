using System;
using System.Linq;
using System.Web.Mvc;
using gTravel.Models;
using System.Data.Entity;
using System.Net;

namespace gTravel.Controllers
{
    public class ContractController : Controller
    {
        private goDbEntities db = new goDbEntities();

        //
        // GET: /Contract/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List(decimal? contractnumber)
        {

            ViewBag.contractnumber = contractnumber;

            if (contractnumber != null)
                return View( db.Contracts.Where(x => x.contractnumber == contractnumber).ToList());

            return View(db.Contracts.ToList());
        }

        private void Contract_ini(Guid seria, Guid contract_id)
        {
            ViewBag.currency = new SelectList(db.Currencies.ToList(), "currencyid", "code");
            ViewBag.territory = new SelectList(db.Territories.ToList(), "TerritoryId", "name");

            ViewBag.risklist = db.v_contractrisk.Where(x => x.ContractId == contract_id).OrderBy(o => o.sort);
            //ViewBag.Cond = db.ConditionSerias.Where(x => x.SeriaId == seria).ToList();
        }

        public ActionResult Contract()
        {
            //Contract_ini();


            return RedirectToAction("List");
        }

        public ActionResult Contract_create()
        {
     
            #region Subject
            //c.Subject = new Subject();
            //c.Subject.Type = "fiz";
            Subject s = new Subject();

            s.SubjectId = Guid.NewGuid();
            s.Type = "fiz";

            db.Subjects.Add(s);
            #endregion

            #region contract
            Contract c = new Contract();

            c.ContractId = Guid.NewGuid();
            c.seriaid = Guid.Parse("00000000-0000-0000-0000-000000000000");
            c.date_begin = null;
            c.date_end = null;
            c.SubjectId = s.SubjectId;
            
            c.StatusId = db.Status.SingleOrDefault(x=>x.Code.Trim()=="project").StatusId;

            db.Contracts.Add(c);
            #endregion

            #region риски

            var cr = db.RiskSerias.Where(x => x.SeriaId == c.seriaid).OrderBy(o=>o.sort);

            foreach(var item in cr)
            {
                ContractRisk item_rs = new  ContractRisk();

                item_rs.ContractRiskId = Guid.NewGuid();
                item_rs.ContractId = c.ContractId;
                item_rs.RiskId = item.RiskId;
                item_rs.Risk = item.Risk;

                c.ContractRisks.Add(item_rs);
            }

            #endregion

            #region доп параметры
            var cs = db.ConditionSerias.Where(x => x.SeriaId == c.seriaid).OrderBy(o=>o.Condition.Code);

            foreach (var item in cs)
            {
                ContractCondition cc = new ContractCondition();
                cc.ContractCondId = Guid.NewGuid();
                cc.ConditionId = item.ConditionId;
                cc.Contractid = c.ContractId;
                cc.Condition = item.Condition;

                if (item.Condition.Type == "L")
                    cc.Val_l = false;

                db.ContractConditions.Add(cc);
            //    c.ContractConditions.Add(cc);
            }
            #endregion

            db.SaveChanges();

            Contract_ini(Guid.Parse("00000000-0000-0000-0000-000000000000"),c.ContractId);


            ViewBag.risklist = db.v_contractrisk.Where(x => x.ContractId == c.ContractId).OrderBy(o=>o.sort);

            return View("Contract", db.Contracts.SingleOrDefault(x=>x.ContractId==c.ContractId));

        }

        [HttpPost]
        public ActionResult Contract_create(Contract contract, FormCollection oform)
        {
            if(ModelState.IsValid)
            {
                contract_before_save(ref contract);

                #region дополнительные параметры
                foreach(var item in contract.ContractConditions)
                {
                    db.Entry(item).State = EntityState.Modified;
                }

                //var cond = db.ConditionSerias.Where(x => x.SeriaId == contract.seriaid);
                //foreach(var item in cond)
                //{
                //    ContractCondition cc = new ContractCondition();

                //    if (item.Condition.Type == "L")
                //    {
                //        cc.Val_l = oform.GetValues("cond_" + item.Condition.Code.Trim()).Contains("true");
                //    }

                //    cc.ContractCondId = Guid.NewGuid();
                //    cc.ConditionId = item.ConditionId;
                //    cc.Contractid = contract.ContractId;
                    
                 //   contract.ContractConditions.Add(cc);
                //}
                #endregion

                //db.Contracts.Add(contract);
                contract.Subject.SubjectId = contract.SubjectId.Value;
                db.Entry(contract.Subject).State = EntityState.Modified;
                
                db.Entry(contract).State = EntityState.Modified;
                //contract.Subject.SubjectId = Guid.NewGuid();
                //db.Subjects.Add(contract.Subject);

                db.SaveChanges();

                return RedirectToAction("List");
            }

            Contract_ini(Guid.Parse("00000000-0000-0000-0000-000000000000"),contract.ContractId);
            return View("contract",contract);
        }

        public ActionResult Contract_edit(Guid id)
        {
            var c = db.Contracts.Include("Contract_territory").Include("ContractConditions").Include("Subjects").SingleOrDefault(x => x.ContractId == id);
            c.ContractConditions = c.ContractConditions.OrderBy(o => o.Condition.Code).ToList();
            ViewBag.terr_count = c.Contract_territory.Count();

            if(c == null)
                return HttpNotFound();

            Contract_ini(Guid.Parse("00000000-0000-0000-0000-000000000000"),c.ContractId);

            return View("contract",c);
        }

        [HttpPost]
        public ActionResult Contract_edit(Contract c)
        {
            if(ModelState.IsValid)
            {
                contract_before_save(ref c);
                
                //обновление территории
                //contract_update_territory(c.ContractId, territory);

                #region дополнительные параметры
                foreach (var cond in c.ContractConditions)
                    db.Entry(cond).State = EntityState.Modified;

                //var cond = db.ContractConditions.Where(x => x.Contractid == c.ContractId);

                //foreach (var item in cond)
                //{
                //    bool ischecked = oform.GetValues("cond_" + item.Condition.Code.Trim()).Contains("true");

                //    if (item.Val_l != ischecked)
                //    {
                //        item.Val_l = ischecked;
                //        db.Entry(item).State = EntityState.Modified;
                //    }


                //}
                #endregion

                #region страхователь
                if (c.Subject != null)
                {
                    c.Subject.SubjectId = c.SubjectId.Value;
                    db.Entry(c.Subject).State = EntityState.Modified;
                }
                #endregion

                db.Entry(c).State = EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction("List");
            }

            Contract_ini(c.seriaid, c.ContractId);
            return View("contract", c);
        }



        private void contract_update_territory(Guid contractid,string[] territory)
        {
            if (territory == null)
                return;

            var t_old = db.Contract_territory.Where(x => x.ContractId == contractid).ToList();
            foreach(var item in t_old)
            {
               //удалить
            }

            foreach(string id in territory)
            {
                Guid tid= Guid.Parse(id);
                //добавить
                if(t_old.Where(x=>x.TerritoryId== tid).Count()==0 )
                {
                    var tnew = new Contract_territory();
                    
                    tnew.ContractTerritoryId = Guid.NewGuid();
                    tnew.TerritoryId=tid;
                    tnew.ContractId=contractid;

                    db.Contract_territory.Add(tnew);
                }

            }

           
        }

        private void contract_before_save(ref Contract c)
    {
        c.date_diff = get_period_diff(c.date_begin, c.date_end);
    }

        public ActionResult contract_terr_insert_row(Guid id, string name, Guid contractid)
        {
            if (db.Contract_territory.Any(x => x.ContractId==contractid && x.TerritoryId == id))
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
        private int get_period_diff(DateTime? d1, DateTime? d2)
        {
            if(d1.HasValue && d2.HasValue)
            return (d2.Value - d1.Value).Days + 1;

            return 0;
        }

        public ActionResult get_strperiodday(string date_from, string date_to)
        {
            DateTime d1=DateTime.Now, d2=DateTime.Now;
            bool isparsed;
            string retval = "0";

            isparsed= DateTime.TryParse(date_from, out d1);
            isparsed = isparsed && DateTime.TryParse(date_to, out d2);
            
            if(isparsed)
            {

                retval = get_period_diff(d1,d2).ToString();
            }

            return Content( retval);
        }


        public ActionResult _addInsuredRow(Guid contractid)
        {
            //if (db.Subjects.Any(x => x.ContractId == contractid && (x.Name1 == null || x.Name1 == "")))
            //    return null;

            Subject s = new Subject();
            s.SubjectId = Guid.NewGuid();
            s.ContractId = contractid;

            db.Subjects.Add(s);
            db.SaveChanges();

            ViewData["indx"] = db.Subjects.Where(x => x.ContractId == contractid).Count()-1;

            return PartialView(s);
        }

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