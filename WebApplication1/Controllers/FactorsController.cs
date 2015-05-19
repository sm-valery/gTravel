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
    public class FactorsController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: Factors
        public ActionResult Index(Guid agentseriaid)
        {
            ViewBag.agent = db.AgentSerias.SingleOrDefault(x => x.AgentSeriaId == agentseriaid);

            return View(db.Factors.Where(x => x.AgentSeriaId == agentseriaid)
                .OrderBy(o => o.RiskId)
                .ThenBy(o => o.Position).ToList());
        }

        // GET: Factors/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factor factor = db.Factors.Find(id);
            if (factor == null)
            {
                return HttpNotFound();
            }
            return View(factor);
        }

        private void factor_ini(Guid agentseriaid, Guid? riskid, Guid? territoryid)
        {
            var ags = db.AgentSerias.SingleOrDefault(x => x.AgentSeriaId == agentseriaid);
            ViewBag.agent = ags;

            var rser = (from rs in db.RiskSerias
                       join r in db.Risks on rs.RiskId equals r.RiskId
                       where rs.SeriaId == ags.SeriaId
                       select r).ToList();

            rser.Add(new Risk { Code = "", RiskId =Guid.Empty });
            if (!riskid.HasValue)
                riskid = Guid.Empty;

            ViewBag.RiskId = new SelectList(rser, "RiskId", "Code", riskid);

            if (!territoryid.HasValue)
                territoryid = Guid.Empty;

            ViewBag.TerritoryId = new SelectList(db.Territories.OrderBy(o => o.Name), "TerritoryId", "Name", territoryid);
        }

        private void factor_ini(Factor f)
        {
            factor_ini(f.AgentSeriaId.Value, f.RiskId, f.TerritoryId);
        }
         private void factor_ini(Guid agentseriaid)
        {
             factor_ini(agentseriaid, null,null);
        }

        // GET: Factors/Create
        public ActionResult Create(Guid agentseriaid)
        {
            factor_ini(agentseriaid);

            return View(new Factor { IdFactor = Guid.NewGuid(), AgentSeriaId = agentseriaid });
        }

        // POST: Factors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Factor factor)
        {

            if (create_factor(factor))
            {
                return RedirectToAction("Index", new { agentseriaid = factor.AgentSeriaId });
            }

            factor_ini(factor);

            return View(factor);
        }

        // GET: Factors/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factor factor = db.Factors.Find(id);
            if (factor == null)
            {
                return HttpNotFound();
            }

            factor_ini(factor);

            //ViewBag.vSingleItemInGroup = (factor.SingleItemInGroup == 1) ? true : false;
            //ViewBag.vauto = (factor.auto == 1) ? true : false;

            return View("Create", factor);
        }

        // POST: Factors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Factor factor)
        {

            if (ModelState.IsValid)
            {
                factor.before_save();

                db.Entry(factor).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", new { agentseriaid = factor.AgentSeriaId });
            }


            factor_ini(factor);

            return View("Create", factor);
        }




        public ActionResult Copy(Guid id)
        {
            var f = (Factor)db.Factors.SingleOrDefault(x => x.IdFactor == id);

            f.action = "Копирование";

            factor_ini(f);

            return View("Create", f);
        }
        [HttpPost]
        public ActionResult Copy(Factor factor)
        {

            if (create_factor(factor))
            {
                return RedirectToAction("Index", new { agentseriaid = factor.AgentSeriaId });
            }

            factor_ini(factor);

            return View(factor);
        }
        private bool create_factor(Factor factor)
        {
            bool ok = false;

            factor.before_save();


            if (ModelState.IsValid)
            {
                factor.IdFactor = Guid.NewGuid();

                db.Factors.Add(factor);
                db.SaveChanges();

                ok = true;
            }

            return ok;
        }

        // GET: Factors/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factor factor = db.Factors.Find(id);
            if (factor == null)
            {
                return HttpNotFound();
            }
            return View(factor);
        }

        // POST: Factors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Factor factor = db.Factors.Find(id);
            db.Factors.Remove(factor);
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
