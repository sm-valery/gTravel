using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gTravel.Models;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;

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

        private void Contract_ini()
        {
            ViewBag.currency = new SelectList(db.Currencies.ToList(), "currencyid", "code");
            ViewBag.territory = new SelectList(db.Territories.ToList(), "TerritoryId", "name");

        }

        public ActionResult Contract()
        {
            Contract_ini();

    
            return View();
        }

        public ActionResult Contract_create()
        {
            Contract_ini();

            Contract c = new Contract();

            c.ContractId = Guid.NewGuid();
            c.seriaid = Guid.Parse("00000000-0000-0000-0000-000000000000");
            c.date_begin = null;
            c.date_end = null;

            return View("Contract", c);
        }

        [HttpPost]
        public ActionResult Contract_create(Contract c)
        {
            if(ModelState.IsValid)
            {

                contract_before_save(ref c);

                db.Contracts.Add(c);
                db.SaveChanges();

                return RedirectToAction("List");
            }

            Contract_ini();
            return View("contract",c);
        }

        public ActionResult Contract_edit(Guid id)
        {
            var c = db.Contracts.Include("Contract_territory").SingleOrDefault(x => x.ContractId == id);
  
            Contract_ini();

            return View("contract",c);
        }

        [HttpPost]
        public ActionResult Contract_edit(Contract c, string[] territory)
        {
            if(ModelState.IsValid)
            {
                contract_before_save(ref c);
                
                //обновление территории
                contract_update_territory(c.ContractId, territory);

                db.Entry(c).State = EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction("List");
            }

            Contract_ini();
            return View("contract", c);
        }

        private void contract_update_territory(Guid contractid,string[] territory)
        {

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


        private int get_period_diff(DateTime? d1, DateTime? d2)
        {
            return (d2.Value - d1.Value).Days + 1;
        }

        public string get_strperiodday(string date_from, string date_to)
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

            return retval;
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