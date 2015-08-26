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
{     [Authorize(Roles = @"Admin")]
    public class AgentsController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: Agents

        public ActionResult Index(Guid? id)
        {
            var ag = from a in db.Agents select a;

            if(id.HasValue)
            {
                ag = ag.Where(x => x.AgentId == id);
            }

               

            return View(ag.Include("Agent2").OrderBy(o=>o.Name).ToList());
        }

        // GET: Agents/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agent agent = db.Agents.Find(id);
            if (agent == null)
            {
                return HttpNotFound();
            }
            return View(agent);
        }

        // GET: Agents/Create
        public ActionResult Create()
        {
            ViewBag.ParentId = new SelectList(db.Agents, "AgentId", "Name");

            return View();
        }

        // POST: Agents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AgentId,Name, ParentId")] Agent agent)
        {
            if (ModelState.IsValid)
            {
                agent.AgentId = Guid.NewGuid();
                db.Agents.Add(agent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(db.Agents, "AgentId", "Name");

            return View(agent);
        }

        // GET: Agents/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agent agent = db.Agents.Find(id);
            if (agent == null)
            {
                return HttpNotFound();
            
            }

            ViewBag.ParentId = new SelectList(db.Agents, "AgentId", "Name");

            ViewBag.AgentType = new SelectList(db.AddRefs.Where(x=>x.Code.Trim()=="subjtype"),"Value","AddRefsId")

            return View(agent);
        }

        // POST: Agents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AgentId,Name")] Agent agent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(agent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(agent);
        }

        // GET: Agents/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agent agent = db.Agents.Find(id);
            if (agent == null)
            {
                return HttpNotFound();
            }
            return View(agent);
        }

        // POST: Agents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Agent agent = db.Agents.Find(id);
            db.Agents.Remove(agent);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    [AllowAnonymous]
        [OutputCache(Duration = 86400, VaryByParam = "*")]
        public PartialViewResult _MenuAgentList(string sessionid , string userid)
        {
            var ag = mLib.GetCurrentUserAgent(userid);

            ViewBag.AgentId = new SelectList(db.Agents, "AgentId", "Name",ag.AgentId);

            return PartialView(ag);
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
