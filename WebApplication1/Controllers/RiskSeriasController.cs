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
    [Authorize(Roles = @"Admin")]
    public class RiskSeriasController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: RiskSerias
        public ActionResult Index()
        {
//TODO добавить таблицу RiskSeria

            var riskSerias = db.RiskSerias.Include(r => r.seria).Include(r => r.Risk).OrderBy(o=>o.SeriaId);
            return View(riskSerias.ToList());
        }

        // GET: RiskSerias/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RiskSeria riskSeria = db.RiskSerias.Find(id);
            if (riskSeria == null)
            {
                return HttpNotFound();
            }
            return View(riskSeria);
        }

        // GET: RiskSerias/Create
        public ActionResult Create()
        {
            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code");
            ViewBag.RiskId = new SelectList(db.Risks, "RiskId", "Code");
            return View();
        }

        // POST: RiskSerias/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RiskSeriaId,RiskId,SeriaId,isMandatory,sort")] RiskSeria riskSeria)
        {
            if (ModelState.IsValid)
            {
                riskSeria.RiskSeriaId = Guid.NewGuid();
                db.RiskSerias.Add(riskSeria);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code", riskSeria.SeriaId);
            ViewBag.RiskId = new SelectList(db.Risks, "RiskId", "Code", riskSeria.RiskId);
            return View(riskSeria);
        }

        // GET: RiskSerias/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RiskSeria riskSeria = db.RiskSerias.Find(id);
            if (riskSeria == null)
            {
                return HttpNotFound();
            }
            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code", riskSeria.SeriaId);
            ViewBag.RiskId = new SelectList(db.Risks, "RiskId", "Code", riskSeria.RiskId);

            ViewBag.TypeTarif = new SelectList(new[]
            {
                new SelectListItem {Text = "Тариф указывается на каждый день", Value = "0"},
                new SelectListItem {Text = "Тариф указывается на всю поездку", Value = "1"}
            }, "Value", "Text");


            return View(riskSeria);
        }

        // POST: RiskSerias/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RiskSeriaId,RiskId,SeriaId,isMandatory,sort,TypeTarif")] RiskSeria riskSeria)
        {
            if (ModelState.IsValid)
            {
                db.Entry(riskSeria).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code", riskSeria.SeriaId);
            ViewBag.RiskId = new SelectList(db.Risks, "RiskId", "Code", riskSeria.RiskId);
            return View(riskSeria);
        }

        // GET: RiskSerias/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RiskSeria riskSeria = db.RiskSerias.Find(id);
            if (riskSeria == null)
            {
                return HttpNotFound();
            }
            return View(riskSeria);
        }

        // POST: RiskSerias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            RiskSeria riskSeria = db.RiskSerias.Find(id);
            db.RiskSerias.Remove(riskSeria);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult RiskPrograms(Guid RiskSeriaId)
        {
            var rs = db.RiskSerias.SingleOrDefault(x => x.RiskSeriaId == RiskSeriaId);
            ViewBag.seria_code = rs.seria.Code;
            ViewBag.risk_code = rs.Risk.Code;
            ViewBag.RiskSeriaId = RiskSeriaId;

          
            return View(db.RiskPrograms.Where(x => x.RiskSeriaId == RiskSeriaId));
        }

        public ActionResult CreateRiskProgram(Guid RiskSeriaId)
        {
            var rs = db.RiskSerias.SingleOrDefault(x => x.RiskSeriaId == RiskSeriaId);
            ViewBag.seria_code = rs.seria.Code;
            ViewBag.risk_code = rs.Risk.Code;
            ViewBag.RiskSeriaId = RiskSeriaId;


            return View(new RiskProgram() {RiskSeriaId = RiskSeriaId});
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRiskProgram(RiskProgram rp)
        {
            if(ModelState.IsValid)
            {
                rp.RiskProgramId = Guid.NewGuid();

                db.RiskPrograms.Add(rp);
                db.SaveChanges();

                return RedirectToAction("RiskPrograms", new { RiskSeriaId =rp.RiskSeriaId});
            }

            var rs = db.RiskSerias.SingleOrDefault(x => x.RiskSeriaId == rp.RiskSeriaId);
            ViewBag.seria_code = rs.seria.Code;
            ViewBag.risk_code = rs.Risk.Code;
            ViewBag.RiskSeriaId = rp.RiskSeriaId;

            return View(rp);
        }

        public ActionResult EditRiskProgram(Guid id)
        {
            var rp= db.RiskPrograms.SingleOrDefault(x=>x.RiskProgramId==id);

            var rs = db.RiskSerias.SingleOrDefault(x => x.RiskSeriaId == rp.RiskSeriaId);
            ViewBag.seria_code = rs.seria.Code;
            ViewBag.risk_code = rs.Risk.Code;
        
            return View(rp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRiskProgram(RiskProgram rp)
        {
            if(ModelState.IsValid)
            {
                db.Entry(rp).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("RiskPrograms", new { RiskSeriaId = rp.RiskSeriaId });
            }

            var rs = db.RiskSerias.SingleOrDefault(x => x.RiskSeriaId == rp.RiskSeriaId);
            ViewBag.seria_code = rs.seria.Code;
            ViewBag.risk_code = rs.Risk.Code;
            ViewBag.RiskSeriaId = rp.RiskSeriaId;

            return View(rp);
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
