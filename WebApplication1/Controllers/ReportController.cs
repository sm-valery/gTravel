using gTravel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace gTravel.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private goDbEntities db = new goDbEntities();

        // GET: Report
        public ActionResult Index()
        {
            return View();
        }


        public class CollectedPremData
        {
            public decimal? insprem { get; set; }
            public decimal? insfee { get; set; }
            public decimal? contract_count { get; set; }
        }
        public ActionResult CollectedPrem()
        {

            return View(new List<CollectedPremData>());
        }


        [HttpPost]
        public ActionResult CollectedPrem(DateTime? dtFrom, DateTime? dtTo)
        {

            var viewrep = from r in db.v_contract
                          where r.date_begin >= dtFrom && r.date_out <= dtTo
                          group r by r.seriaid into g
                          select new CollectedPremData { insprem = g.Sum(a => a.InsPrem),
                          insfee = g.Sum(a=>a.InsFee),
                          contract_count = g.Count()
                          };

            ViewBag.dtFrom = dtFrom;
            ViewBag.dtFrom = dtTo;


            return View(viewrep.ToList());
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