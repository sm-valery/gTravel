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
    public class TarifsController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: Tarifs
        public ActionResult Index(Guid agentseriaid)
        {
            var tarifs = db.Tarifs.Include(t => t.Risk).Include(t=>t.Territory).Include(t=>t.seria).Where(x=>x.AgentSeriaId==agentseriaid)
                .OrderBy(o=>o.RiskId).ThenBy(o=>o.InsSumFrom).ThenBy(o=>o.PeriodFrom);

            ViewBag.agentseria = db.AgentSerias.SingleOrDefault(x => x.AgentSeriaId == agentseriaid);

            return View(tarifs.ToList());
        }


        public ActionResult Copy(Guid? id)
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

            Tarif t = new Tarif();
            t.TarifId = Guid.NewGuid();
            t.AgentSeriaId = tarif.AgentSeriaId;
            t.FranshPerc = tarif.FranshPerc;
            t.FranshSum = tarif.FranshSum;

            t.InsSumFrom = tarif.InsSumFrom;
            t.InsSumTo = tarif.InsSumTo;
            t.PremSum = 0;
            t.RiskId = tarif.RiskId;
            t.SeriaId = tarif.SeriaId;
            t.TerritoryId = tarif.TerritoryId;

            t.PeriodFrom = tarif.PeriodFrom;
            t.PeriodTo = tarif.PeriodTo;

            db.Tarifs.Add(t);
            db.SaveChanges();

            ViewData.Model = tarif;

            return RedirectToAction("edit", new { id = t.TarifId, editaction = "Копирование" });
        }

        // GET: Tarifs/Create
        public ActionResult Create(Guid agentseriaid)
        {

            var ags= db.AgentSerias.SingleOrDefault(x => x.AgentSeriaId == agentseriaid);

            ViewBag.agentseria=ags;

            var rser = from rs in db.RiskSerias 
                       join r in db.Risks on rs.RiskId equals r.RiskId
                       where rs.SeriaId == ags.SeriaId
                       select r;

            ViewBag.RiskId = new SelectList(rser, "RiskId", "Code");
           // ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code");
            ViewBag.TerritoryId = new SelectList(db.Territories, "TerritoryId", "Name");

            ViewBag.PeriodMultiType = new SelectList(new[]
            {
                new SelectListItem {Text = "Нет", Value = "0"},
                new SelectListItem {Text = "За весь период", Value = "1"},
                new SelectListItem {Text = "за одну поездку", Value = "2"}
            }, "Value", "Text"); 

            return View(new Tarif {TarifId=Guid.NewGuid(), AgentSeriaId =agentseriaid });
        }

        // POST: Tarifs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Tarif tarif, string editaction = "save")
        {
            var ags = db.AgentSerias.SingleOrDefault(x => x.AgentSeriaId == tarif.AgentSeriaId);


            if (ModelState.IsValid)
            {
                
                if (!tarif.InsFee.HasValue)
                    tarif.InsFee = tarif.PremSum;
                tarif.SeriaId = ags.SeriaId;

                db.Tarifs.Add(tarif);
                db.SaveChanges();


                if (editaction == "SaveCopy")
                    return RedirectToAction("Copy", new { id = tarif.TarifId });

                return RedirectToAction("Index", new { agentseriaid = tarif.AgentSeriaId });
            }

           
            ViewBag.agentseria = ags;

            var rser = from rs in db.RiskSerias
                       join r in db.Risks on rs.RiskId equals r.RiskId
                       where rs.SeriaId == ags.SeriaId
                       select r;

            ViewBag.RiskId = new SelectList(rser, "RiskId", "Code");
            ViewBag.TerritoryId = new SelectList(db.Territories, "TerritoryId", "Name");


            return View(tarif);
        }

        // GET: Tarifs/Edit/5
        public ActionResult Edit(Guid? id, string editaction="Редактирование")
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

            var ags = db.AgentSerias.SingleOrDefault(x => x.AgentSeriaId == tarif.AgentSeriaId);

            ViewBag.agentseria = ags;

            var rser = from rs in db.RiskSerias
                       join r in db.Risks on rs.RiskId equals r.RiskId
                       where rs.SeriaId == ags.SeriaId
                       select r;

            ViewBag.RiskId = new SelectList(rser, "RiskId", "Code");
            ViewBag.TerritoryId = new SelectList(db.Territories, "TerritoryId", "Name");

            ViewBag.action = editaction;

            ViewBag.PeriodMultiType = new SelectList(new[]
            {
                new SelectListItem {Text = "Нет", Value = "0"},
                new SelectListItem {Text = "За весь период", Value = "1"},
                new SelectListItem {Text = "за одну поездку", Value = "2"}
            }, "Value", "Text",tarif.RepeatedType); 

            return View(tarif);
        }

        // POST: Tarifs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tarif tarif,string editaction)
        {
            if (ModelState.IsValid)
            {
                if (!tarif.InsFee.HasValue)
                    tarif.InsFee = tarif.PremSum;

                db.Entry(tarif).State = EntityState.Modified;
                db.SaveChanges();
                

                if (editaction == "SaveCopy")
                    return RedirectToAction("Copy", new { id = tarif.TarifId });

                return RedirectToAction("Index", new { agentseriaid = tarif.AgentSeriaId });

            }
           
            var ags = db.AgentSerias.SingleOrDefault(x => x.AgentSeriaId == tarif.AgentSeriaId);

            ViewBag.agentseria = ags;

            var rser = from rs in db.RiskSerias
                       join r in db.Risks on rs.RiskId equals r.RiskId
                       where rs.SeriaId == ags.SeriaId
                       select r;

            ViewBag.RiskId = new SelectList(rser, "RiskId", "Code");
            ViewBag.TerritoryId = new SelectList(db.Territories, "TerritoryId", "Name");

            ViewBag.action = editaction;


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


        //#region тарифы план
        //public ActionResult tplan()
        //{

        //    return View(db.TarifPlans.ToList());
        //}
        //#endregion


        //public ActionResult tplan_edit(Guid id)
        //{
        //    ViewBag.TarifPlan = db.TarifPlans.SingleOrDefault(x => x.TarifPlanId == id);

        //    return View(db.Tarifs.Where(x => x.TarifPlanId == id).OrderBy(o=>o.seria.Code).ToList());
        //}

        //public ActionResult tplan_addrow(Guid TarifPlanId)
        //{
        //    var t = new Tarif();
        //    t.TarifPlanId = TarifPlanId;

        //    ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code",null);
        //    ViewBag.TerritoryId = new SelectList(db.Territories, "TerritoryId", "Name");

        //    return View(t);
        //}

        //public PartialViewResult _changeseria(Guid seriaid)
        //{

        //    var risklist = from rs in db.RiskSerias
        //             join r in db.Risks on rs.RiskId equals r.RiskId
        //             where rs.SeriaId == seriaid
        //             select r;

        //    ViewBag.RiskId = new SelectList(risklist, "RiskId", "Code");

        //    return PartialView();
        //}

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
