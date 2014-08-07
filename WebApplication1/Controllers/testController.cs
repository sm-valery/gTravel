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
    public class testController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: test
        public ActionResult Index()
        {
            return View(db.serias.ToList());
        }

        // GET: test/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            seria seria = db.serias.Find(id);
            if (seria == null)
            {
                return HttpNotFound();
            }
            return View(seria);
        }

        // GET: test/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: test/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SeriaId,Code,Name")] seria seria)
        {
            if (ModelState.IsValid)
            {
                seria.SeriaId = Guid.NewGuid();
                db.serias.Add(seria);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(seria);
        }

        // GET: test/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            seria seria = db.serias.Find(id);
            if (seria == null)
            {
                return HttpNotFound();
            }
            return View(seria);
        }

        // POST: test/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SeriaId,Code,Name")] seria seria)
        {
            if (ModelState.IsValid)
            {
                db.Entry(seria).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(seria);
        }

        // GET: test/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            seria seria = db.serias.Find(id);
            if (seria == null)
            {
                return HttpNotFound();
            }
            return View(seria);
        }

        // POST: test/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            seria seria = db.serias.Find(id);
            db.serias.Remove(seria);
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
