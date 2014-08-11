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
    public class ContractsTestController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: Contracts
        public ActionResult Index()
        {
            var contracts = db.Contracts.Include(c => c.Currency).Include(c => c.seria);
            return View(contracts.ToList());
        }

        // GET: Contracts/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contract contract = db.Contracts.Find(id);
            if (contract == null)
            {
                return HttpNotFound();
            }
            return View(contract);
        }

        // GET: Contracts/Create
        public ActionResult Create()
        {
            ViewBag.currencyid = new SelectList(db.Currencies, "CurrencyId", "name");
            ViewBag.seriaid = new SelectList(db.serias, "SeriaId", "Code");
            return View();
        }

        // POST: Contracts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ContractId,currencyid,contractnumber,seriaid,date_begin,date_end,date_diff")] Contract contract)
        {
            if (ModelState.IsValid)
            {
                contract.ContractId = Guid.NewGuid();
                db.Contracts.Add(contract);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.currencyid = new SelectList(db.Currencies, "CurrencyId", "name", contract.currencyid);
            ViewBag.seriaid = new SelectList(db.serias, "SeriaId", "Code", contract.seriaid);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contract contract = db.Contracts.Find(id);
            if (contract == null)
            {
                return HttpNotFound();
            }
            ViewBag.currencyid = new SelectList(db.Currencies, "CurrencyId", "name", contract.currencyid);
            ViewBag.seriaid = new SelectList(db.serias, "SeriaId", "Code", contract.seriaid);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ContractId,currencyid,contractnumber,seriaid,date_begin,date_end,date_diff")] Contract contract)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contract).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.currencyid = new SelectList(db.Currencies, "CurrencyId", "name", contract.currencyid);
            ViewBag.seriaid = new SelectList(db.serias, "SeriaId", "Code", contract.seriaid);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contract contract = db.Contracts.Find(id);
            if (contract == null)
            {
                return HttpNotFound();
            }
            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Contract contract = db.Contracts.Find(id);
            db.Contracts.Remove(contract);
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
