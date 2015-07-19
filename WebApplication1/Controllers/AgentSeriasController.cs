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
    public class AgentSeriasController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: AgentSerias
        public ActionResult Index(Guid agentid)
        {
            var agentSerias = db.AgentSerias.Include(a => a.seria).Include(a => a.TerritoryGrp).Where(x=>x.AgentId==agentid);

            ViewBag.agent = db.Agents.SingleOrDefault(x => x.AgentId == agentid);

            ViewBag.arole = db.AgentRoles.Where(x => x.AgentId == agentid).ToList();

            return View(agentSerias.ToList());
        }

        [HttpPost]
        public ActionResult _addRole(Guid agentid)
        {
            var agrole = new AgentRole();
            agrole.AgentId = agentid;
            agrole.AgentRoleId = Guid.NewGuid();

            db.AgentRoles.Add(agrole);
            db.SaveChanges();

            return PartialView(agrole);
        }

        public ActionResult _saveRole(Guid pk,string value)
        {
            var agrole = db.AgentRoles.SingleOrDefault(x => x.AgentRoleId == pk);

            if(agrole!=null)
            {
                agrole.RoleCode = value;
                db.Entry(agrole).State = EntityState.Modified;

                db.SaveChanges();
            }

            return null;
        }

        public ActionResult EditAgentNumber(Guid pk,string value)
        {
            var ag = db.Agents.SingleOrDefault(x => x.AgentId == pk);

            if(ag==null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ag.AgentContractNum = value;

            db.Entry(ag).State = EntityState.Modified;
            db.SaveChanges();

            return null;
        }
        public ActionResult EditAgentDate(Guid pk, DateTime? value)
        {
            var ag = db.Agents.SingleOrDefault(x => x.AgentId == pk);

            if (ag == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ag.AgentContractDate = value;

            db.Entry(ag).State = EntityState.Modified;
            db.SaveChanges();

            return null;
        }
        
        // GET: AgentSerias/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AgentSeria agentSeria = db.AgentSerias.Find(id);
            if (agentSeria == null)
            {
                return HttpNotFound();
            }
            return View(agentSeria);
        }

        // GET: AgentSerias/Create
        public ActionResult Create(Guid agentid)
        {
            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code");

            ViewBag.TerritoryGrpId = new SelectList(db.TerritoryGrps, "TerritoryGrpId", "Code");

            ViewBag.Agent = db.Agents.SingleOrDefault(x => x.AgentId == agentid);

            return View(new AgentSeria { AgentId = agentid});
        }

        // POST: AgentSerias/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AgentSeria1,AgentId,SeriaId,TerritoryGrpId")] AgentSeria agentSeria)
        {
            if (ModelState.IsValid)
            {
                agentSeria.AgentSeriaId = Guid.NewGuid();
                db.AgentSerias.Add(agentSeria);
                db.SaveChanges();
                return RedirectToAction("Index", new { agentid   = agentSeria.AgentId});
            }

            ViewBag.Agent = db.Agents.SingleOrDefault(x => x.AgentId == agentSeria.AgentId);

            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code", agentSeria.SeriaId);
            ViewBag.TerritoryGrpId = new SelectList(db.TerritoryGrps, "TerritoryGrpId", "Code", agentSeria.TerritoryGrpId);
            return View(agentSeria);
        }

        // GET: AgentSerias/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AgentSeria agentSeria = db.AgentSerias.Find(id);
            if (agentSeria == null)
            {
                return HttpNotFound();
            }
            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code", agentSeria.SeriaId);
            ViewBag.TerritoryGrpId = new SelectList(db.TerritoryGrps, "TerritoryGrpId", "Code", agentSeria.TerritoryGrpId);

           

            return View(agentSeria);
        }

        // POST: AgentSerias/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( AgentSeria agentSeria)
        {
            if (ModelState.IsValid)
            {
                db.Entry(agentSeria).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { agentid = agentSeria.AgentId });
            }
            ViewBag.SeriaId = new SelectList(db.serias, "SeriaId", "Code", agentSeria.SeriaId);
            ViewBag.TerritoryGrpId = new SelectList(db.TerritoryGrps, "TerritoryGrpId", "Code", agentSeria.TerritoryGrpId);

            agentSeria.seria = db.serias.Find(agentSeria.SeriaId);

            return View(agentSeria);

        }

        // GET: AgentSerias/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AgentSeria agentSeria = db.AgentSerias.Find(id);
            if (agentSeria == null)
            {
                return HttpNotFound();
            }
            return View(agentSeria);
        }

        // POST: AgentSerias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            AgentSeria agentSeria = db.AgentSerias.Find(id);

            var agentid = agentSeria.AgentId;

            db.AgentSerias.Remove(agentSeria);
            db.SaveChanges();
            return RedirectToAction("Index", new { agentid = agentid });
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
