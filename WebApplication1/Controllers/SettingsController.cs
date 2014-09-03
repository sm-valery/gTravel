using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gTravel.Models;

namespace gTravel.Controllers
{
    public class SettingsController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: Settings
        public ActionResult Index()
        {
            return View();
        }

        #region Currency
        public ActionResult Currency()
        {

            return View(db.Currencies.ToList());
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