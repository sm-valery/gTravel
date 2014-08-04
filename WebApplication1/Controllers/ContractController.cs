using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gTravel.Models;

namespace gTravel.Controllers
{
    public class ContractController : Controller
    {
        private goDbEntities db = new goDbEntities();

        //
        // GET: /Contract/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {

            return View();
        }

        private void Contract_ini()
        {
            ViewBag.currency = new SelectList(db.Currencies.ToList(), "currencyid", "code");
            ViewBag.territory = new SelectList(db.Territories.ToList(), "TerritoryId", "name");

        }

        public ActionResult Contract()
        {
            Contract_ini();

    
            return View();
        }

        public ActionResult Contract_create()
        {
            Contract_ini();

            Contract c = new Contract();

            return View("Contract", c);
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