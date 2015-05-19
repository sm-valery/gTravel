using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using gTravel.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace gTravel.Controllers
{
    [Authorize(Roles = @"Admin")]
    public class AgentUsersController : Controller
    {

        public AgentUsersController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        private goDbEntities db = new goDbEntities();
        public UserManager<ApplicationUser> UserManager { get; private set; }


        public AgentUsersController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;

            //http://metanit.com/sharp/mvc5/12.13.php
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                //RequireUniqueEmail = true
            };


        }
        // GET: AgentUsers
        public ActionResult Index(Guid agentid)
        {
            var agentUsers = db.AgentUsers.Include(a => a.Agent).Include(a => a.AspNetUser).Where(x => x.AgentId == agentid);

            ViewBag.agent = db.Agents.SingleOrDefault(x=>x.AgentId == agentid);

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
        public ActionResult Create(Guid agentid)
        {
            //ViewBag.AgentId = new SelectList(db.Agents, "AgentId", "Name");
            //ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "UserName");

            var ag = db.Agents.SingleOrDefault(x => x.AgentId == agentid);

            var au = new AgentUser();
            au.AgentId = agentid;
            au.Agent = ag;
            au.AspNetUser = new AspNetUser();

            return View(au);
        }

        // POST: AgentUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AgentUser agentUser, string UserName, string UserPassword)
        {

            if (ModelState.IsValid)
            {
                //создаем пользователя
                var user = new ApplicationUser() { UserName = UserName };


                var result = UserManager.Create(user, UserPassword);

                if (result.Succeeded)
                {
                    agentUser.AgentUserId = Guid.NewGuid();
                    agentUser.UserId = user.Id;

                    db.AgentUsers.Add(agentUser);

                    db.SaveChanges();
                    return RedirectToAction("Index", new { agentid = agentUser.AgentId });
                }
                else
                {
                    foreach (var e in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, e);
                    }
                }


            }
            agentUser.Agent = db.Agents.SingleOrDefault(x => x.AgentId == agentUser.AgentId);

            //ViewBag.AgentId = new SelectList(db.Agents, "AgentId", "Name", agentUser.AgentId);
            //ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "UserName", agentUser.UserId);
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
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }
    }
}
