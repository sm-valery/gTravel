using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using gTravel.Models;

namespace gTravel.Controllers
{
    public class TarifsController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: Tarifs
        public ActionResult Index()
        {
            var tarifs = db.Tarifs.Include(t => t.Risk).Include(t=>t.Territory).Include(t=>t.seria);
            return View(tarifs.ToList());
        }

        // GET: Tarifs/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tarif tarif = db.Tarifs.Find(id);
            if (tarif == null)
            {
                return HttpNotFound();
            }
            return View(tarif);
        }

        // GET: Tarifs/Create
        public ActionResult Create()
        {
            ViewBag.RiskId = new SelectList(db.Risks, "RiskId", "Code");
            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code");
            ViewBag.TerritoryId = new SelectList(db.Territories, "TerritoryId", "Name");
            return View();
        }

        // POST: Tarifs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TarifId,RiskId,SeriaId,TerritoryId,InsSumFrom,InsSumTo,FranshSum,FranshPerc,PremSum,InsFee")] Tarif tarif)
        {
            if (ModelState.IsValid)
            {
                tarif.TarifId = Guid.NewGuid();
                db.Tarifs.Add(tarif);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RiskId = new SelectList(db.Risks, "RiskId", "Code", tarif.RiskId);
            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code",tarif.SeriaId);
            ViewBag.TerritoryId = new SelectList(db.Territories, "TerritoryId", "Name",tarif.TerritoryId);
            return View(tarif);
        }

        // GET: Tarifs/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tarif tarif = db.Tarifs.Find(id);
            if (tarif == null)
            {
                return HttpNotFound();
            }
            ViewBag.RiskId = new SelectList(db.Risks, "RiskId", "Code", tarif.RiskId);
            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code",tarif.SeriaId);
            ViewBag.TerritoryId = new SelectList(db.Territories, "TerritoryId", "Name",tarif.TerritoryId);
            return View(tarif);
        }

        // POST: Tarifs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TarifId,RiskId,SeriaId,TerritoryId,InsSumFrom,InsSumTo,FranshSum,FranshPerc,PremSum,InsFee")] Tarif tarif)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tarif).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RiskId = new SelectList(db.Risks, "RiskId", "Code", tarif.RiskId);
            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code",tarif.SeriaId);
            ViewBag.TerritoryId = new SelectList(db.Territories, "TerritoryId", "Name",tarif.TerritoryId);
            return View(tarif);
        }

        // GET: Tarifs/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tarif tarif = db.Tarifs.Find(id);
            if (tarif == null)
            {
                return HttpNotFound();
            }
            return View(tarif);
        }

        // POST: Tarifs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Tarif tarif = db.Tarifs.Find(id);
            db.Tarifs.Remove(tarif);
            db.SaveChanges();
            return RedirectToAction("Index");
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
