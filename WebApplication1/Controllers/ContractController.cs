using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gTravel.Models;
using System.Data.Entity.Core.Objects;
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

        public ActionResult List()
        {

            return View(db.Contracts.ToList());
        }

        private void Contract_ini(Guid seria)
        {
            ViewBag.currency = new SelectList(db.Currencies.ToList(), "currencyid", "code");
            ViewBag.territory = new SelectList(db.Territories.ToList(), "TerritoryId", "name");

            ViewBag.Cond = db.ConditionSerias.Where(x => x.SeriaId == seria).ToList();
        }

        public ActionResult Contract()
        {
            //Contract_ini();


            return RedirectToAction("List");
        }

        public ActionResult Contract_create()
        {
            Contract_ini(Guid.Parse("00000000-0000-0000-0000-000000000000"));

            Contract c = new Contract();

            c.ContractId = Guid.NewGuid();
            c.seriaid = Guid.Parse("00000000-0000-0000-0000-000000000000");
            c.date_begin = null;
            c.date_end = null;

            var cs = db.ConditionSerias.Where(x => x.SeriaId == c.seriaid);

            foreach (var item in cs)
            {
                ContractCondition cc = new ContractCondition();
                cc.ContractCondId = Guid.NewGuid();
                cc.ConditionId = item.ConditionId;
                cc.Contractid = c.ContractId;
                cc.Condition = item.Condition;

                if (item.Condition.Type == "L")
                    cc.Val_l = false;

                c.ContractConditions.Add(cc);
            }


            return View("Contract", c);

        }

        [HttpPost]
        public ActionResult Contract_create(Contract contract, FormCollection oform)
        {
            if(ModelState.IsValid)
            {
                contract_before_save(ref contract);

                #region дополнительные параметры
                var cond = db.ConditionSerias.Where(x => x.SeriaId == contract.seriaid);
                foreach(var item in cond)
                {
                    ContractCondition cc = new ContractCondition();

                    if (item.Condition.Type == "L")
                    {
                        cc.Val_l = oform.GetValues("cond_" + item.Condition.Code.Trim()).Contains("true");
                    }

                    cc.ContractCondId = Guid.NewGuid();
                    cc.ConditionId = item.ConditionId;
                    cc.Contractid = contract.ContractId;
                    
                    contract.ContractConditions.Add(cc);
                }
                #endregion

                db.Contracts.Add(contract);


                db.SaveChanges();

                return RedirectToAction("List");
            }

            Contract_ini(Guid.Parse("00000000-0000-0000-0000-000000000000"));
            return View("contract",contract);
        }

        public ActionResult Contract_edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var c = db.Contracts.Include("Contract_territory").SingleOrDefault(x => x.ContractId == id);

            if(c == null)
                return HttpNotFound();

            Contract_ini(Guid.Parse("00000000-0000-0000-0000-000000000000"));

            return View("contract",c);
        }

        [HttpPost]
        public ActionResult Contract_edit(Contract c, string[] territory, FormCollection oform)
        {
            if(ModelState.IsValid)
            {
                contract_before_save(ref c);
                
                //обновление территории
                contract_update_territory(c.ContractId, territory);

                #region дополнительные параметры
                var cond = db.ContractConditions.Where(x => x.Contractid == c.ContractId);

                foreach (var item in cond)
                {
                    bool ischecked = oform.GetValues("cond_" + item.Condition.Code.Trim()).Contains("true");
                   
                    if(item.Val_l!=ischecked)
                    {
                        item.Val_l = ischecked;
                        db.Entry(item).State = EntityState.Modified;
                    }

                    
                }
                #endregion


                db.Entry(c).State = EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction("List");
            }

            Contract_ini(c.seriaid);
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

        public string contract_terr_insert_row(string id, string name)
        {

            return string.Format("<tr><td  class='input-value'> <input id='{2}' type='hidden' name='territory' value='{0}' /> {1}</td><td><button class='btn btn-default btn-sm'>x</button></td></tr>",
                id,name,"terr_" + id);
        }
        private int get_period_diff(DateTime? d1, DateTime? d2)
        {
            return (d2.Value - d1.Value).Days + 1;
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