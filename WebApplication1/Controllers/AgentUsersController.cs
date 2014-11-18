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
    public class AgentUsersController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: AgentUsers
        public ActionResult Index()
        {
            var agentUsers = db.AgentUsers.Include(a => a.Agent).Include(a => a.AspNetUser);
            return View(agentUsers.ToList());
        }

        // GET: AgentUsers/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AgentUser agentUser = db.AgentUsers.Find(id);
            if (agentUser == null)
            {
                return HttpNotFound();
            }
            return View(agentUser);
        }

        // GET: AgentUsers/Create
        public ActionResult Create()
        {
            ViewBag.AgentId = new SelectList(db.Agents, "AgentId", "Name");
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "UserName");
            return View();
        }

        // POST: AgentUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AgentUserId,AgentId,UserId,IsGlobalUser")] AgentUser agentUser)
        {
            if (ModelState.IsValid)
            {
                agentUser.AgentUserId = Guid.NewGuid();
                db.AgentUsers.Add(agentUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AgentId = new SelectList(db.Agents, "AgentId", "Name", agentUser.AgentId);
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "UserName", agentUser.UserId);
            return View(agentUser);
        }

        // GET: AgentUsers/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AgentUser agentUser = db.AgentUsers.Find(id);
            if (agentUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.AgentId = new SelectList(db.Agents, "AgentId", "Name", agentUser.AgentId);
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "UserName", agentUser.UserId);
            return View(agentUser);
        }

        // POST: AgentUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AgentUserId,AgentId,UserId,IsGlobalUser")] AgentUser agentUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(agentUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AgentId = new SelectList(db.Agents, "AgentId", "Name", agentUser.AgentId);
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "UserName", agentUser.UserId);
            return View(agentUser);
        }

        // GET: AgentUsers/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AgentUser agentUser = db.AgentUsers.Find(id);
            if (agentUser == null)
            {
                return HttpNotFound();
            }
            return View(agentUser);
        }

        // POST: AgentUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            AgentUser agentUser = db.AgentUsers.Find(id);
            db.AgentUsers.Remove(agentUser);
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
