using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gTravel.Models;
using PagedList;

namespace gTravel.Controllers
{
    [Authorize(Roles = @"Admin")]
    public class SettingsController : Controller
    {
        private goDbEntities db = new goDbEntities();
        
       

       

        // GET: Settings
        public ActionResult Index()
        {
            return View();
        }

        #region Currency
        private void CurRateUpdate(DateTime? dt)
        {
            if (dt == null)
                dt = DateTime.Now;


        }
        
        public ActionResult Currency()
        {
            var viewdb = db.Currencies;

            return View(viewdb.ToList());
        }

        public ActionResult Currency_create()
        {
            Currency c = new Currency();

            return View(c);
        }

        [HttpPost]
        public ActionResult Currency_create(Currency c)
        {
            if(ModelState.IsValid)
            {
                c.CurrencyId = Guid.NewGuid();

                db.Currencies.Add(c);
                db.SaveChanges();

                return RedirectToAction("Currency");
            }

            return View(c);
        }

        public ActionResult Currency_edit(Guid CurrencyId)
        {
            Currency c = new Currency();

            return View(c);
        }


        public ActionResult Currate(int? page)
        {
            var pageNumber = page ?? 1;
            //https://github.com/TroyGoode/PagedList

            return View(db.CurRates.OrderByDescending(o => o.RateDate).ToPagedList(pageNumber, 25));
        }
        public ActionResult CurrateCreate()
        {

            ViewBag.RateDate = DateTime.Now.ToShortDateString();
            return View();
        }

        [HttpPost]
        public ActionResult CurrateCreate(DateTime RateDate)
        {
            CurrManage.updateCurRate(db, RateDate);

            return RedirectToAction("Currate");
        }

        #endregion
        
        #region seria
        public ActionResult seria()
        {
            return View(db.serias.ToList());
        }

        public ActionResult Seria_Create()
        {
            seria s = new seria();
           

            return View(s);
        }

        [HttpPost]
        public ActionResult Seria_Create(seria s)
        {

            if(ModelState.IsValid)
            {
                s.SeriaId = Guid.NewGuid();

                db.serias.Add(s);
                db.SaveChanges();

                return RedirectToAction("seria");
            }

            return View(s);
        }

        public ActionResult Seria_edit(Guid id)
        {
            ViewBag.DefaultCurrencyId = new SelectList(db.Currencies, "CurrencyId", "name");

            return View(db.serias.SingleOrDefault(x=>x.SeriaId == id));
        }

        [HttpPost]
        public ActionResult Seria_edit(seria s)
        {
            if(ModelState.IsValid)
            {
                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Seria");
            }

            ViewBag.DefaultCurrencyId = new SelectList(db.Currencies, "CurrencyId", "name");

            return View(s);
        }

        #endregion

        #region territory
        public ActionResult Territory()
        {
            return View(db.Territories.ToList());
        }

        public ActionResult Territory_Create()
        {
            Territory t = new Territory();

            return View(t);
        }

        [HttpPost]
        public ActionResult Territory_Create(Territory t)
        {
            if(ModelState.IsValid)
            {
                t.TerritoryId = Guid.NewGuid();

                db.Territories.Add(t);
                db.SaveChanges();

                return RedirectToAction("territory");
            }

            return View(t);
        }
        #endregion

        #region Condition
    
        public ActionResult condition()
        {

            return View(db.Conditions.ToList());
        }

        #endregion

        #region ConditionSeria
        public ActionResult ConditionSeria()
        {
            return View(db.ConditionSerias.OrderBy(o=>o.seria.Code).ToList());
        }

        public ActionResult ConditionSeriaCreate()
        {
            ConditionSeria cc = new ConditionSeria();

            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code");
            ViewBag.ConditionId = new SelectList(db.Conditions, "ConditionId", "Name");

            return View(cc);
        }

        [HttpPost]
        public ActionResult ConditionSeriaCreate(ConditionSeria cc)
        {
            if(ModelState.IsValid)
            {
                cc.ConditionSeriaId = Guid.NewGuid();

                db.ConditionSerias.Add(cc);
                db.SaveChanges();

                return RedirectToAction("ConditionSeria");
            }

            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code");
            ViewBag.ConditionId = new SelectList(db.Conditions, "ConditionId", "Name");

            return View(cc);
        }

#endregion

        #region AddRefs

        public ActionResult addrefs()
        {
            return View(db.AddRefs.OrderBy(x=>x.Code).ThenBy(x=>x.OrderNum).ToList());
        }

        public ActionResult AddRefCreate()
        {
            return View(new AddRef());
        }

        [HttpPost]
        public ActionResult AddRefCreate(AddRef add)
        {
            if (ModelState.IsValid)
            {


                add.AddRefsId = Guid.NewGuid();
                db.AddRefs.Add(add);

                db.SaveChanges();

                return RedirectToAction("addrefs");
            }

            return View(add);
        }
        #endregion



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