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
            //var tarifs = db.Tarifs.Include(t=>t.Territory).Where(x=>x.AgentSeriaId==agentseriaid)
            //    .OrderBy(o=>o.RiskProgramId).ThenBy(o=>o.InsSumFrom).ThenBy(o=>o.PeriodFrom);

            ViewBag.agentseria = db.AgentSerias.SingleOrDefault(x => x.AgentSeriaId == agentseriaid);

            return View(db.v_Tarifs.Where(x=>x.AgentSeriaId==agentseriaid).OrderBy(x=>x.RiskProgramId).ThenBy(x=>x.InsSumFrom).ThenBy(x=>x.PeriodFrom).ToList());
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
            t.RiskProgramId = tarif.RiskProgramId;
            t.TerritoryId = tarif.TerritoryId;

            t.PeriodFrom = tarif.PeriodFrom;
            t.PeriodTo = tarif.PeriodTo;

            db.Tarifs.Add(t);
            db.SaveChanges();

            ViewData.Model = tarif;

            return RedirectToAction("edit", new { id = t.TarifId, editaction = "Копирование" });
        }

        private void tarifini(Guid agentseriaid,string action)
        {
            tarifini(agentseriaid,action, null);
        }
        private void tarifini(Guid agentseriaid,string action, Guid? RiskProgramId)
        {
            var ags = db.AgentSerias.SingleOrDefault(x => x.AgentSeriaId == agentseriaid);

            ViewBag.agentseria = ags;

            var rser = from rs in db.RiskSerias
                       join r in db.Risks on rs.RiskId equals r.RiskId
                       where rs.SeriaId == ags.SeriaId
                       select r;

            if (RiskProgramId.HasValue)
            {
                var riskid = (from r in db.RiskPrograms
                              join rs in db.RiskSerias on r.RiskSeriaId equals rs.RiskSeriaId
                              where r.RiskProgramId == RiskProgramId
                              select rs).SingleOrDefault().RiskId;

                ViewBag.RiskId = new SelectList(rser, "RiskId", "Code", riskid);
            }
            else
                ViewBag.RiskId = new SelectList(rser, "RiskId", "Code");

            ViewBag.TerritoryId = new SelectList(db.Territories, "TerritoryId", "Name");


            ViewBag.PeriodMultiType = new SelectList(new[]
            {
                new SelectListItem {Text = "Нет", Value = "0"},
                new SelectListItem {Text = "Многократная. Каждая из поездок не больше", Value = "1"},
                new SelectListItem {Text = "Многократная. Всего поездок не больше", Value = "2"}
            }, "Value", "Text");

            ViewBag.action = action;
        }

        // GET: Tarifs/Create
        public ActionResult Create(Guid agentseriaid)
        {

            tarifini(agentseriaid,"create");

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


                db.Tarifs.Add(tarif);
                db.SaveChanges();


                if (editaction == "SaveCopy")
                    return RedirectToAction("Copy", new { id = tarif.TarifId });

                return RedirectToAction("Index", new { agentseriaid = tarif.AgentSeriaId });
            }


            tarifini(ags.AgentSeriaId,editaction,tarif.RiskProgramId);


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

            tarifini((Guid)tarif.AgentSeriaId, editaction, tarif.RiskProgramId);

            return View("Create", tarif);
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

            tarifini((Guid)tarif.AgentSeriaId, editaction, tarif.RiskProgramId);


            return View("Create",tarif);
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

        public PartialViewResult _RiskProgList(Guid riskid, Guid seriaid, Guid? RiskProgramId)
        {
            mLib.NoAjaxCache();

            var rs = db.RiskSerias.SingleOrDefault(x=>x.SeriaId==seriaid && x.RiskId==riskid);

            if(RiskProgramId.HasValue)
                ViewBag.RiskProgramId = new SelectList(db.RiskPrograms.Where(x => x.RiskSeriaId == rs.RiskSeriaId), "RiskProgramId", "Name", RiskProgramId); 
            else
                ViewBag.RiskProgramId = new SelectList(db.RiskPrograms.Where(x => x.RiskSeriaId == rs.RiskSeriaId), "RiskProgramId", "Name"); 

            return PartialView();
        }


        public ActionResult TarifPlan()
        {

            return View(db.TarifPlans);
        }

        public ActionResult TarifPlanList(Guid tarifplanid)
        {

            return View();
        }
        
        public ActionResult TarifPlanCreate()
        {
            ViewBag.seriaid = new SelectList(db.serias, "SeriaId", "Code");

            return View();
        }



        public ActionResult TarifPlanEdit(Guid id)
        {
            var tp = db.TarifPlans.SingleOrDefault(x => x.TarifPlanId == id);
            if (tp == null)
            {
                return HttpNotFound();
            }

            ViewBag.seriaid = new SelectList(db.serias, "SeriaId", "Code",tp.SeriaId);

            return View(tp);
        }

        [HttpPost]
        public ActionResult TarifPlanEdit(TarifPlan tp)
        {

            if(ModelState.IsValid)
            {
                db.Entry(tp).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("TarifPlan");
            }

            ViewBag.seriaid = new SelectList(db.serias, "SeriaId", "Code", tp.SeriaId);

            return View(tp);
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
